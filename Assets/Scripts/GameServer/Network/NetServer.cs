using System.Collections;
using System.Collections.Generic;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

/***
 *  Asynchronous TCP Server
 *  
 *  This class only handles the logic of listen server. After accepting client connection, it handles session by NetSession
 */
public class NetServer
{
    public int Port { get; private set; }
    public int MaxCapacity { get; private set; }            // Maximum TCP Connection capacity. 0 means unlimited

    // Network Transport
    private TcpListener serverListener;
    private Dictionary<int, NetSession> sessionPool = new Dictionary<int, NetSession>();

    public delegate void PacketHandler(int clientId, long packetId, PacketType packetType, string message);
    private Dictionary<int, PacketHandler> packetHandlers;
    private NetServerHandle netServerHandle;

    // Network Message
    public NetMessageQueue netMessageQueue;

    // Start is called before the first frame update
    public void Start(int port, int maxCapacity)
    {
        LogManager.Singleton.WriteLog("Starting Server...");
        Port = port;
        MaxCapacity = maxCapacity;

        InitializeHandlers();

        netMessageQueue = new NetMessageQueue();

        StartTcpListen();
        LogManager.Singleton.WriteLog("Server started on IP - " + GetServerIP() + ", Port - " + Port);
    }

    private string GetServerIP()
    {
        if (ServerManager.Singleton.isLocal)
        {
            return "127.0.0.1";
        }
        else
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            LogManager.Singleton.WriteLog("No network adapters with an IPv4 address in the System!");
            throw new System.Exception("No network adapters with an IPv4 address in the System!");
        }
    }

    private void InitializeHandlers()
    {
        netServerHandle = new NetServerHandle(this);
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)PacketType.CONNECT, netServerHandle.ConnectHandler },
            { (int)PacketType.DISCONNECT, netServerHandle.DisconnectHandler },
            { (int)PacketType.PLAYER_INIT, netServerHandle.PlayerInitHandler },
            { (int)PacketType.PLAYER_MOVEMENT, netServerHandle.PlayerMovementHandler },
            { (int)PacketType.PLAYER_CATCH, netServerHandle.PlayerCatchHandler },
            { (int)PacketType.DUMMY_PLAY, netServerHandle.DummyPlayHandler }
        };
    }

    private void StartTcpListen()
    {
        serverListener = new TcpListener(IPAddress.Any, Port);
        serverListener.Start();
        serverListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectionCallback), null);
        LogManager.Singleton.WriteLog("Start TCP Listen...");
    }

    private void TcpConnectionCallback(IAsyncResult result)
    {
        LogManager.Singleton.WriteLog("[NetServer] TCP Connection Accepted");
        //Accept New Connection and begin accepting again
        TcpClient client = serverListener.EndAcceptTcpClient(result);
        serverListener.BeginAcceptTcpClient(new AsyncCallback(TcpConnectionCallback), null);

        LogManager.Singleton.WriteLog("[NetServer] TCP Session Pool. Count:" + sessionPool.Count);
        if (MaxCapacity == 0 || sessionPool.Count < MaxCapacity)
        {
            int newSessionId = sessionPool.Count + 1;
            sessionPool.Add(newSessionId, new NetSession(this, newSessionId, client));
            LogManager.Singleton.WriteLog("Client[" + newSessionId + "] connected completed");
        }
        else
        {
            LogManager.Singleton.WriteLog("Failed to Connect. Server Maximum capacity Full");
        }
    }

    public Dictionary<int, NetSession> GetSessionPool()
    {
        return this.sessionPool;
    }

    public void InvokeServerPacketHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        packetHandlers[(int)packetType].Invoke(clientId, packetId, packetType, message);
    }

    public void SendMessage(int clientId, long packetId, PacketType packetType, string message)
    {
        sessionPool[clientId].SendData(packetId, packetType, message);
    }

    public void Broadcast(long packetId, PacketType packetType, string message)
    {
        foreach (var item in GetSessionPool())
        {
            int sessionId = item.Key;
            NetSession netSession = item.Value;
            netSession.SendData(packetId, packetType, message);
        }
    }

    public void Close()
    {
        sessionPool.Clear();
        serverListener.Stop();
        LogManager.Singleton.WriteLog("Server Closed.");
    }
}
