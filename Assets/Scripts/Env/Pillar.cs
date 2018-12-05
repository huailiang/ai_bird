using UnityEngine;

public class Pillar : MonoBehaviour
{
    private Transform _transform;
    private int _state = 0;

    private bool _recyle = false;

    public int State { get { return _state; } }

    void Awake() { _transform = this.transform; }

    void Update()
    {
        if (GameMgr.S.IsGameOver || _recyle) return;
        _transform.Translate(new Vector3(-EnvGlobalValue.MoveSpeed * Time.deltaTime, 0, 0));
        if (_transform.position.x < -11)
        {
            GameMgr.S.pillMgr.RecylePillar(this);
        }
    }

    public void SetState(int state)
    {
        _recyle = false;
        Vector3 pos = _transform.position;
        pos.y = 0.5f + state;
        this._state = state;
        _transform.position = pos;
    }

    public void Recyle()
    {
        _recyle = true;
        _transform.position = Vector3.one * 1000;
    }

}
