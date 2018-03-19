using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// 内部实现强化学习q_learning
/// </summary>
public class InternalEnv : BaseEnv
{
    int last_r = 0, last_state = 0;
    bool last_action = false;

    Dictionary<int, Row> q_table;

    public class Row
    {
        /// <summary>
        /// 拍翅膀
        /// </summary>
        public float pad;
        public float stay;
    }

    public override void Init()
    {
        Build_Q_Table();
        EventHandle.AddCommandHook(COMMAND_TYPE.GAME_START, OnStart);
        EventHandle.AddCommandHook(COMMAND_TYPE.SCORE, OnScore);
        EventHandle.AddCommandHook(COMMAND_TYPE.COMMAND_MAX, OnScore);
        EventHandle.AddCommandHook(COMMAND_TYPE.GAME_OVERD, OnDied);
        loadQTable();
    }

    void OnStart(object o)
    {
        last_r = 1;
        last_r = 0;
        last_state = -1;
    }

    void OnScore(object arg)
    {
        Debug.Log("score");
        last_r = 20;
    }

    void OnDied(object arg)
    {
        last_r = -10;
    }

    /*
    comment: tick time is 15f
     */
    public override void OnTick()
    {
        int state = GetCurrentState();
        if (last_state != -1)
        {
            UpdateState(last_state, state, last_r, last_action);
        }

        //do next loop
        bool action = choose_action(state);
        GameManager.S.RespondByDecision(action);
        last_r = 1;
        last_state = state;
        last_action = action;
    }

    public int GetCurrentState()
    {
#if ENABLE_PILLAR
        int p_st = PillarManager.S.GetPillarMiniState();
        int b_st = GameManager.S.mainBird.GetState();
        return p_st + b_st;
#else
        return GameManager.S.mainBird.GetState();
#endif
    }


    /// <summary>
    /// Bird [0-9)一共九个状态
    /// Pillar [0-5) 一共5个状态 
    /// 状态统计 9x5=45个状态
    /// </summary>
    public void Build_Q_Table()
    {
        q_table = new Dictionary<int, Row>();
        for (int i = 0; i < 9; i++)
        {
#if ENABLE_PILLAR
            for (int j = 0; j < 5; j++)
            {
                Row row = new Row() { pad = 0f, stay = 0f };
                // Debug.Log("i:" + i + " j:" + j + " val:" + (i + 10 * j));
                q_table.Add(i + 10 * j, row);
            }
#else
            Row row = new Row() { pad = 0f, stay = 0f };
            q_table.Add(i, row);
#endif

        }
    }

    public override bool choose_action(int state)
    {
        if (q_table == null || Random.Range(0.0f, 1.0f) > epsilon)
        {
            return Random.Range(0, 2) > 0;
        }
        else
        {
            Row row = q_table[state];
            return row.pad > row.stay;
        }
    }

    /**
        更新 Q_TABLE
     */
    public void UpdateState(int state, int state_, int rewd, bool action)
    {
        if (q_table != null)
        {
            Row row = q_table[state_];
            float max = row.pad > row.stay ? row.pad : row.stay;
            float q_target = rewd + gamma * max;
            float q_predict = action ? q_table[state].pad : q_table[state].stay;
            float add = alpha * (q_target - q_predict);

            if (action)
            {
                q_table[state].pad += add;
            }
            else
            {
                q_table[state].stay += add;
            }
            if (rewd != 1) Debug.Log("state:" + state + " rewd:" + rewd + " action:" + action + " add: " + add);
        }
    }

    /// <summary>
    /// 导出q_table
    /// </summary>
    public override void exportQTable()
    {
        Debug.Log(save_path);
        FileStream fs = new FileStream(save_path, FileMode.OpenOrCreate, FileAccess.Write);
        StreamWriter sw = new StreamWriter(fs);
        foreach (var item in q_table)
        {
            string line = item.Key + "," + item.Value.pad + "," + item.Value.stay;
            sw.WriteLine(line);
        }
        sw.Close();
        fs.Close();
    }

    /// <summary>
    /// 游戏进入时 加载q_table
    /// </summary>
    private void loadQTable()
    {
        if (q_table == null) q_table = new Dictionary<int, Row>();
        if (File.Exists(save_path))
        {
            FileStream fs = new FileStream(save_path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            while (true)
            {
                string line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) break;
                string[] ch = line.Split(':');
                if (ch.Length >= 3)
                {
                    int key = int.Parse(ch[0]);
                    float pad = float.Parse(ch[1]);
                    float stay = float.Parse(ch[2]);
                    Row row = new Row() { stay = stay, pad = pad };
                    if (!q_table.ContainsKey(key)) q_table.Add(key, row);
                    else q_table[key] = row;
                }
            }
            sr.Dispose();
            fs.Dispose();
        }
    }
}