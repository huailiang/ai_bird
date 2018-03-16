using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reinforcement 
 {

	 private static Reinforcement s;

	 public static Reinforcement S{	get	{	if(s==null)	s=new Reinforcement();	return s;	}	}

	bool action =false;

	// greedy police
	float epsilon=0.9f;

	// learning rate
	float alpha=0.1f;

	//discount factor
	float gamma=0.9f;

	int max_episodes=10;

	int last_r=0,last_state=0;
	bool last_action=false;
	

	Dictionary<int,Row> q_table;

	public class Row
	{
		public float pad;
		public float stay;
	}


	public void Init()
	{
		Build_Q_Table();
		MainLogic.AddCommandHook(COMMAND_TYPE.GAME_START,OnStart);
		MainLogic.AddCommandHook(COMMAND_TYPE.SCORE,OnScore);
		MainLogic.AddCommandHook(COMMAND_TYPE.GAME_OVERD,OnDied);
	}

	void OnStart(object o)
	{
		last_r=0;
		last_state=0;
	}

	void OnScore(object arg)
	{
		Debug.Log("score");
		last_r=1;
	}

	void OnDied(object arg)
	{
		Debug.LogWarning("died");
		last_r=-100;
	}

	public void OnTick()
	{
		int state = GetCurrentState();
		if(last_state!=0)
		{
			//cul last loop
			UpdateState(last_state,state,last_r,last_action);
		}

		//do next loop
		bool action = choose_action(state);
		GameManager.S.RespondByDecision(action);
		last_r=0;
		last_state=state;
		last_action=action;
	}


	public int GetCurrentState()
	{
		int p_st = PillarManager.S.GetPillarState();
		int b_st = GameManager.S.mainBird.GetState();
		// Debug.Log("pillar: "+p_st+" bird:"+b_st);
		return p_st+b_st;
	}


	//dic:key is state, val is action
	public void Build_Q_Table()
	{
		q_table=new Dictionary<int,Row>();
		for(int i=0;i<5;i++)
		{
			for(int j=0;j<5;j++)
			{
				for(int k=0;k<5;k++)
				{
					for(int l=0;l<5;l++)
					{
						Row row=new Row(){pad=0f,stay=0f};
						int v=i+j*10+k*100+l*1000;
						Debug.Log("i:"+i+" j:"+j+" k:"+k+" l:"+l+" v:"+v);
						q_table.Add(v, row);
					}
				}
			}
		}
	}


	public bool choose_action(int state)
	{
		if(q_table==null || Random.Range(0.0f,1.0f) > epsilon)
		{
		   	return Random.Range(0,2)>0;
		}
		else
		{
			Row row=q_table[state];
			return row.pad>row.stay;
		}
	}


	public void UpdateState(int state,int state_,int rewd,bool action)
	{
		if(q_table!=null)
		{
			if(rewd!=0)
			{
				Debug.Log("rewd:"+rewd+" state:"+state+" _state:"+state_);
			}
			Row row=q_table[state_];
			float max=row.pad>row.stay?row.pad:row.stay;
			float q_target=rewd+gamma*max;
			float q_predict= action?q_table[state].pad:q_table[state].stay;
			float add = alpha*(q_target-q_predict);
			if(action)
			{
			 	q_table[state].pad+=add;
			}
			else
			{
				q_table[state].stay+=add;
			}
		}
	}

}
