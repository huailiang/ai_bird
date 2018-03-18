using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Reinforcement
{

    private static Reinforcement s;

    public static Reinforcement S { get { if (s == null) s = new Reinforcement(); return s; } }

    // greedy police
    float epsilon = 0.9f;

    // learning rate
    float alpha = 0.1f;

    //discount factor
    float gamma = 0.9f;

    int last_r = 0, last_state = 0;
    bool last_action = false;

    string save_path
    {
        get
        {
            string p = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(p, "q_tb.csv");
        }
    }

    Dictionary<int, Row> q_table;

    public class Row
    {
        public float pad;
        public float stay;
    }

    public void Init()
    {
        Build_Q_Table();
        MainLogic.AddCommandHook(COMMAND_TYPE.GAME_START, OnStart);
        MainLogic.AddCommandHook(COMMAND_TYPE.SCORE, OnScore);
        MainLogic.AddCommandHook(COMMAND_TYPE.COMMAND_MAX, OnScore);
        MainLogic.AddCommandHook(COMMAND_TYPE.GAME_OVERD, OnDied);
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
        // Debug.LogWarning ("died");
        last_r = -10;
    }

    /*
    comment: tick time is 15f
     */
    public void OnTick()
    {
        int state = GetCurrentState();
        if (last_state != -1)
        {
            //cul last loop
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
        // int p_st = PillarManager.S.GetPillarState();
        // int b_st = GameManager.S.mainBird.GetState();
        // return p_st + b_st;
        return GameManager.S.mainBird.GetState();
    }

    //柱子一共有3x3+1=10种状态
    //加上鸟 一共5x10=50个状态
    public void Build_Q_Table()
    {
        q_table = new Dictionary<int, Row>();
        for (int i = 0; i <= 4; i++)
        {
            // for (int j = 1; j <= 3; j++)
            // {
            //     for (int k = 1; k <= 3; k++)
            //     {
            //         Row row = new Row() { pad = 0f, stay = 0f };
            //         int key = i + (int)Mathf.Pow(10, j) * k;
            //         q_table.Add(key, row);
            //     }
            // }
            Row row2 = new Row() { pad = 0f, stay = 0f };
            q_table.Add(i, row2);
        }
    }

    public bool choose_action(int state)
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
            if (rewd != 0) Debug.Log("state:" + state + " rewd:" + rewd + " add:" + add);
            if (action)
            {
                q_table[state].pad += add;
            }
            else
            {
                q_table[state].stay += add;
            }
            Debug.Log("state:" + state + " rewd:" + rewd + " action:" + action);
        }
    }

    /// <summary>
    /// 导出q_table
    /// </summary>
    public void exportQTable()
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