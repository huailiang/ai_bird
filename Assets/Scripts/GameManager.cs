using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
	private static GameManager instance;
	public static GameManager S { get { return instance; } }

	void Awake () { instance = this; }
	// 游戏是否开始
	[HideInInspector] public bool isGameStart = false;
	// 游戏是否结束
	[HideInInspector] private bool isGameOver  = false;

	[SerializeField] private Bird mainBird;

	public bool IsGameOver { get { return isGameOver; } }
	
	public bool isWaiting=false;
	
	
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.Mouse0))
		{
			if(isWaiting) return;
			if(!GameManager.S.isGameStart || IsGameOver)
			{
				GameControl();
			}
			else
			{
				mainBird.FlyUp();
			}
		}
	}

	void GameControl()
	{
		if(GameManager.S.isGameOver)
		{
			ResetGame();
			Debug.Log("游戏重置");
			MainLogic.Command(null, COMMAND_TYPE.GAME_RESET);
		}
		else if(!GameManager.S.isGameStart)
		{
			GameManager.S.isGameStart = true;
			Debug.Log("游戏开始");
			MainLogic.Command(null, COMMAND_TYPE.GAME_START);
		}
	}

	public void GameOver()
	{
		
		isGameOver = true;
		MainLogic.Command(this, COMMAND_TYPE.GAME_OVERD);
	}
	
	
	public void ResetGame()
	{
		PillarManager.S.ClearPillars();
		mainBird.ResetPos();
		GameManager.S.isGameStart			= false;
		GameManager.S.isGameOver			= false;
		Scorers.S.ResetMark();
	}
}
