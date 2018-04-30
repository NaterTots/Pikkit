using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity objects for the current Game Board 
/// </summary>
public class PlayingGameBoard : MonoBehaviour
{
	public PlayingTile[] tiles;

	public ObjectPool gameTilePool;

	// Use this for initialization
	void Start()
	{
		gameTilePool.Initialize();

		foreach (PlayingTile tile in tiles)
		{
			switch (Random.Range(0, 3))
			{
				case 0:
					tile.expectedFlavor = GameTileFlavor.Blue;
					break;
				case 1:
					tile.expectedFlavor = GameTileFlavor.Green;
					break;
				case 2:
					tile.expectedFlavor = GameTileFlavor.Red;
					break;
			}
		}
	}

	private bool TryGenerateNewGame()
	{
		return true;
	}

	private void NewGame()
	{
		int boardSize = 4;

		for (int x = 0; x < boardSize; x++)
		{
			for (int y = 0; y < boardSize; y++)
			{
				GameObject newTile = gameTilePool.InitNewObject();

				//newTile.GetComponent<GameTile>().expectedFlavor = ?
			}
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}
