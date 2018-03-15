using UnityEngine;
using System.Collections;

public class Bird : MonoBehaviour {
	
	private float time = 0;
	[SerializeField] private GameObject mesh;
	[SerializeField] private Animation anim;

	private Vector3 flySpeed = Vector3.zero;

	void Update () 
	{
		if(!GameManager.S.isGameStart || GameManager.S.IsGameOver)
			return;

		if(time > 0) FlyUpUpdate();
		else VolplaneUpdate();
	}

	// 碰撞
	void OnTriggerEnter( Collider other )
	{
		if(other.gameObject.tag == "Score")
		{
			Scorers.S.Plus();
		}
		else
		{
			SoundManager.PlaySound(SoundType.Hit);
			GameManager.S.GameOver();
			Death();
		}
	}

	/// <summary>
	/// 设置小鸟初始位置
	/// </summary>
	public void ResetPos()
	{
		StopCoroutine("Decline");
	  	gameObject.GetComponent<Collider>().enabled = true;
		transform.position 		   = new Vector3(0, 1.5f, 0);
		mesh.transform.eulerAngles = new Vector3(0, 90, 0);
		anim.CrossFade("Idle", 0, PlayMode.StopAll);
	}

	/// <summary>
	/// 向上飞
	/// </summary>
	public void FlyUp()
	{
		anim.CrossFade("Run", 0f, PlayMode.StopAll);
		time = GlobalValue.FlyUpTime;
		SoundManager.PlaySound(SoundType.Fly);
	}

	// 向上飞
	void FlyUpUpdate()
	{
		anim["Run"].speed = 1;
		time -= Time.deltaTime;
		flySpeed.y = GlobalValue.FlyUpSpeed * Time.deltaTime;
		transform.Translate(flySpeed);
		//mesh.transform.eulerAngles = new Vector3(-20,90,0);
	}

	// 滑翔
	void VolplaneUpdate()
	{
		anim["Run"].speed = 0;
		anim["Run"].normalizedTime = 0.1f;
		flySpeed.y = -GlobalValue.FlyDownSpeed * Time.deltaTime;
		this.transform.Translate(flySpeed);
		//this.mesh.transform.eulerAngles = new Vector3(0,90,0);
	}

	void Death()
	{
		anim.Stop();
		GetComponent<Collider>().enabled = false;
		StopCoroutine("Decline");
		StartCoroutine("Decline");
	}

	IEnumerator Decline()
	{
		Vector3 startPos = transform.localPosition;
		startPos.z=-3;
		Vector3 endPos   = startPos;
		endPos.y = -3.5f;
		GameManager.S.isWaiting=true;
		yield return new WaitForSeconds(1f);
		
		SoundManager.PlaySound(SoundType.Death);

		float tempTime = 0f;
		while(tempTime < 0.5f)
		{
			tempTime += Time.deltaTime;
			transform.localPosition = Vector3.Lerp(startPos, endPos, tempTime / 0.5f);
			yield return 0;
		}
		
		yield return new WaitForSeconds(0.2f);
		GameManager.S.isWaiting=false;

	}
}
