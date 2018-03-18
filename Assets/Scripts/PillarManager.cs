using UnityEngine;
using System.Collections.Generic;

public class PillarManager : MonoBehaviour
{
    private static PillarManager instance;
    public static PillarManager S { get { return instance; } }
    private static List<Pillar> pillars = new List<Pillar>();

    [SerializeField] private Pillar pillarTemplate;
    private float oldTime = 0;

    void Awake() { instance = this; }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.S.IsGameOver)
        {
            return;
        }

        if (GameManager.S.isGameStart && Time.time - oldTime > 1.5f)
        {
            // this.CreatePillar();
            oldTime = Time.time;
        }
    }

    void CreatePillar()
    {
        Pillar pillar = Instantiate(this.pillarTemplate) as Pillar;

        pillar.transform.position = new Vector3(12, 0, 0);
        pillar.transform.localScale = Vector3.one;
        int height = Random.Range(0, 3);
        pillar.SetHeight(height);
        pillars.Add(pillar);
    }

    public void DeletePillar(Pillar _pillar)
    {
        pillars.Remove(_pillar);
        Object.Destroy(_pillar.gameObject);
    }

    // 清除所有柱子
    public void ClearPillars()
    {
        foreach (var pillar in pillars)
        {
            if (pillar.gameObject != null)
                Object.Destroy(pillar.gameObject);
        }

        pillars.Clear();
    }


    //优化 只计算离自己最近的柱子的状态
    // 这样做 柱子最多有9个状态 
    public int GetPillarState()
    {
        int ret = 0;
        float _dis = 1000;
        Pillar pillar = null;
        if (pillars.Count > 0)
        {
            for (int i = 0; i < pillars.Count; i++)
            {
                Vector3 pos = pillars[i].transform.position;
                if (pos.x > 0 && pos.x < _dis)
                {
                    _dis = pos.x;
                    pillar = pillars[i];
                }
            }
        }
        if (pillar != null)
        {
            // Debug.Log(pillar.transform.position.x);
            if (_dis < 2) ret = 10 * (pillar.height + 1);
            else if (_dis < 4) ret = 100 * (pillar.height + 1);
            else if (_dis < 6) ret = 1000 * (pillar.height + 1);
        }
        return ret;
    }


    public int GetNextPillarState()
    {
        int ret = 0;
        pillars.Sort((x, y) =>
        {
            float _x = x.transform.position.x;
            float _y = y.transform.position.x;
            return (int)_x - (int)_y;
        });
        float _dis = 1000;
        Pillar pillar = null;
        if (pillars.Count > 0)
        {
            for (int i = 0; i < pillars.Count; i++)
            {
                Vector3 pos = pillars[i].transform.position;
                if (pos.x > 0 && pos.x < _dis)
                {
                    _dis = pos.x;
                    pillar = pillars[i];
                }
            }
        }



        return ret;
    }


}
