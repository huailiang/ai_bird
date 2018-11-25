using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExternalEnv : BaseEnv
{
    bool init = false;
    Socket sender;
    byte[] messageHolder;
    const int messageLength = 10240;

    public override void Init()
    {
        base.Init();
        messageHolder = new byte[messageLength];
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

        // Create a TCP/IP  socket
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sender.Connect("localhost", 5006);
        sender.Send(Encoding.ASCII.GetBytes(envMessage));
        init = true;
        Debug.Log("****   socket is init   ****");
    }

    public override void OnTick()
    {
        int state = GetCurrentState();
        if (last_state != -1)
        {
            UpdateState(last_state, state, last_r, last_action);
        }
        BirdAction action = choose_action(state);
        GameManager.S.RespondByDecision(action);
        last_r = 1;
        last_state = state;
        last_action = action;
    }

    public override BirdAction choose_action(int state)
    {
        if (init)
        {
            ChoiceNode node = new ChoiceNode();
            node.state = state;
            int res = 0;
            string sr = Send(node, true);
            if (!int.TryParse(sr, out res))
            {
#if UNITY_EDITOR
                Debug.Log("Unity Stop Run");
                Debug.LogError("server chose action error " + sr);
                EditorApplication.isPlaying = false;
#endif
            }
            else
            {
                Debug.Log("send state: " + state + " & rcv action:" + (BirdAction)res);
            }
            return (BirdAction)res;
        }
        return 0;
    }

    public override void UpdateState(int state, int state_, int rewd, BirdAction action)
    {
        if (init)
        {
            UpdateNode node = new UpdateNode();
            node.state = state;
            node.state_ = state_;
            node.rewd = rewd;
            node.action = (int)action;
            Send(node);
        }
    }

    public override void OnRestart(int state)
    {
        if (init)
        {
            EpsoleNode node = new EpsoleNode();
            node.state = state;
            Send(node);
        }
    }

    private string Send(Protol paramer, bool recv = false)
    {
        try
        {
            string envMessage = JsonConvert.SerializeObject(paramer, Formatting.Indented);
            // Debug.Log("send: " + paramer.Code + " msg: " + envMessage);
            sender.Send(AppendLength(Encoding.ASCII.GetBytes(envMessage)));
            if (recv)
            {
                int location = sender.Receive(messageHolder);
                string res = Encoding.ASCII.GetString(messageHolder, 0, location); ;
                if (res == "EXIT")
                {
                    init = false;
                    sender.Close();
                    Debug.Log("Socket closed");
                }
                return res;
            }
            else
            {
                return string.Empty;
            }
        }
        catch (SocketException e)
        {
            Debug.LogWarning(e.Message);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            return string.Empty;
        }
    }

    private byte[] AppendLength(byte[] input)
    {
        byte[] newArray = new byte[input.Length + 4];
        input.CopyTo(newArray, 4);
        System.BitConverter.GetBytes(input.Length).CopyTo(newArray, 0);
        return newArray;
    }

    public override void OnApplicationQuit()
    {
        if (init && sender != null)
        {
            try
            {
                EexitNode node = new EexitNode();
                Send(node);
                sender.Close();
                Debug.Log("Socket closed");
            }
            catch (SocketException e)
            {
                Debug.LogError("socket close err:" + e.Message);
            }
        }
    }

}
