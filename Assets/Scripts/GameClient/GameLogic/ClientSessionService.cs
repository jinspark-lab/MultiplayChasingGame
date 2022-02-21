using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSessionService
{
    private NetClient netClient;

    public ClientSessionService()
    {
        this.netClient = new NetClient();
    }

    public class ConnectionInfo
    {
        public string address = "127.0.0.1";
        public int port = 7777;
        public string playerSessionId = "";

        public static ConnectionInfo GetLocalConnectionInfo()
        {
            ConnectionInfo connectionInfo = new ConnectionInfo();
            connectionInfo.address = "127.0.0.1";
            connectionInfo.port = 7777;
            return connectionInfo;
        }
    }

    public void ConnectToServer(ConnectionInfo connectionInfo)
    {
        LogManager.Singleton.WriteLog("[ClientManager] Connect To Server. IP=" + connectionInfo.address + ", Port=" + connectionInfo.port);
        netClient.ConnectToServer(connectionInfo.address, connectionInfo.port);
    }

    public void DisconnectFromServer()
    {
        LogManager.Singleton.WriteLog("[ClientManager] Disconnect...");
        netClient.DisconnectFromServer();
    }

    public void ReceivePlayerConnected(int clientId, string data)
    {
        netClient.clientId = int.Parse(data);           //Session ID -> Client ID
        LogManager.Singleton.WriteLog("[ClientManager] Client[" + netClient.clientId + "] is connected to Server. message:" + data);

        netClient.SendData(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_INIT, "User Info(" + netClient.clientId + ")");
    }

    public void ReceivePlayerDisconnected(int clientId, string data)
    {
        GameModel.PlayerObject playerObject = JsonUtility.FromJson<GameModel.PlayerObject>(data);
        LogManager.Singleton.WriteLog("[ClientManager] Client[" + playerObject.clientId + "] is disconnected to Server. message:" + data);

        // Remove Player info from client side's player pool.
        ClientManager.Singleton.clientPlayService.RemoveGamePlayer(playerObject.clientId);

        //usernames.Remove(playerObject.clientId);

        // Dispose network resources if the player disconnects.
        if (netClient.clientId == playerObject.clientId && !playerObject.connected)
        {
            netClient.Disconnect();
        }
    }

    public NetClient GetNetClient()
    {
        return netClient;
    }

    public NetMessageQueue GetMessageQueue()
    {
        return netClient.netMessageQueue;
    }

    public NetMessage PopMessage()
    {
        return netClient.netMessageQueue.PopMessage();
    }

}
