using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***
 * 
 * This class describes GameObject model information for communication
 * This can be replaced when you are using Protobuf as a protocol
 * 
 */
public class GameModel
{
    [Serializable]
    public class GameSession
    {
        public List<PlayerObject> playerObjects;

        public GameSession(List<PlayerObject> playerObjects)
        {
            this.playerObjects = playerObjects;
        }
    }

    [Serializable]
    public class PlayerObject
    {
        public int clientId;
        public string username;
        public float x;
        public float y;
        public bool connected;

        public PlayerObject(int clientId, string username, float x, float y, bool connected)
        {
            this.clientId = clientId;
            this.username = username;
            this.x = x;
            this.y = y;
            this.connected = connected;
        }
    }

    [Serializable]
    public class ControlObject
    {
        public enum ControlInput
        {
            UP,
            DOWN,
            LEFT,
            RIGHT
        };

        public int clientId;
        public bool[] inputs;

        public ControlObject(int clientId, bool[] inputs)
        {
            this.clientId = clientId;
            this.inputs = inputs;
        }
    }

    [Serializable]
    public class PlayerMovement
    {
        public int clientId;
        public float x;
        public float y;

        public PlayerMovement(int clientId, float x, float y)
        {
            this.clientId = clientId;
            this.x = x;
            this.y = y;
        }
    }

    [Serializable]
    public class PlayerCatch
    {
        public int clientId;
        public int chaserId;
        public List<int> playerIdList;
        public int nextChaserId;

        public PlayerCatch(int clientId, int chaserId, List<int> playerIdList)
        {
            this.clientId = clientId;
            this.chaserId = chaserId;
            this.playerIdList = playerIdList;
        }
        public PlayerCatch(int clientId, int chaserId, List<int> playerIdList, int nextChaserId)
        {
            this.clientId = clientId;
            this.chaserId = chaserId;
            this.playerIdList = playerIdList;
            this.nextChaserId = nextChaserId;
        }
    }

}
