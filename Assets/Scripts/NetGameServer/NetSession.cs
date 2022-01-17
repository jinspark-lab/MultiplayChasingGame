
using System;
using System.Net.Sockets;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

/***
 *  Connection Class to manage client socket
 *  
 *  This class is Client connection managed from Game Server side.
 *  After accepting TCP Connection, Persistent connection is handled in NetSession class
 */
public class NetSession
{
    public static int BUFFER_SIZE = 4096;

    public int clientId;
    public TcpClient clientSocket;
    private NetServer netServer;
    private NetworkStream networkStream;

    private byte[] receiveBuffer;

    public NetSession(NetServer netServer, int clientId, TcpClient tcpClient)
    {
        this.clientId = clientId;
        this.clientSocket = tcpClient;
        this.netServer = netServer;
        this.networkStream = tcpClient.GetStream();
        this.receiveBuffer = new byte[BUFFER_SIZE];

        networkStream.BeginRead(receiveBuffer, 0, BUFFER_SIZE, ReceiveCallback, null);
        LogManager.Singleton.WriteLog("[NetSession] Net Session Started. clientId=" + clientId);
    }

    public void SendData(long packetId, PacketType packetType, string data)
    {
        try
        {
            if (clientSocket != null)
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
            LogManager.Singleton.WriteLog("Error Sending data to player Id - " + clientId + ", Exception: " + ex.Message);
        }
    }

    // This method is for socket to receive message
    private void ReceiveCallback(IAsyncResult result)
    {
        if (networkStream != null)
        {
            int byteLength = networkStream.EndRead(result);
            if (byteLength <= 0)
            {
                LogManager.Singleton.WriteLog("[NetSession] byteLength <= 0");
                Disconnect();
                return;
            }
            HandleData(receiveBuffer, byteLength);
            networkStream.BeginRead(receiveBuffer, 0, BUFFER_SIZE, ReceiveCallback, null);
        }
    }

    private void HandleData(byte[] data, int byteLength)
    {
        NetPacket netPacket = new NetPacket();
        netPacket.SetBytes(data);
        /*  Protocol
        *   PacketId(timestamp) | PacketType | Data
        */
        long packetId = netPacket.ReadLong();
        PacketType packetType = (PacketType)netPacket.ReadInt();
        string payload = netPacket.ReadString();
        netServer.InvokeServerPacketHandler(clientId, packetId, packetType, payload);
    }

    public void Disconnect()
    {
        clientSocket.Close();
        networkStream = null;
        clientSocket = null;
    }
}
