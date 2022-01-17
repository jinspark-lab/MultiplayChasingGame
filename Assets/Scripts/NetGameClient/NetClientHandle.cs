using System.Collections;
using System.Collections.Generic;

public class NetClientHandle
{
    private NetClient netClient;

    public NetClientHandle(NetClient netClient)
    {
        this.netClient = netClient;
    }

    public void ConnectPacketHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        //LogManager.Singleton.WriteLog("[NetClientHandle] Connect Handler called. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netClient.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

    public void DisconnectPacketHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        //LogManager.Singleton.WriteLog("[NetClientHandle] Disconnect Handler called. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netClient.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

    public void PlayerInitPacketHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        //LogManager.Singleton.WriteLog("[NetClientHandle] Player Init Handler called. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netClient.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

    public void PlayerMovementPacketHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        //LogManager.Singleton.WriteLog("[NetClientHandle] PlayerMovement Handler called. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netClient.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

    public void PlayerCatchPacketHandler(int clientId, long packetId, PacketType packetType, string message)
    {
        //LogManager.Singleton.WriteLog("[NetClientHandle] ChaserCatch Handler called. clientId=" + clientId + ", packetId=" + packetId + ", packetType=" + packetType + ", message=" + message);

        netClient.netMessageQueue.PushMessage(new NetMessage(clientId, packetId, packetType, message));
    }

}
