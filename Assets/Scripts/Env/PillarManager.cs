using UnityEngine;
using System.Collections.Generic;

public class PillarMgr 
{
    private static List<Pillar> pillars = new List<Pillar>();
    private Pillar pillarTemplate;
    private float oldTime = 0;
    
    public PillarMgr(Pillar temp)
    {
        pillarTemplate = temp;
    }

    public void Update(float delta)
    {
        if (GameMgr.S.IsGameOver)
        {
            oldTime = 0;
            return;
        }

        if (GameMgr.S.IsGameStart && Time.time - oldTime > 1.5f)
        {
#if ENABLE_PILLAR
            CreatePillar();
            oldTime = Time.time;
#endif
        }
    }

    void CreatePillar()
    {
        Pillar pillar = GameObject.Instantiate(pillarTemplate) as Pillar;
        pillar.transform.position = new Vector3(EnvGlobalValue.PillarBornX, 0, 0);
        pillar.transform.localScale = Vector3.one;
        int state = Random.Range(0, 2);
        pillar.SetState(state);
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
            ret[0] = pillar.State;
        }
        ret[1] = Mathf.FloorToInt(_dis / 2f);
        return ret;
    }


}
