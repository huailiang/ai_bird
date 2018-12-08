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
        tickTime = 15 * Time.deltaTime;
        style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.red;
        FillEnv();
        pillMgr = new PillarMgr(pillar);
        env.Init();
    }

    void OnGUI()
    {
        string str = string.Format("round:{0} timer:{1}", epsilon, (Time.time - resetTime).ToString("f2"));
        GUI.Label(new Rect(30, 30, 100, 30), str, style);
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
        pillMgr.Clear();
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
        mainBird.ResetPos();
        isGameStart = false;
        isGameOver = false;
    }

}