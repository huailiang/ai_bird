using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEnv : BaseEnv
{

    public override void OnTick()
    {
    }

    public override BirdAction choose_action(int[] state)
    {
        return BirdAction.NONE;
    }

    public override void UpdateState(int[] state, int[] state_, int rewd, BirdAction action)
    {
        //nothing
    }
}
