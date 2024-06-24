using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using PimDeWitte.UnityMainThreadDispatcher;
using Unity.VisualScripting;
using UnityEngine;

public class HeartbeatTCPServer : MonoBehaviour
{
    private TcpListener tcpListener;
    private Thread tcpListenerThread;
    private bool isRunning = false;

    void Start()
    {
        StartServer(6666);
    }
    public void StartServer(int port)
    {
        if (!isRunning)
        {
            isRunning = true;
            tcpListenerThread = new Thread(() => ListenForIncomingRequests(port));
            tcpListenerThread.IsBackground = true;
            tcpListenerThread.Start();
            print("监听端口" + port);
        }
    }

    private void ListenForIncomingRequests(int port)
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            Byte[] bytes = new Byte[1024];
            while (isRunning)
            {
                using (TcpClient client = tcpListener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    int length;
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incomingData = new byte[length];
                        Array.Copy(bytes, 0, incomingData, 0, length);
                        string clientMessage = Encoding.UTF8.GetString(incomingData);

                        if (clientMessage != "FF")
                        {
                            print("接收消息: " + clientMessage);
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                        {
                            ChatSample.Instance.SendData(clientMessage);
                        });
                        }

                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            if (isRunning)
            {
                print("SocketException " + socketException.ToString());
            }
        }
        finally
        {
            StopServer();
        }
    }


    public void StopServer()
    {
        isRunning = false;
        tcpListener?.Stop();
        tcpListenerThread.Abort();
        print("停止监听");
    }
    void OnDestroy(){
        StopServer();
    }
}
