using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Stores everything necessary to load and play a single puzzle.  
/// </summary>
public class Puzzle
{
	public GameBoard Board { get; set; }
}

public static class PuzzleFactory
{
	public static bool TryGenerateNewPuzzle(PuzzleMakingParameters parameters, out Puzzle newPuzzle)
	{
		bool generatedPuzzle = false;
		newPuzzle = new Puzzle();
		newPuzzle.Board = new GameBoard();
		newPuzzle.Board.Initialize(parameters.BoardSize);

		int retryCount = parameters.RetryAttempts;
		if (retryCount <= 0 || retryCount > 1000) retryCount = 1000; //arbitrarily high amount of retries

		bool bSuccess = false;
		while (retryCount > 0 && bSuccess == false)
		{
			Random.InitState(parameters.RandomizationSeed + retryCount);
			newPuzzle.Board.ClearTiles();

			bSuccess =
				(TryPopulateMirrors(parameters, newPuzzle) &&
				 newPuzzle.Board.SetPaths() &&
				 LongestPathMatchesDifficulty(parameters, newPuzzle) &&
				 TryPopulateUniquePaths(parameters, newPuzzle) &&
				 TryFillRestWithRandom(parameters, newPuzzle) &&
				 EnsureSingleSolution(newPuzzle));

			--retryCount;
		}

		return generatedPuzzle;
	}

	private static bool TryPopulateMirrors(PuzzleMakingParameters parameters, Puzzle puzzle)
	{
		bool bSuccess = true;

		//the number of mirrors is calculated based on the min and max ratio of mirrors from the difficulty times the board size
		//ex: if the min/max range is .25->.75 and the board is 4x4 (16 tiles) then this will return a value between (inclusive) 4->12
		int mirrorCount = Mathf.RoundToInt((float)puzzle.Board.TileCount * Random.Range(parameters.DifficultyParameters.MirrorsRatioMin, parameters.DifficultyParameters.MirrorsRatioMax));

		//index isn't iterated here - we only iterated it when we successfully assign a mirror

		int bailOut = 0;
		for (int i = 0; i < mirrorCount; )
		{
			GameTile nextTile;
			if (puzzle.Board.TryGetGameTile(Random.Range(0, puzzle.Board.TileCount), out nextTile))
			{
				if (nextTile.ExpectedState == GameTileState.Empty)
				{
					if (Random.Range(0f, 1.0f) > 0.5f)
					{
						nextTile.SetExpectedState(GameTileState.MirrorBotLeftTopRight);
					}
					else
					{
						nextTile.SetExpectedState(GameTileState.MirrorTopLeftBotRight);
					}
					i++;
				}
			}

			bailOut++;
			if (bailOut > 10000) //arbitrarily high value to keep this from being an infinite loop
			{
				bSuccess = false;
				break;
			}
		}

		return bSuccess;
	}

	private static bool LongestPathMatchesDifficulty(PuzzleMakingParameters parameters, Puzzle puzzle)
	{
		bool success = true;

		int maxPath = parameters.DifficultyParameters.MaxPaths(parameters.BoardSize);

		foreach(var path in puzzle.Board.GetPaths())
		{
			int pathCounter = 0;
			foreach(var tile in path)
			{
				if (tile.ExpectedState != GameTileState.MirrorBotLeftTopRight &&
					tile.ExpectedState != GameTileState.MirrorTopLeftBotRight)
				{
					pathCounter++;
				}
			}

			if (pathCounter > maxPath)
			{
				success = false;
				break;
			}
		}

		return success;
	}

	private static bool TryPopulateUniquePaths(PuzzleMakingParameters parameters, Puzzle puzzle)
	{
		bool success = false;

		int totalPathCount = puzzle.Board.BoardSize * 4;
		int targetNonUniquePaths = Mathf.RoundToInt(totalPathCount * (1.0f - parameters.DifficultyParameters.UniqueSolutionPaths));
		List<List<GameTile>> incompletePaths = puzzle.Board.GetPaths().ToList();
		ShuffleList(incompletePaths); //this is done so that we're dispersing the unique solutions around the puzzle instead of in sequential order

		bool madeProgress = false;
		do
		{
			madeProgress = false;

			if (incompletePaths.Count <= targetNonUniquePaths)
			{
				success = true;
			}
			else
			{
				//iterate in reverse so we can safely remove items from the list
				for (int i = incompletePaths.Count - 1; i >= 0; i--)
				{
					if (TryToFillPathWithUniqueSolution(incompletePaths[i], parameters))
					{
						madeProgress = true;
						incompletePaths.RemoveAt(i);
					}
				}
			}

			break;
		} while (madeProgress && !success);

		return success;
	}

	//Ripped from Stack Overflow and adjusted to use Unity Random
	private static void ShuffleList<T>(IList<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = Random.Range(0, n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	private static bool TryToFillPathWithUniqueSolution(List<GameTile> path, PuzzleMakingParameters parameters)
	{
		bool success = false;

		//first, take metrics on the current state of the path
		Dictionary<GameTileState, int> tilesBeforeMirror = new Dictionary<GameTileState, int>();
		bool hasMirror = false;
		Dictionary<GameTileState, int> tilesAfterMirror = new Dictionary<GameTileState, int>();
		foreach (var tile in path)
		{
			if (tile.ExpectedState.IsMirror())
			{
				hasMirror = true;
			}
			else if (hasMirror) //we're after a mirror
			{
				if (tilesAfterMirror.ContainsKey(tile.ExpectedState))
				{
					tilesAfterMirror[tile.ExpectedState]++;
				}
				else
				{
					tilesAfterMirror.Add(tile.ExpectedState, 1);
				}
			}
			else //we haven't yet hit a mirror
			{
				if (tilesBeforeMirror.ContainsKey(tile.ExpectedState))
				{
					tilesBeforeMirror[tile.ExpectedState]++;
				}
				else
				{
					tilesBeforeMirror.Add(tile.ExpectedState, 1);
				}
			}
		}

		//second, see if there aren't unique solutions (more than 1 of a monster type on either side of a mirror)
		if (tilesBeforeMirror.Where(kvp => kvp.Key != GameTileState.Empty).Count() > 1 ||
			tilesAfterMirror.Where(kvp => kvp.Key != GameTileState.Empty).Count() > 1)
		{
			success = false;
		}
		else
		{
			//third - if we got to this point, we know the path can be filled with a unique solution
			GameTileState beforeMirrorTile;
			GameTileState afterMirrorTile;

			if (tilesBeforeMirror.Where(kvp => kvp.Key != GameTileState.Empty).Count() > 1)
			{
				//we already have a non-empty tile before a mirror, so figure out what to fill the rest with
				beforeMirrorTile = tilesBeforeMirror.Where(kvp => kvp.Key != GameTileState.Empty).First().Key;
			}
			else
			{
				//pick what should go before
				//easiest: human, ghost
				//hardest: vampire
				if (Random.value < parameters.DifficultyParameters.RandomFillComplexity)
				{
					beforeMirrorTile = GameTileState.Vampire;
				}
				else if (Random.value > 0.5f)
				{
					beforeMirrorTile = GameTileState.Ghost;
				}
				else
				{
					beforeMirrorTile = GameTileState.Human;
				}
			}

			if (tilesAfterMirror.Where(kvp => kvp.Key != GameTileState.Empty).Count() > 1)
			{
				//we already have a non-empty tile after a mirror, so figure out what to fill the rest with
				afterMirrorTile = tilesAfterMirror.Where(kvp => kvp.Key != GameTileState.Empty).First().Key;
			}
			else
			{
				//pick what should go after
				//easiest: human, vampire
				//hardest: ghost
				if (Random.value < parameters.DifficultyParameters.RandomFillComplexity)
				{
					afterMirrorTile = GameTileState.Ghost;
				}
				else if (Random.value > 0.5f)
				{
					afterMirrorTile = GameTileState.Human;
				}
				else
				{
					afterMirrorTile = GameTileState.Vampire;
				}
			}

			bool hitMirror = false;
			foreach (var tile in path)
			{
				if (tile.ExpectedState.IsMirror())
				{
					hitMirror = true;
				}
				else if (tile.ExpectedState == GameTileState.Empty)
				{
					if (hitMirror)
					{
						tile.SetExpectedState(afterMirrorTile);
					}
					else
					{
						tile.SetExpectedState(beforeMirrorTile);
					}
				}
			}
			success = true;
		}
		return success;
	}

	private static bool TryFillRestWithRandom(PuzzleMakingParameters parameters, Puzzle puzzle)
	{
		bool success = true;

		foreach(GameTile tile in puzzle.Board.GetAllGameTiles())
		{
			if (tile.ExpectedState == GameTileState.Empty)
			{
				//RandomFillComplexity drives whether it's a simple monster (a human) or a complex one
				//If it's a complex one, there's a 50/50 chance for it to be a ghost or vampire
				if (Random.value >= parameters.DifficultyParameters.RandomFillComplexity)
				{
					tile.SetExpectedState(GameTileState.Human);
				}
				else if (Random.value > 0.5f)
				{
					tile.SetExpectedState(GameTileState.Ghost);
				}
				else
				{
					tile.SetExpectedState(GameTileState.Vampire);
				}
			}
		}
		
		return success;
	}

	private static bool EnsureSingleSolution(Puzzle puzzle)
	{
		bool success = true;

		//TODO: THIS!!

		return success;
	}
}

public class PuzzleMakingParameters
{
	public BoardSize BoardSize { get; set; }
	public PuzzleDifficulty Difficulty { get; set; }

	public int RandomizationSeed { get; set; }
	public int RetryAttempts { get; set; }

	DifficultyParameters difficultyParameters = null;
	public DifficultyParameters DifficultyParameters
	{
		get
		{
			if (difficultyParameters == null)
			{
				difficultyParameters = DifficultyFactory.GetDifficultyParameters(Difficulty);
			}
			return difficultyParameters;
		}
	}
}
