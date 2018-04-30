using System.Collections;
using System.Collections.Generic;

public enum PuzzleDifficulty
{
	Easy = 0,
	Medium,
	Hard,
	VeryHard
}

public class DifficultyParameters
{
	public PuzzleDifficulty Difficulty { get; set; }

	/// <summary>
	/// This value is [0,1] and represents the minimum percentage of squares that are mirrors, regardless of direction.
	/// </summary>
	public float MirrorsRatioMin { get; set; }

	/// <summary>
	/// This value is [0,1] and represents the maximum percentage of squares that are mirrors, regardless of direction.
	/// </summary>
	public float MirrorsRatioMax { get; set; }

	Dictionary<BoardSize, int> boardSizeToMaxPath = new Dictionary<BoardSize, int>();

	/// <summary>
	/// The max distance traveled by a single "line of sight" bouncing along mirrors
	/// </summary>
	/// <param name="boardSize"></param>
	/// <returns></returns>
	public int MaxPaths(BoardSize boardSize)
	{
		int maxPaths = 0;
		if (boardSizeToMaxPath.ContainsKey(boardSize))
		{
			maxPaths = boardSizeToMaxPath[boardSize];
		}
		return maxPaths;
	}

	public void SetMaxPath(BoardSize boardSize, int maxPath)
	{
		boardSizeToMaxPath[boardSize] = maxPath;
	}

	/// <summary>
	/// This value is [0,1] and represents the percentage of paths that are immediately solveable.  The higher the number, the easier the puzzle.
	/// </summary>
	public float UniqueSolutionPaths { get; set; }

	/// <summary>
	/// This value is [0,1] and represents the complexity of monsters that get added in the "random fill" step.  
	/// At 0.0, it would only fill humans.  At 1.0 it would fill 50/50 ghosts and vampires.
	/// </summary>
	public float RandomFillComplexity { get; set; }
}

/// <summary>
/// This whole class is better served in a data file.  Putting it here for now so I can get everything up and running faster.
/// </summary>
public static class DifficultyFactory
{
	public static DifficultyParameters GetDifficultyParameters(PuzzleDifficulty difficulty)
	{
		DifficultyParameters difficultyParameters = null;
		switch (difficulty)
		{
			case PuzzleDifficulty.Easy:
				difficultyParameters = GetEasy();
				break;
			case PuzzleDifficulty.Medium:
				difficultyParameters = GetMedium();
				break;
			case PuzzleDifficulty.Hard:
				difficultyParameters = GetHard();
				break;
			case PuzzleDifficulty.VeryHard:
				difficultyParameters = GetVeryHard();
				break;
		}
		return difficultyParameters;
	}

	private static DifficultyParameters GetEasy()
	{
		var easyParams = new DifficultyParameters()
		{
			Difficulty = PuzzleDifficulty.Easy,
			MirrorsRatioMin = 0.22f,
			MirrorsRatioMax = 0.52f,
			UniqueSolutionPaths = 0.80f,
			RandomFillComplexity = 0.2f
		};
		easyParams.SetMaxPath(BoardSize.Four, 5);
		easyParams.SetMaxPath(BoardSize.Five, 6);
		easyParams.SetMaxPath(BoardSize.Six, 7);
		easyParams.SetMaxPath(BoardSize.Eight, 9);

		return easyParams;
	}

	private static DifficultyParameters GetMedium()
	{
		var mediumParams = new DifficultyParameters()
		{
			Difficulty = PuzzleDifficulty.Medium,
			MirrorsRatioMin = 0.22f,
			MirrorsRatioMax = 0.52f,
			UniqueSolutionPaths = 0.74f,
			RandomFillComplexity = 0.4f
		};
		mediumParams.SetMaxPath(BoardSize.Four, 6);
		mediumParams.SetMaxPath(BoardSize.Five, 7);
		mediumParams.SetMaxPath(BoardSize.Six, 9);
		mediumParams.SetMaxPath(BoardSize.Eight, 12);

		return mediumParams;
	}

	private static DifficultyParameters GetHard()
	{
		var hardParams = new DifficultyParameters()
		{
			Difficulty = PuzzleDifficulty.Hard,
			MirrorsRatioMin = 0.22f,
			MirrorsRatioMax = 0.52f,
			UniqueSolutionPaths = 0.65f,
			RandomFillComplexity = 0.6f
		};
		hardParams.SetMaxPath(BoardSize.Four, 8);
		hardParams.SetMaxPath(BoardSize.Five, 9);
		hardParams.SetMaxPath(BoardSize.Six, 11);
		hardParams.SetMaxPath(BoardSize.Eight, 14);

		return hardParams;
	}

	private static DifficultyParameters GetVeryHard()
	{
		var hardParams = new DifficultyParameters()
		{
			Difficulty = PuzzleDifficulty.VeryHard,
			MirrorsRatioMin = 0.22f,
			MirrorsRatioMax = 0.52f,
			UniqueSolutionPaths = 0.55f,
			RandomFillComplexity = 0.8f
		};
		hardParams.SetMaxPath(BoardSize.Four, 9);
		hardParams.SetMaxPath(BoardSize.Five, 11);
		hardParams.SetMaxPath(BoardSize.Six, 13);
		hardParams.SetMaxPath(BoardSize.Eight, 16);

		return hardParams;
	}
}
