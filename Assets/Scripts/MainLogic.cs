using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum COMMAND_TYPE
{
	/// <summary>
	/// 游戏结束
	/// </summary>
	GAME_OVERD = 1,

	/// <summary>
	/// 游戏重置
	/// </summary>
	GAME_RESET,

	/// <summary>
	/// 游戏开始
	/// </summary>
	GAME_START,

	/// <summary>
	/// 加分
	/// </summary>
	SCORE,

	/// <summary>
	/// 打破记录
	/// </summary>
	BREAKING_RECORDS,


	COMMAND_MAX,
}

// 主要逻辑模块
public class MainLogic : MonoBehaviour {

	private static MainLogic instance = null;

	public static MainLogic S { get { return instance; } }
	void Awake () { instance = this; }
	
	// 事件
	#region Command event
	public delegate void CommandCallBack(object obj, params object[] args);
	static Dictionary<COMMAND_TYPE, Dictionary<string, CommandCallBack>> allCB = new Dictionary<COMMAND_TYPE, Dictionary<string, CommandCallBack>>();
	
	// 添加监听
	public static bool AddCommandHook(string hookName, COMMAND_TYPE command, CommandCallBack cb)
	{
		if (string.IsNullOrEmpty(hookName))
		{
			return false;
		}

		if (command <= 0 || command >= COMMAND_TYPE.COMMAND_MAX)
		{
			return false;
		}

		if (!allCB.ContainsKey(command))
		{
			allCB.Add(command, new Dictionary<string, CommandCallBack>());
		}

		Dictionary<string, CommandCallBack> list = allCB[command];
		if (list.ContainsKey(hookName))
		{
			return false;
		}

		list.Add(hookName, cb);

		return true;
	}
	
	// 删除监听
	public static bool RemoveCommandHook(string hookName, COMMAND_TYPE command)
	{
		if (string.IsNullOrEmpty(hookName))
		{
			return false;
		}

		if (command <= 0 || command >= COMMAND_TYPE.COMMAND_MAX)
		{
			return false;
		}

		if (!allCB.ContainsKey(command))
		{
			return false;
		}

		Dictionary<string, CommandCallBack> list = allCB[command];
		if (!list.ContainsKey(hookName))
		{
			return false;
		}

		list.Remove(hookName);

		return true;
	}
	
	// 监听就否存在
	public static bool FindCommandHook(string hookName, COMMAND_TYPE command)
	{
		if (string.IsNullOrEmpty(hookName))
		{
			return false;
		}

		if (command <= 0 || command >= COMMAND_TYPE.COMMAND_MAX)
		{
			return false;
		}

		if (!allCB.ContainsKey(command))
		{
			return false;
		}

		Dictionary<string, CommandCallBack> list = allCB[command];
		if (!list.ContainsKey(hookName))
		{
			return false;
		}

		return true;
	}
	
	// 发送监听信息
	public static void Command(object obj, COMMAND_TYPE command, params object[] args)
	{
//		if (null == obj)
//		{
//			return;
//		}

		if (command <= 0 || command >= COMMAND_TYPE.COMMAND_MAX)
		{
			return ;
		}

		if (!allCB.ContainsKey(command))
		{
			return ;
		}

		Dictionary<string, CommandCallBack> list = allCB[command];

		List<CommandCallBack> runList = new List<CommandCallBack>();

		foreach (KeyValuePair<string, CommandCallBack> kv in list)
		{
			runList.Add(kv.Value);
		}

		try
		{
			foreach (CommandCallBack cb in runList)
			{
//				if (obj != null)
				{
					cb(obj, args);
				}
			}
		}
		catch (System.Exception ex)
		{
			Debug.Log("Command exception: " + ex.ToString());
		}
	}
	#endregion
}
