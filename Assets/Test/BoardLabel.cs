using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardLabel : MonoBehaviour
{
	public PlayingTile[] observingTiles;

	float timerdur = 0.1f;
	float curtime = 0.0f;

	private Text thisText;

	// Use this for initialization
	void Start ()
	{
		thisText = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		curtime += Time.deltaTime;

		if (curtime > timerdur)
		{
			curtime = 0.0f;
			Recheck();
		}
	}

	void Recheck()
	{
		int correct = 0;

		foreach (PlayingTile tile in observingTiles)
		{	
			if (tile.expectedFlavor == tile.currentFlavor)
			{
				correct++;
			}
		}
		thisText.text = correct.ToString();
	}
}
