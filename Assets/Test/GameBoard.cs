using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum BoardSize
{
	Four = 4,
	Five = 5,
	Six = 6,
	Eight = 8
}

public enum ColumnIndicator
{
	ColumnTop,
	ColumnBottom,
	RowTop,
	RowBottom
}

/// <summary>
/// In-Memory representation of a Game Board
/// </summary>
public class GameBoard
{
	/*
	 
		Board by x,y:
		0,0   0,1   0,2   0,3
		1,0   1,1   1,2   1,3
		2,0   2,1   2,2   2,3
		3,0   3,1   3,2   3,3

		Board by index:
		0     1     2     3
		4     5     6     7
		8     9     10    11
		12    13    14    15

		Path by Column/Row, Index, Top/Bot:
		        C,0,T   C,1,T   C,2,T   C,3,T
		R,0,T                                   R,0,B
		R,1,T									R,1,B
		R,2,T									R,2,B
		R,3,T									R,3,B
		        C,0,B   C,1,B   C,2,B   C,3,B
		
		Path by index:
		     0    1    2    3
		8                        12
		9                        13
		10                       14
		11                       15
		     4    5    6    7
	 */

	GameTile[] board;
	List<GameTile>[] paths;

	private int boardSize;
	public int BoardSize
	{
		get
		{
			return boardSize;
		}
	}

	public int TileCount
	{
		get
		{
			return boardSize * boardSize;
		}
	}

	public void Initialize(BoardSize size)
	{
		boardSize = (int)size;
		board = new GameTile[boardSize];
	}

	public bool TryGetGameTile(int x, int y, out GameTile gameTile)
	{
		bool bExists = false;
		gameTile = null;

		int index = (x * BoardSize) + y;
		if (x >= 0 && y >= 0 && index < board.Length)
		{
			gameTile = board[index];
			bExists = true;
		}

		return bExists;
	}

	public bool TryGetGameTile(int index, out GameTile gameTile)
	{
		bool bExists = false;
		gameTile = null;

		if (index < board.Length)
		{
			gameTile = board[index];
			bExists = true;
		}

		return bExists;
	}

	public IEnumerable<GameTile> GetAllGameTiles()
	{
		foreach(GameTile gt in board)
		{
			yield return gt;
		}
	}

	public void ClearTiles()
	{
		foreach(GameTile tile in board)
		{
			tile.ClearTile();
		}
		paths = null;
	}

	public IEnumerable<List<GameTile>> GetPaths()
	{
		foreach (List<GameTile> path in paths)
		{
			yield return path;
		}
	}

	public bool SetPaths()
	{
		paths = new List<GameTile>[BoardSize * 4];

		int curIndex = 0;
		//top of columns
		for (int tempIndex = 0; tempIndex < BoardSize; tempIndex++)
		{
			paths[curIndex + tempIndex] = CreatePath(new PathTraversal()
			{
				CurrentX = tempIndex,
				CurrentY = -1,
				Direction = PathTraversal.TraversalDirection.Down
			}).ToList();
		}
		curIndex += BoardSize;
		
		//bot of columns
		for (int tempIndex = 0; tempIndex < BoardSize; tempIndex++)
		{
			paths[curIndex + tempIndex] = CreatePath(new PathTraversal()
			{
				CurrentX = tempIndex,
				CurrentY = BoardSize,
				Direction = PathTraversal.TraversalDirection.Up
			}).ToList();
		}
		curIndex += BoardSize;

		//left of rows
		for (int tempIndex = 0; tempIndex < BoardSize; tempIndex++)
		{
			paths[curIndex + tempIndex] = CreatePath(new PathTraversal()
			{
				CurrentX = -1,
				CurrentY = tempIndex,
				Direction = PathTraversal.TraversalDirection.Right
			}).ToList();
		}
		curIndex += BoardSize;

		//right of rows
		for (int tempIndex = 0; tempIndex < BoardSize; tempIndex++)
		{
			paths[curIndex + tempIndex] = CreatePath(new PathTraversal()
			{
				CurrentX = BoardSize,
				CurrentY = tempIndex,
				Direction = PathTraversal.TraversalDirection.Left
			}).ToList();
		}
		curIndex += BoardSize;

		return true; //this will always succeed
	}

	private IEnumerable<GameTile> CreatePath(PathTraversal traversal)
	{
		IEnumerable<GameTile> path = Enumerable.Empty<GameTile>();

		GameTile nextTile;
		if (TryGetGameTile(traversal.NextX, traversal.NextY, out nextTile))
		{
			PathTraversal continuePath = new PathTraversal()
			{
				CurrentX = traversal.NextX,
				CurrentY = traversal.CurrentY
			};

			if (nextTile.ExpectedState == GameTileState.MirrorBotLeftTopRight)
			{
				/*
				    /
			       /
			      /

			     Up -> Right
				 Down -> Left
				 Left -> Down
				 Right -> Up
				 */
				switch(traversal.Direction)
				{
					case PathTraversal.TraversalDirection.Up:
						continuePath.Direction = PathTraversal.TraversalDirection.Right;
						break;
					case PathTraversal.TraversalDirection.Down:
						continuePath.Direction = PathTraversal.TraversalDirection.Left;
						break;
					case PathTraversal.TraversalDirection.Left:
						continuePath.Direction = PathTraversal.TraversalDirection.Down;
						break;
					case PathTraversal.TraversalDirection.Right:
						continuePath.Direction = PathTraversal.TraversalDirection.Up;
						break;
				}

			}
			else if (nextTile.ExpectedState == GameTileState.MirrorTopLeftBotRight)
			{
				/*
				  \
				   \
				    \

				 Up -> Left
				 Down -> Right
				 Left -> Up
				 Right -> Down
				 */
				switch (traversal.Direction)
				{
					case PathTraversal.TraversalDirection.Up:
						continuePath.Direction = PathTraversal.TraversalDirection.Left;
						break;
					case PathTraversal.TraversalDirection.Down:
						continuePath.Direction = PathTraversal.TraversalDirection.Right;
						break;
					case PathTraversal.TraversalDirection.Left:
						continuePath.Direction = PathTraversal.TraversalDirection.Up;
						break;
					case PathTraversal.TraversalDirection.Right:
						continuePath.Direction = PathTraversal.TraversalDirection.Down;
						break;
				}
			}
			else
			{
				//continue in the same direction
				continuePath.Direction = traversal.Direction;
			}

			//there might be a better way to do this than concatting twice in a row
			path = path.Concat(new[] { nextTile });
			path = path.Concat(CreatePath(continuePath));
		}

		return path;
	}

	private struct PathTraversal
	{
		internal int CurrentX;
		internal int CurrentY;
		internal TraversalDirection Direction;

		internal enum TraversalDirection
		{
			Up,
			Down,
			Left,
			Right
		}

		internal int NextX
		{
			get
			{
				if (Direction == TraversalDirection.Left)
				{
					return CurrentX - 1;
				}
				else if (Direction == TraversalDirection.Right)
				{
					return CurrentX + 1;
				}
				else
				{
					return CurrentX;
				}
			}
		}

		internal int NextY
		{
			get
			{
				if (Direction == TraversalDirection.Up)
				{
					return CurrentY - 1;
				}
				else if (Direction == TraversalDirection.Down)
				{
					return CurrentY + 1;
				}
				else
				{
					return CurrentY;
				}
			}
		}

	}
}

