using UnityEngine;
using System.Collections;

public class Scorers : MonoBehaviour 
{
	private static Scorers instance;

	public static Scorers S { get { return instance; } }

	private int currentMark;
	private int maxMark;

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
		Plus(1);
	}

	public void Plus(int _mark)
	{
		currentMark += _mark;

		if(currentMark > maxMark)
		{
			maxMark = currentMark;
			PlayerPrefs.SetInt("MaxMark", maxMark);
			MainLogic.Command(maxMark, COMMAND_TYPE.BREAKING_RECORDS);
		}

		MainLogic.Command(_mark, COMMAND_TYPE.SCORE);
		SoundManager.PlaySound(SoundType.Plus);
	}

	void SetMaxMark()
	{
		maxMark = PlayerPrefs.GetInt("MaxMark");
	}

	void OnGUI()
	{
		GUI.skin.label.fontSize = 25;
		GUI.color = currentMarkColor;
		GUI.Label(new Rect(20, 20, 250, 40), string.Format("当前分：{0}", currentMark.ToString()));
		GUI.color = UnityEngine.Color.red;
		GUI.Label(new Rect(20, 60, 250, 40), string.Format("最高分：{0}", maxMark.ToString()));
	}
}
