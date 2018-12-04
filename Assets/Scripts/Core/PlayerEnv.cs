using UnityEngine;

public class PlayerEnv : BaseEnv
{
    protected override bool birdFly { get { return true; } }

    public override BirdAction choose_action(int[] state)
    {
        return BirdAction.NONE;
    }

    public override void UpdateState(int[] state, int[] state_, int rewd, BirdAction action)
    {
        //nothing
    }
}
