using UnityEngine;

public abstract class BaseEnv : ScriptableObject
{
    protected float epsilon = 0.9f;
    protected float alpha = 0.1f;
    protected float gamma = 0.9f;
    protected int last_r = 1;
    protected int last_state = -1;
    protected bool last_action = false;

    public virtual void Init()
    {
        EventHandle.AddCommandHook(COMMAND_TYPE.GAME_START, OnStart);
        EventHandle.AddCommandHook(COMMAND_TYPE.SCORE, OnScore);
        EventHandle.AddCommandHook(COMMAND_TYPE.COMMAND_MAX, OnScore);
        EventHandle.AddCommandHook(COMMAND_TYPE.GAME_OVERD, OnDied);
    }

    void OnStart(object o)
    {
        last_r = 0;
        last_state = -1;
    }

    void OnScore(object arg)
    {
        last_r = 20;
    }

    void OnDied(object arg)
    {
        last_r = -100;
    }

    public virtual void OnApplicationQuit() { }

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

    public abstract void OnTick();

    public abstract bool choose_action(int state);

    public abstract void UpdateState(int state, int state_, int rewd, bool action);


    public virtual void OnRestart(int state) { }

    public virtual void OnInspector() { }

}