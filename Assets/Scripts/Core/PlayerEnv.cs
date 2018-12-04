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

    public override void OnUpdate(float delta)
    {
        base.OnUpdate(delta);

        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
        {
            GameMgr.S.ManuControl();
        }
    }

    public override void UpdateState(int[] state, int[] state_, int rewd, BirdAction action)
    {
        //nothing
    }
}
