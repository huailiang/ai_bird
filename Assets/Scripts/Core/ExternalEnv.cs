using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

public class ExternalEnv : BaseEnv
{
    Communicator communicator;

    Queue<BirdAction> m_oDataQueue = new Queue<BirdAction>();

    public override void Init()
    {
        base.Init();
        communicator = new Communicator();
        Parameters paramerters = new Parameters();
        paramerters.epsilon = epsilon;
        paramerters.gamma = gamma;
        paramerters.alpha = alpha;
        paramerters.states = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                paramerters.states.Add(i + 10 * j);
            }
        }
        string envMessage = JsonConvert.SerializeObject(paramerters, Formatting.Indented);
        communicator.Connect(envMessage, OnRecv);
        Debug.Log("****   socket is init   ****");
    }

    public override void OnUpdate(float delta)
    {
        Monitor.Enter(m_oDataQueue);
        if (m_oDataQueue.Count > 0)
        {
            BirdAction action = m_oDataQueue.Dequeue();
            GameManager.S.RespondByDecision(action);
            last_action = action;
        }
        Monitor.Exit(m_oDataQueue);
    }

    public override void OnTick()
    {
        int[] state = GetCurrentState();
        if (last_state != null)
        {
            UpdateState(last_state, state, last_r, last_action);
        }
        choose_action(state);

        last_r = 1;
        last_state = state;
    }

    public void OnRecv(string recv, int length)
    {
        int res = 0;
        if (!int.TryParse(recv, out res))
        {
            Debug.LogError("server chose action error " + recv);
        }
        else
        {
            BirdAction action = (BirdAction)res;
            Monitor.Enter(m_oDataQueue);
            m_oDataQueue.Enqueue(action);
            Debug.Log("rcv action:" + action);
            Monitor.Exit(m_oDataQueue);
        }
    }

    public override BirdAction choose_action(int[] state)
    {
        ChoiceNode node = new ChoiceNode();
        node.state = state;
        Send(node);
        return BirdAction.NONE;
    }

    public override void UpdateState(int[] state, int[] state_, int rewd, BirdAction action)
    {
        UpdateNode node = new UpdateNode();
        node.state = state;
        node.state_ = state_;
        node.rewd = rewd;
        node.action = (int)action;
        Send(node);
    }

    public override void OnRestart(int[] state)
    {
        EpsoleNode node = new EpsoleNode();
        node.state = state;
        Send(node);
    }
    private void Send(Protol paramer)
    {
        Send(paramer, true);
    }

    private void Send(Protol paramer, bool async)
    {
        string envMessage = JsonConvert.SerializeObject(paramer, Formatting.Indented);
        if (async)
        {
            communicator.Send(envMessage);
        }
        else
        {
            communicator.SendImm(envMessage);
        }
        // Debug.Log("recv: " + paramer.recv);
        if (paramer.recv)
        {
            communicator.Recive();
        }
    }

    public override void OnApplicationQuit()
    {
        if (communicator != null)
        {
            try
            {
                EexitNode node = new EexitNode();
                Send(node, false);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                communicator.Close();
            }
        }
    }

}
