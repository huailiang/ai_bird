using UnityEngine;

public class Bird : MonoBehaviour
{
    [SerializeField] private GameObject mesh;
    [SerializeField] private Animation anim;
    private float time = 0;
    private Vector3 flySpeed = Vector3.zero;

    void Update()
    {
        if (!GameMgr.S.IsGameStart || GameMgr.S.IsGameOver) return;
        if (time > 0) FlyUpUpdate();
        else PadUpdate();
        
        if (transform.position.y < -EnvGlobalValue.BirdBounds || 
            transform.position.y > EnvGlobalValue.BirdBounds)
        {
            GameMgr.S.GameOver();
            Death();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Score"))
        {
            GameMgr.S.OnScore();
        }
        else
        {
            GameMgr.S.GameOver();
            Death();
        }
    }

    public void ResetPos()
    {
        StopCoroutine("Decline");
        gameObject.GetComponent<Collider>().enabled = true;
        transform.position = new Vector3(0, EnvGlobalValue.BirdInitY, 0);
        mesh.transform.eulerAngles = new Vector3(0, 90, 0);
        anim.CrossFade("Idle", 0, PlayMode.StopAll);
    }

    public void FlyUp()
    {
        anim.CrossFade("Run", 0f, PlayMode.StopAll);
        time = EnvGlobalValue.FlyUpTime;
    }
    
    void FlyUpUpdate()
    {
        anim["Run"].speed = 1;
        time -= Time.deltaTime;
        flySpeed.y = EnvGlobalValue.FlyUpSpeed * Time.deltaTime;
        transform.Translate(flySpeed);
    }
    
    void PadUpdate()
    {
        anim["Run"].speed = 0;
        anim["Run"].normalizedTime = 0.1f;
        flySpeed.y = -EnvGlobalValue.FlyDownSpeed * Time.deltaTime;
        this.transform.Translate(flySpeed);
    }

    void Death()
    {
        anim.Stop();
        GetComponent<Collider>().enabled = false;
    }
    
    public int GetState()
    {
        int v = (int)transform.position.y + 4;
        return Mathf.Clamp(v, 0, 8);
    }

}
