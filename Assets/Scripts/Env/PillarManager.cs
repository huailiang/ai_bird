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

    void Update()
    {
        if (GameManager.S.IsGameOver)
        {
            oldTime = 0;
            return;
        }

        if (GameManager.S.isGameStart && Time.time - oldTime > 1.5f)
        {
#if ENABLE_PILLAR
            this.CreatePillar();
            oldTime = Time.time;
#endif
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
    
    public void ClearPillars()
    {
        foreach (var pillar in pillars)
        {
            if (pillar.gameObject != null)
                Object.Destroy(pillar.gameObject);
        }

        pillars.Clear();
    }

    
    public int[] GetPillarState()
    {
        int[] ret = new int[2];
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
            ret[0] = pillar.height;
        }
        ret[1] = Mathf.FloorToInt(_dis / 2f);
        return ret;
    }


}
