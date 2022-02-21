using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSessionService
{
    private NetServer netServer;


    public ServerSessionService()
    {
        // Server Start
        netServer = new NetServer();
        netServer.Start(7777, 0);
    }

    public NetServer GetNetServer()
    {
        return netServer;
    }

    public void OnPlayerConnected(int clientId)
    {
        LogManager.Singleton.WriteLog("[ServerManager] On Player Connected. clientId=" + clientId);

        //TODO: Validation Logic

        //Broadcast Newly joined player's sessionID
        netServer.SendMessage(clientId, NetPacket.GeneratePacketIdTimestamp(), PacketType.CONNECT, "" + clientId);
    }

    public void OnPlayerDisconnected(int clientId)
    {
        //// Remote disconnected player from playerpool
        //Destroy(playerPool[clientId]);
        //Destroy(gameObjectPool[clientId]);
        ////
        //playerPool.Remove(clientId);
        //gameObjectPool.Remove(clientId);
        ////
        ///
        ServerManager.Singleton.serverPlayService.HandlePlayerDisconnect(clientId);

        GameModel.PlayerObject playerObject = new GameModel.PlayerObject(clientId, "Player[" + clientId + "]", 0, 0, false);
        string message = JsonUtility.ToJson(playerObject);

        netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.DISCONNECT, message);
        LogManager.Singleton.WriteLog("[ServerManager] Player[" + clientId + "] disconnected");
    }

    public NetMessageQueue GetMessageQueue()
    {
        return netServer.netMessageQueue;
    }

    public NetMessage PopMessage()
    {
        return netServer.netMessageQueue.PopMessage();
    }


}
