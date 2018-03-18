using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager S { get { return instance; } }


    void Awake() { instance = this; Reinforcement.S.Init(); }

    void Start()
    {
        Debug.Log("time:" + Time.fixedDeltaTime + " fps:" + Application.targetFrameRate);
    }

    // 游戏是否开始
    [HideInInspector] public bool isGameStart = false;
    // 游戏是否结束
    [HideInInspector] private bool isGameOver = false;

    [SerializeField] public Bird mainBird;

    [SerializeField] public bool isTrainning = true;

    public bool IsGameOver { get { return isGameOver; } }

    public bool isWaiting = false;

    private float lastSignTime = 0;

    public static float tickTime { get { return Time.deltaTime * 15; } }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isWaiting || (isTrainning && isGameStart))
            {
                Reinforcement.S.exportQTable();
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
        if (Time.realtimeSinceStartup - lastSignTime > tickTime)
        {
            Reinforcement.S.OnTick();
        }
        if (GameManager.S.isGameStart)
        {
            Scorers.S.SetLiveTime(false);
        }
    }

    void GameControl()
    {
        if (GameManager.S.isGameOver)
        {
            ResetGame();
            Debug.Log("游戏重置");
            MainLogic.Command(COMMAND_TYPE.GAME_RESET);
        }
        else if (!GameManager.S.isGameStart)
        {
            GameManager.S.isGameStart = true;
            Debug.Log("游戏开始");
            MainLogic.Command(COMMAND_TYPE.GAME_START);
        }
    }

    public bool RespondByDecision(bool action)
    {
        if (!GameManager.S.isGameStart || IsGameOver)
        {
            return false;
        }
        else
        {
            if (action)
            {
                mainBird.FlyUp();
            }
            return true;
        }
    }

    public void OnApplicationQuit()
    {
        Reinforcement.S.exportQTable();
    }

    public void GameOver()
    {
        isGameOver = true;
        MainLogic.Command(COMMAND_TYPE.GAME_OVERD);

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

    //等一段时间再开始 让 reinforcement 把最后一次失败更新到 q_table
    IEnumerator RestartGame()
    {
        yield return new WaitForSecondsRealtime(tickTime);
        ResetGame();
        MainLogic.Command(COMMAND_TYPE.GAME_RESET);
        GameManager.S.isGameStart = true;
        MainLogic.Command(COMMAND_TYPE.GAME_START);
        Scorers.S.SetLiveTime(true);
    }

    public void ResetGame()
    {
        PillarManager.S.ClearPillars();
        mainBird.ResetPos();
        GameManager.S.isGameStart = false;
        GameManager.S.isGameOver = false;
        Scorers.S.ResetMark();
    }
}
