using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayingTile : MonoBehaviour
{
	public Sprite blueSprite;
	public Sprite greenSprite;
	public Sprite redSprite;

	public GameTileFlavor expectedFlavor;
	public GameTileFlavor currentFlavor;

	private SpriteRenderer thisRenderer;

	// Use this for initialization
	void Start()
	{
		thisRenderer = GetComponent<SpriteRenderer>();
		currentFlavor = GameTileFlavor.Red;
		SetSprite();
	}

	// Update is called once per frame
	void Update()
	{

	}

	void OnMouseDown()
	{
		//cycle
		if (currentFlavor == GameTileFlavor.Red)
		{
			currentFlavor = GameTileFlavor.Blue;
		}
		else if (currentFlavor == GameTileFlavor.Blue)
		{
			currentFlavor = GameTileFlavor.Green;
		}
		else if (currentFlavor == GameTileFlavor.Green)
		{
			currentFlavor = GameTileFlavor.Red;
		}

		SetSprite();
	}

	private void SetSprite()
	{
		switch(currentFlavor)
		{
			case GameTileFlavor.Blue:
				thisRenderer.sprite = blueSprite;
				break;
			case GameTileFlavor.Green:
				thisRenderer.sprite = greenSprite;
				break;
			case GameTileFlavor.Red:
				thisRenderer.sprite = redSprite;
				break;
		}
	}
}

public enum GameTileFlavor
{
	Blue,
	Green,
	Red
}