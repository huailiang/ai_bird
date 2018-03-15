using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackdropManager : MonoBehaviour 
{
	private static BackdropManager instance;
	[SerializeField] private List<UVScroller> backdrops;
	public static BackdropManager S { get { return instance; } }

	void Awake() { instance = this; }

	void Start()
	{
		MainLogic.AddCommandHook("BackdropManager::Moves", COMMAND_TYPE.GAME_START, Moves);
		MainLogic.AddCommandHook("BackdropManager::Stops", COMMAND_TYPE.GAME_OVERD, Stops);
		MainLogic.AddCommandHook("BackdropManager::Resets", COMMAND_TYPE.GAME_RESET, Resets);
	}

	public void Moves(object obj, params object[] args) { Moves(); }
	public void Stops(object obj, params object[] args) { Stops(); }
	public void Resets(object obj, params object[] args) { Resets(); }

	public void Moves()
	{
		// 背景越远速度越慢
		for(int i = 0, iMax = backdrops.Count; i < iMax; i++)
		{
			backdrops[i].Move(-GlobalValue.MoveSpeed * 0.325f * (1f/(i+1)));
		}
	}

	public void Stops()
	{
		for(int i = 0, iMax = backdrops.Count; i < iMax; i++)
		{
			backdrops[i].Stop();
		}
	}

	public void Resets()
	{
		for(int i = 0, iMax = backdrops.Count; i < iMax; i++)
		{
			backdrops[i].Reset();
		}
	}
}
