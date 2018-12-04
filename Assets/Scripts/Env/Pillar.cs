using UnityEngine;

public class Pillar : MonoBehaviour
{
    private Transform _transform;
    private int _state = 0;

    public int State { get { return _state; } }

    void Awake() { _transform = this.transform; }

    void Update()
    {
        if (GameMgr.S.IsGameOver) return;
        _transform.Translate(new Vector3(-EnvGlobalValue.MoveSpeed * Time.deltaTime, 0, 0));
        if (_transform.position.x < -11)
        {
            GameMgr.S.pillMgr.DeletePillar(this);
        }
    }

    public void SetState(int state)
    {
        Vector3 pos = _transform.position;
        pos.y = 0.5f + state;
        this._state = state;
        _transform.position = pos;
    }

}
