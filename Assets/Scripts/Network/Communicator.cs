using System.Net.Sockets;
using UnityEngine;
using System.Text;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Communicator
{
    private Socket sender;
    const string ip = "localhost";
    const int port = 5006;
    public delegate void RecvEventHandler(string recv, int len);
    public RecvEventHandler recvHandler;

    public void Connect(string envMessage, RecvEventHandler onrecv)
    {
        // Create a TCP/IP  socket
        recvHandler = onrecv;
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sender.Connect(ip, port);
        sender.Send(Encoding.ASCII.GetBytes(envMessage));
    }

    /// <summary>
    /// bytes前四位记录bytes的长度
    /// </summary>
    private byte[] AppendLength(byte[] input)
    {
        byte[] newArray = new byte[input.Length + 4];
        input.CopyTo(newArray, 4);
        System.BitConverter.GetBytes(input.Length).CopyTo(newArray, 0);
        return newArray;
    }


    public void SendImm(string msg)
    {
        byte[] data = AppendLength(Encoding.ASCII.GetBytes(msg));
        try
        {
            sender.Send(data);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void Send(string msg)
    {
        byte[] data = AppendLength(Encoding.ASCII.GetBytes(msg));
        try
        {
            sender.BeginSend(data, 0, data.Length, SocketFlags.None, asyncResult =>
            {
                sender.EndSend(asyncResult);
                // Debug.Log(string.Format("client send:{0}", msg));
            }, null);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }

    public void Recive()
    {
        byte[] data = new byte[1024];
        try
        {
            sender.BeginReceive(data, 0, data.Length, SocketFlags.None,
            asyncResult =>
            {
                try
                {

                    int length = sender.EndReceive(asyncResult);
                    string recv = Encoding.ASCII.GetString(data);
                    // Debug.Log(string.Format("recv server message：{0}  len:({1})", recv, length));
                    if (recv == "EXIT")
                    {
                        Close();
                    }
                    else
                    {
                        if (recvHandler != null) recvHandler(recv, length);
                    }
                }
                catch (SocketException e)
                {
                    if (e.ErrorCode == 10054)
                    {
                        Close();
                        Debug.Log("server has closed!");
#if UNITY_EDITOR
                        EditorApplication.isPlaying = false;
#endif
                    }
                    else
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }, null);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }
    }


    public void Close()
    {
        try
        {
            if (sender != null)
            {
                sender.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.LogError("socket close err:" + e.Message);
        }
    }

}
