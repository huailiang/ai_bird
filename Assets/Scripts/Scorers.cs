using UnityEngine;

public class Scorers : MonoBehaviour
{
    private static Scorers instance;

    public static Scorers S { get { return instance; } }

    private int currentMark;
    private int maxMark;

    private float liveTime;

    [SerializeField] private Color currentMarkColor;
    [SerializeField] private Color maxMarkColor;

    public void ResetMark()
    {
        currentMark = 0;
    }

    void Awake()
    {
        instance = this;
        SetMaxMark();
    }

    public void Plus()
    {
        currentMark += 1;

        if (currentMark > maxMark)
        {
            maxMark = currentMark;
            PlayerPrefs.SetInt("MaxMark", maxMark);
            MainLogic.Command(COMMAND_TYPE.BREAKING_RECORDS);
        }

        MainLogic.Command(COMMAND_TYPE.SCORE);
    }


    void SetMaxMark()
    {
        maxMark = PlayerPrefs.GetInt("MaxMark");
    }

    public void SetLiveTime(bool reset)
    {
        if (reset)
        {
            liveTime = 0;
        }
        else
        {
            liveTime += Time.deltaTime;
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = 25;
        GUI.color = currentMarkColor;
        GUI.Label(new Rect(20, 20, 250, 40), string.Format("当前分：{0}", currentMark.ToString()));
        GUI.color = UnityEngine.Color.red;
        GUI.Label(new Rect(20, 60, 250, 40), string.Format("最高分：{0}", maxMark.ToString()));
        GUI.color = UnityEngine.Color.black;
        GUI.Label(new Rect(20, 100, 250, 40), string.Format("生存时间：{0}", liveTime.ToString()));
    }
}
