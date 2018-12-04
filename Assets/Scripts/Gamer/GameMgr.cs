using UnityEngine;
using System.Collections;

public enum TrainMode
{
    Internal,
    External,
    Player,
}

public class GameMgr : MonoBehaviour
{
    private static GameMgr instance;
    public static GameMgr S { get { return instance; } }

    [SerializeField] ScriptableObject[] envs;
    [SerializeField] Pillar pillar;
    [SerializeField] private TrainMode mode = TrainMode.Internal;

    private BaseEnv env;
    private bool isGameStart = false;
    private bool isGameOver = false;
    private float resetTime = 0f;
    private static float tickTime;
    private int epsilon = 0;
    private GUIStyle style;
    private float lastSignTime = float.MinValue;

    public Bird mainBird;
    public PillarMgr pillMgr;

    const float fpsMeasurePeriod = 0.5f;
    private int m_FpsAccumulator = 0;
    private float m_FpsNextPeriod = 0;
    private float m_TotalTime = 0f;
    private float m_CurrentFps;
    private float m_SignTime = 0;

    public bool IsGameOver { get { return isGameOver; } }

    public bool IsGameStart { get { return isGameStart; } }

    public BaseEnv Env { get { return env; } }

    public void FillEnv()
    {
        int num = System.Enum.GetValues(typeof(TrainMode)).Length;
        if (envs == null || envs.Length != num)
        {
            envs = new ScriptableObject[num];
            foreach (TrainMode mode in System.Enum.GetValues(typeof(TrainMode)))
            {
                envs[(int)mode] = ScriptableObject.CreateInstance(mode.ToString() + "Env");
            }
        }
        env = (BaseEnv)envs[(int)mode];
    }


    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
        tickTime = 10 * Time.deltaTime;
        style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.red;
        FillEnv();
        pillMgr = new PillarMgr(pillar);
        env.Init();
    }


    void OnGUI()
    {
        string str = string.Format("frame:{0} ", m_CurrentFps.ToString("f2"));
        GUI.Label(new Rect(20, 20, 100, 30), str, style);
        str = string.Format("runer:{0}", (Time.time - resetTime).ToString("f2"));
        GUI.Label(new Rect(20, 50, 100, 30), str, style);
        str = string.Format("epsln:{0}", epsilon);
        GUI.Label(new Rect(20, 80, 100, 30), str, style);
        str = string.Format("score:{0}", env.Score);
        GUI.Label(new Rect(20, 110, 100, 30), str, style);
    }

    void Update()
    {
        float delta = Time.deltaTime;
        if (isGameStart)
        {
            if (Time.time - lastSignTime > tickTime)
            {
                env.OnTick();
                lastSignTime = Time.time;
            }
        }
        env.OnUpdate(delta);
        pillMgr.Update(delta);

        m_FpsAccumulator++;
        m_TotalTime += Time.realtimeSinceStartup - m_SignTime;
        m_SignTime = Time.realtimeSinceStartup;
        if (Time.realtimeSinceStartup > m_FpsNextPeriod)
        {
            m_CurrentFps = m_FpsAccumulator / m_TotalTime;
            m_TotalTime = 0;
            m_FpsAccumulator = 0;
            m_FpsNextPeriod = Time.realtimeSinceStartup + fpsMeasurePeriod;
        }
    }

    public void ManuControl(bool fly)
    {
        if (isGameOver)
        {
            ResetGame();
            Debug.Log("Game Reset");
            EventHandle.Command(COMMAND_TYPE.GAME_RESET);
        }
        else if (!isGameStart)
        {
            isGameStart = true;
            Debug.Log("Game Start");
            EventHandle.Command(COMMAND_TYPE.GAME_START);
        }
        else if (fly)
        {
            mainBird.FlyUp();
        }
    }


    public void RespondByDecision(BirdAction action)
    {
        if (isGameStart && !isGameOver)
        {
            if (action == BirdAction.FLY)
            {
                mainBird.FlyUp();
            }
        }
    }

    public void OnApplicationQuit()
    {
        if (env != null)
        {
            env.OnApplicationQuit();
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        EventHandle.Command(COMMAND_TYPE.GAME_OVERD);

        if (mode == TrainMode.External || mode == TrainMode.Internal)
        {
            StartCoroutine(RestartGame());
        }
    }

    public void OnScore()
    {
        EventHandle.Command(COMMAND_TYPE.SCORE);
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSecondsRealtime(tickTime);
        ResetGame();
        EventHandle.Command(COMMAND_TYPE.GAME_RESET);
        isGameStart = true;
        EventHandle.Command(COMMAND_TYPE.GAME_START);
    }

    public void ResetGame()
    {
        epsilon++;
        resetTime = Time.time;
        pillMgr.ClearPillars();
        mainBird.ResetPos();
        isGameStart = false;
        isGameOver = false;
    }

}