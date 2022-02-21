using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class NetClient
{
    public static int BUFFER_SIZE = 4096;

    public string remoteAddress;
    public int port;
    public int clientId;

    // Network Transport
    private TcpClient socket;
    private NetworkStream networkStream;
    private byte[] receivedBuffer;

    public delegate void PacketHandler(int clientId, long packetId, PacketType packetType, string message);
    private Dictionary<int, PacketHandler> packetHandlers;
    private NetClientHandle netClientHandle;

    // Network MessageQueue
    public NetMessageQueue netMessageQueue;

    public NetClient()
    {
        remoteAddress = "127.0.0.1";
        port = 7777;
        clientId = 0;

        socket = new TcpClient()
        {
            ReceiveBufferSize = BUFFER_SIZE,
            SendBufferSize = BUFFER_SIZE
        };
        receivedBuffer = new byte[BUFFER_SIZE];

        InitializeHandlers();

        netMessageQueue = new NetMessageQueue();
    }

    private void InitializeHandlers()
    {
        netClientHandle = new NetClientHandle(this);
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)PacketType.CONNECT, netClientHandle.ConnectPacketHandler },
            { (int)PacketType.DISCONNECT, netClientHandle.DisconnectPacketHandler },
            { (int)PacketType.GAME_INIT, netClientHandle.GameInitPacketHandler },
            { (int)PacketType.PLAYER_MOVEMENT, netClientHandle.PlayerMovementPacketHandler },
            { (int)PacketType.PLAYER_CATCH, netClientHandle.PlayerCatchPacketHandler },
            { (int)PacketType.GAME_OVER, netClientHandle.GameOverPacketHandler }
        };
        LogManager.Singleton.WriteLog("[NetClient] Initialize Packet Handlers");
    }

    public void ConnectToServer(string remoteAddress, int port)
    {
        this.remoteAddress = remoteAddress;
        this.port = port;
        LogManager.Singleton.WriteLog("[NetClient] Connecting to host, IP=" + remoteAddress + ", Port=" + port);
        socket.BeginConnect(this.remoteAddress, this.port, ConnectCallback, socket);
    }

    private void ConnectCallback(IAsyncResult result)
    {
        // End Connection Trial after connection completion
        socket.EndConnect(result);
        if (!socket.Connected)
        {
            LogManager.Singleton.WriteLog("[NetClient] Socket connection failed.");
            return;
        }
        // Begin Network Stream
        networkStream = socket.GetStream();
        networkStream.BeginRead(receivedBuffer, 0, BUFFER_SIZE, ReceiveCallback, null);

        SendData(NetPacket.GeneratePacketIdTimestamp(), PacketType.CONNECT, "Hello Server");
        LogManager.Singleton.WriteLog("[NetClient] Socket Connected");
    }

    public void DisconnectFromServer()
    {
        SendData(NetPacket.GeneratePacketIdTimestamp(), PacketType.DISCONNECT, "Bye Server");
    }

    public void SendData(long packetId, PacketType packetType, string data)
    {
        try
        {
            //LogManager.Singleton.WriteLog("[NetClient] Send Data. packetId=" + packetId + ", packetType=" + packetType + ", data=" + data);
            if (socket != null)
            {
                /* Protocol
                 * PacketId(timestamp) | PacketType | Data
                 */
                NetPacket netPacket = new NetPacket();
                netPacket.Write(packetId);
                netPacket.Write((int)packetType);
                netPacket.Write(data);
                byte[] dataBytes = netPacket.ToArray();
                networkStream.BeginWrite(dataBytes, 0, dataBytes.Length, null, null);
            }
        }
        catch (Exception ex)
        {
            LogManager.Singleton.WriteLog("Error Sending data to Server. " + "Exception: " + ex.Message);
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            if (networkStream != null)
            {
                int byteLength = networkStream.EndRead(result);
                if (byteLength <= 0)
                {
                    Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(receivedBuffer, data, byteLength);
                HandleData(data);

                networkStream.BeginRead(receivedBuffer, 0, BUFFER_SIZE, ReceiveCallback, null);
            }
        }
        catch (Exception ex)
        {
            LogManager.Singleton.WriteLog("Error receiving TCP data: " + ex.Message);
        }
    }

    private void HandleData(byte[] data)
    {
        NetPacket netPacket = new NetPacket();
        netPacket.SetBytes(data);
        /*  Protocol
        *   PacketId(timestamp) | PacketType | Data
        */
        long packetId = netPacket.ReadLong();
        PacketType packetType = (PacketType)netPacket.ReadInt();
        string payload = netPacket.ReadString();
        InvokeClientPacketHandler(packetId, packetType, payload);
    }

    public void InvokeClientPacketHandler(long packetId, PacketType packetType, string message)
    {
        packetHandlers[(int)packetType].Invoke(clientId, packetId, packetType, message);
    }

    public void Disconnect()
    {
        socket.Close();
        networkStream = null;
        receivedBuffer = null;
        socket = null;
    }

}
