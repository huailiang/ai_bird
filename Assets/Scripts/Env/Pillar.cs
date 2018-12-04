using UnityEngine;

public class Pillar : MonoBehaviour
{
    private Transform mTransform;
    public int height = 0;

    void Awake() { mTransform = this.transform; }

    void Update()
    {
        if (GameMgr.S.IsGameOver) return;
        mTransform.Translate(new Vector3(-EnvGlobalValue.MoveSpeed * Time.deltaTime, 0, 0));
        if (mTransform.position.x < -11)
        {
            PillarManager.S.DeletePillar(this);
        }
    }

    public void SetHeight(int height)
    {
        Vector3 pos = mTransform.position;
        pos.y = height * 2;
        this.height = height;
        mTransform.position = pos;
    }
}
