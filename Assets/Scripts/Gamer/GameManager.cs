using UnityEngine;
using System.Collections;

public enum TrainMode
{
    Internal,
    External,
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager S { get { return instance; } }

    [SerializeField] ScriptableObject[] envs;

    private BaseEnv env;

    public bool isGameStart = false;

    private bool isGameOver = false;

    public Bird mainBird;

    [SerializeField] public bool isTrainning = true;

    [SerializeField] private TrainMode mode = TrainMode.Internal;

    public bool isWaiting = false;

    private float resetTime = 0f;

    private GUIStyle style;

    private float lastSignTime = float.MinValue;

    private static float tickTime;

    public bool IsGameOver { get { return isGameOver; } }

    public BaseEnv Env { get { return env; } }

    public void FillEnv()
    {
        if (envs == null || envs.Length == 0)
        {
            int num = System.Enum.GetValues(typeof(TrainMode)).Length;

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
        tickTime = 15 * Time.deltaTime;
        style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.red;
        FillEnv();
        env.Init();
    }


    void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 100, 30), "TIME: " + (Time.time - resetTime).ToString("f2"), style);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isWaiting || (isTrainning && isGameStart))
            {
                return;
            }
            if (!GameManager.S.isGameStart || IsGameOver)
            {
                GameControl();
            }
            else
            {
                mainBird.FlyUp();
            }
        }

        if (GameManager.S.isGameStart)
        {
            if (Time.time - lastSignTime > tickTime)
            {
                env.OnTick();
                lastSignTime = Time.time;
            }
        }
    }

    void GameControl()
    {
        if (GameManager.S.isGameOver)
        {
            ResetGame();
            Debug.Log("游戏重置");
            EventHandle.Command(COMMAND_TYPE.GAME_RESET);
        }
        else if (!GameManager.S.isGameStart)
        {
            GameManager.S.isGameStart = true;
            Debug.Log("游戏开始");
            EventHandle.Command(COMMAND_TYPE.GAME_START);
        }
    }

    public bool RespondByDecision(BirdAction action)
    {
        if (!GameManager.S.isGameStart || IsGameOver)
        {
            return false;
        }
        else
        {
            if (action == BirdAction.FLY)
            {
                mainBird.FlyUp();
            }
            return true;
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

        if (isTrainning)
        {
            StartCoroutine(RestartGame());
        }
    }

    public void OnEsplisonEnd()
    {
        if (isTrainning)
        {
            isGameOver = true;
            StartCoroutine(RestartGame());
        }
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSecondsRealtime(tickTime);
        ResetGame();
        EventHandle.Command(COMMAND_TYPE.GAME_RESET);
        GameManager.S.isGameStart = true;
        EventHandle.Command(COMMAND_TYPE.GAME_START);
    }

    public void ResetGame()
    {
        resetTime = Time.time;
        PillarManager.S.ClearPillars();
        mainBird.ResetPos();
        GameManager.S.isGameStart = false;
        GameManager.S.isGameOver = false;
    }

}
