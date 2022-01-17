using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***
 * Message Queue from Server/Client handler to Server/Client game interface.
 * Custom game server and game client should handle the message through the queue.
 */
public class NetMessageQueue
{
    private Queue<NetMessage> netMessages;

    public NetMessageQueue()
    {
        this.netMessages = new Queue<NetMessage>();
    }

    public bool IsEmpty()
    {
        return netMessages.Count <= 0;
    }

    public NetMessage[] GetAllNetMessages()
    {
        return netMessages.ToArray();
    }

    public void PushMessage(NetMessage message)
    {
        netMessages.Enqueue(message);
    }

    public NetMessage PopMessage()
    {
        return netMessages.Dequeue();
    }

    public NetMessage PeekMessage()
    {
        return netMessages.Peek();
    }

    public void Clear()
    {
        netMessages.Clear();
    }

}
