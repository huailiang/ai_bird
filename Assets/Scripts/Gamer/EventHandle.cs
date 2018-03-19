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
public class EventHandle

{

    public delegate void CommandCallBack(params object[] args);
    static Dictionary<COMMAND_TYPE, List<CommandCallBack>> allCB = new Dictionary<COMMAND_TYPE, List<CommandCallBack>>();

    // 添加监听
    public static bool AddCommandHook(COMMAND_TYPE command, CommandCallBack cb)
    {
        if (command <= 0 || command >= COMMAND_TYPE.COMMAND_MAX)
        {
            return false;
        }

        if (!allCB.ContainsKey(command))
        {
            List<CommandCallBack> list = new List<CommandCallBack>();
            list.Add(cb);
            allCB.Add(command, list);
        }
        else
        {
            allCB[command].Add(cb);
        }

        return true;
    }

    // 删除监听
    public static bool RemoveCommandHook(COMMAND_TYPE command)
    {
        if (command <= 0 || command >= COMMAND_TYPE.COMMAND_MAX)
        {
            return false;
        }
        if (!allCB.ContainsKey(command))
        {
            return false;
        }
        else
        {
            allCB.Remove(command);
            return true;
        }
    }

    // 监听就否存在
    public static bool FindCommandHook(COMMAND_TYPE command)
    {

        if (command <= 0 || command >= COMMAND_TYPE.COMMAND_MAX)
        {
            return false;
        }

        return allCB.ContainsKey(command);
    }

    // 发送监听信息
    public static void Command(COMMAND_TYPE command, params object[] args)
    {
        if (command <= 0 || command >= COMMAND_TYPE.COMMAND_MAX)
        {
            return;
        }

        if (!allCB.ContainsKey(command))
        {
            return;
        }

        List<CommandCallBack> list = allCB[command];
        foreach (var item in list)
        {
            if (item != null)
            {
                item(args);
            }
        }
    }
}
