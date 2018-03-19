using System.Collections.Generic;
using UnityEngine;

public class ExternalEnv : BaseEnv
{

    private static ExternalEnv _s;

    public static ExternalEnv S { get { if (_s == null) _s = new ExternalEnv(); return _s; } }

    public override void Init()
    {

    }

    public override void OnTick()
    {

    }

    public override bool choose_action(int state)
    {
        return false;
    }


    public void UpdateQTable()
    {

    }

}
