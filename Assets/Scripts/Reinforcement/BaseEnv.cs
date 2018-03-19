using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnv
{

    // greedy police
    protected float epsilon = 0.9f;

    // learning rate
    protected float alpha = 0.1f;

    //discount factor
    protected float gamma = 0.9f;


    protected string save_path
    {
        get
        {
            string p = Path.GetDirectoryName(Application.dataPath);
            return Path.Combine(p, "q_tb.csv");
        }
    }

    public virtual void Init()
    {
    }


    public virtual void OnTick()
    {
    }

    public virtual bool choose_action(int state)
    {
        return false;
    }

    public virtual void exportQTable()
    {
    }

}
