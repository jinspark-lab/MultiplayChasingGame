
using System;

/**
 *  Serverside Packet Handler Class
 *  
 *  1. This class handles network level processes and pass it to message queue in order to make Custom Game Server pull the messages.
 *  
 *  2. This class is implemented to get clientId and string message(JSON) as parameters.
 *  It would be changed to specific protocol for better performance.
 */
public class NetServerHandle
{
    private NetServer netServer;

    public NetServerHandle(NetServer netServer)
    {
        this.netServer = netServer;
    }

    public void ConnectHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        LogManager.Singleton.WriteLog("[NetServerHandle] New Client Connected. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netServer.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

    public void DisconnectHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        LogManager.Singleton.WriteLog("[NetServerHandle] Disconnect called. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netServer.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

    public void PlayerInitHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        LogManager.Singleton.WriteLog("[NetServerHandle] Player Spawn called. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netServer.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

    public void PlayerMovementHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        LogManager.Singleton.WriteLog("[NetServerHandle] PlayerMovement called. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netServer.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

    public void PlayerCatchHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        LogManager.Singleton.WriteLog("[NetServerHandle] ChaserCatch called. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netServer.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

}
