using UnityEngine;
using System.Collections;

public class UVScroller : MonoBehaviour {

	private bool  isScroller;
	private float scrollSpeedX = 0;
	private float offsetX;
	private Material mat;

	void Awake ()
	{
		SetMaterial();
	}

	void SetMaterial()
	{
		Renderer rend =GetComponent<Renderer>();
		if (rend) mat = rend.material;
		else mat = null;
	}
	
	void Update () 
	{
		if (mat == null || !isScroller) return;
		
		offsetX = offsetX + Time.deltaTime*scrollSpeedX;
		if (offsetX>1) offsetX = offsetX - 1;

        mat.mainTextureOffset = new Vector2(offsetX, 0);
	}

	public void Reset()
	{
		if(mat != null)
		{
			mat.mainTextureOffset = Vector2.zero;
			offsetX = 0;
		}
	}

	public void Stop()
	{
		isScroller = false;
	}

	public void Move()
	{
		Move(0.2f);
	}

	public void Move(float speed)
	{
		isScroller = true;
		scrollSpeedX = speed;
	}
}
