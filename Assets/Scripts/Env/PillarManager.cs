using UnityEngine;
using System.Collections.Generic;

public class PillarMgr
{
    private Pillar currPillar;

    private Queue<Pillar> recyle_pool = new Queue<Pillar>();

    private List<Pillar> run_pool = new List<Pillar>();

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

        if (GameMgr.S.IsGameStart && Time.time - oldTime > 2.5f)
        {
#if ENABLE_PILLAR
            CreatePillar();
            oldTime = Time.time;
#endif
        }
    }

    void CreatePillar()
    {
        Pillar pillar;
        if (recyle_pool.Count > 0)
        {
            pillar = recyle_pool.Dequeue();
        }
        else
        {
            pillar = GameObject.Instantiate(pillarTemplate) as Pillar;
        }
        currPillar = pillar;
        pillar.transform.position = new Vector3(EnvGlobalValue.PillarBornX, 0, 0);
        pillar.transform.localScale = Vector3.one;
        int state = Random.Range(0, 2);
        pillar.SetState(state);
        run_pool.Add(pillar);
    }

    public void RecylePillar(Pillar _pillar)
    {
        run_pool.Remove(_pillar);
        recyle_pool.Enqueue(_pillar);
        _pillar.Recyle();
    }

    public void Clear()
    {
        for (int i = 0; i < run_pool.Count; i++)
        {
            recyle_pool.Enqueue(run_pool[i]);
            run_pool[i].Recyle();
        }
        run_pool.Clear();
        currPillar = null;
    }


    public int[] GetPillarState()
    {
        int[] ret = new int[2];
        if (currPillar != null)
        {
            ret[0] = currPillar.State;
            float _dis = currPillar.transform.position.x;
            ret[1] = Mathf.FloorToInt(_dis / 2f);
        }
        return ret;
    }


}
