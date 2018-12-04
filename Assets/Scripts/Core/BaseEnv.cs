using UnityEngine;

public abstract class BaseEnv : ScriptableObject
{
    protected float epsilon = 0.9f;
    protected float alpha = 0.1f;
    protected float gamma = 0.9f;
    protected int last_r = 1;
    protected int[] last_state;
    protected int total_r = 0;
    protected BirdAction last_action = BirdAction.PAD;
    
    public int Score { get { return total_r; } }

    protected abstract bool birdFly { get; }

    public virtual void Init()
    {
        EventHandle.AddCommandHook(COMMAND_TYPE.GAME_START, OnStart);
        EventHandle.AddCommandHook(COMMAND_TYPE.SCORE, OnScore);
        EventHandle.AddCommandHook(COMMAND_TYPE.GAME_OVERD, OnDied);
    }

    void OnStart(object o)
    {
        last_r = 0;
        total_r = 0;
        last_state = null;
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

    public int[] GetCurrentState()
    {
#if ENABLE_PILLAR
        int[] p_st = GameMgr.S.pillMgr.GetPillarState();
        int b_st = GameMgr.S.mainBird.GetState();
        int[] rst = new int[3];
        rst[0] = p_st[0];
        rst[1] = p_st[1];
        rst[2] = b_st;
        return rst;
#else
        return new int[GameManager.S.mainBird.GetState()];
#endif
    }

    public virtual void OnUpdate(float delta)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
        {
            GameMgr.S.ManuControl(birdFly);
        }
    }

    public virtual void OnTick()
    {
        total_r += last_r;
    }

    public abstract BirdAction choose_action(int[] state);

    public abstract void UpdateState(int[] state, int[] state_, int rewd, BirdAction action);
    
    public virtual void OnRestart(int[] state) { }

    public virtual void OnInspector() { }

}