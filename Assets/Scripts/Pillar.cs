using UnityEngine;
using System.Collections;

public class Pillar : MonoBehaviour 
{
	private Transform mTransform;
	
	void Awake() { mTransform = this.transform; }

	void Update () 
	{
		if(GameManager.S.IsGameOver)
			return;
		
		mTransform.Translate(new Vector3(-GlobalValue.MoveSpeed * Time.deltaTime, 0, 0));
		
		if(mTransform.position.x < -11)
		{
			PillarManager.S.DeletePillar(this);
		}
	}
	
	public void SetHeight(int height)
	{
		Vector3 pos = mTransform.position;
		pos.y = height * 2;
		mTransform.position = pos;
	}
}
