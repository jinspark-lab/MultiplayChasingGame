using System;
using System.Collections.Generic;
using System.Text;

public enum PacketType
{
    CONNECT,
    DISCONNECT,
    PLAYER_INIT,
    PLAYER_MOVEMENT,
    PLAYER_CATCH,
    PLAYER_RESPAWN,
    GAMEOVER
}

/***
 * This class is for Customize Net Packet.
 * Use this class for the future development
 */
public class NetPacket
{
    // Reference : https://github.com/benlap/Unity-Headless-Server-and-Client-Prototype/blob/master/GameServer/Assets/Scripts/Packet.cs

    private List<byte> buffer;
    private byte[] readableBuffer;
    private int readPos;

    public NetPacket()
    {
        buffer = new List<byte>();
        readPos = 0;

    }

    public static long GeneratePacketIdTimestamp()
    {
        long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        return milliseconds;
    }

    // Write Byte Array to the packet and set it to buffer 
    public void SetBytes(byte[] data)
    {
        Write(data);
        readableBuffer = buffer.ToArray();
    }

    // Read byte range from the packet for the given length
    public byte[] ReadBytes(int length)
    {
        if (buffer.Count > readPos)
        {
            byte[] value = buffer.GetRange(readPos, length).ToArray();
            readPos += length;
            return value;
        }
        throw new Exception("Could not read value of type byte[]");
    }

    // Read integer value from the packet. Read 4 byte from it
    public int ReadInt()
    {
        if (buffer.Count > readPos)
        {
            int value = BitConverter.ToInt32(readableBuffer, readPos);
            readPos += 4;
            return value;
        }
        throw new Exception("Could not read value of type int32");
    }

    public long ReadLong()
    {
        if (buffer.Count > readPos)
        {
            long value = BitConverter.ToInt64(readableBuffer, readPos);
            readPos += 8;
            return value;
        }
        throw new Exception("Could not read value of type int64");
    }

    // Read string value from the packet. the length to read should be appended before text
    public string ReadString()
    {
        try
        {
            int length = ReadInt();
            string value = Encoding.ASCII.GetString(readableBuffer, readPos, length);
            readPos += length;
            return value;
        }
        catch
        {
            throw new Exception("Could not read value of string");
        }
    }

    // Add Byte Array to the packet
    public void Write(byte[] data)
    {
        buffer.AddRange(data);
    }

    // Add Int to the packet
    public void Write(int data)
    {
        buffer.AddRange(BitConverter.GetBytes(data));
    }

    // Add Long to the packet
    public void Write(long data)
    {
        buffer.AddRange(BitConverter.GetBytes(data));
    }

    // Add String to the packet
    public void Write(string data)
    {
        Write(data.Length);
        buffer.AddRange(Encoding.ASCII.GetBytes(data));
    }

    // Get the byte array of the packet
    public byte[] ToArray()
    {
        readableBuffer = buffer.ToArray();
        return readableBuffer;
    }

    // Return the length of packet
    public int Length()
    {
        return buffer.Count;
    }

    // Return the length of unread data
    public int UnreadLength()
    {
        return Length() - readPos;
    }

    public void Reset()
    {
        buffer.Clear();
        readableBuffer = null;
        readPos = 0;
    }
}