using System.Collections;
using System.Collections.Generic;

/// <summary>
/// In-memory representation of a game tile
/// </summary>
public class GameTile
{
	GameTileState expectedState = GameTileState.Empty;
	public GameTileState ExpectedState
	{
		get
		{
			return expectedState;
		}
	}

	GameTileState currentState = GameTileState.Empty;
	public GameTileState CurrentState
	{
		get
		{
			return currentState;
		}
	}

	public bool CanChangeCurrentState()
	{
		return currentState.CanChangeState();
	}

	public GameTile(GameTileState expected, GameTileState current = GameTileState.Empty)
	{
		SetExpectedState(expected, current);
	}

	public delegate void StateChange(GameTileState newState);
	public event StateChange OnCurrentStateChange;

	public bool ChangeState(GameTileState newState)
	{
		bool changed = false;
		if (currentState.CanChangeState() && currentState != newState)
		{
			currentState = newState;
			changed = true;
			if (OnCurrentStateChange != null)
			{
				OnCurrentStateChange.Invoke(currentState);
			}
		}

		return changed;
	}

	public void ClearTile()
	{
		expectedState = GameTileState.Empty;
		currentState = GameTileState.Empty;
		if (OnCurrentStateChange != null)
		{
			OnCurrentStateChange.Invoke(currentState);
		}
	}

	public void SetExpectedState(GameTileState expected, GameTileState current = GameTileState.Empty)
	{
		expectedState = expected;
		if (expectedState.CanChangeState())
		{
			currentState = current;
		}
		else
		{
			currentState = expectedState;
		}
	}
}

public enum GameTileState
{
	Empty = 0,
	Vampire,
	Ghost,
	Human,
	MirrorBotLeftTopRight,
	MirrorTopLeftBotRight
}

public static class Extensions
{
	public static bool CanChangeState(this GameTileState thisState)
	{
		return (
			thisState == GameTileState.Empty ||
			thisState == GameTileState.Vampire ||
			thisState == GameTileState.Ghost ||
			thisState == GameTileState.Human);
	}

	public static bool IsMirror(this GameTileState thisState)
	{
		return (
			thisState == GameTileState.MirrorBotLeftTopRight ||
			thisState == GameTileState.MirrorTopLeftBotRight);
	}
}
