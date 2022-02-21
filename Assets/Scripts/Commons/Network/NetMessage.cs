using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMessage
{
    public int clientId;            //Who send the message
    public long packetId;           //Unique packetId (Generally Timestamp)
    public PacketType packetType;   //PacketType to handle
    public string message;          //Message (JSON)

    public NetMessage(int clientId, long packetId, PacketType packetType, string message)
    {
        this.clientId = clientId;
        this.packetId = packetId;
        this.packetType = packetType;
        this.message = message;
    }

}
