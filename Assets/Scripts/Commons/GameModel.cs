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
        public string gameSessionId;
        public List<PlayerObject> playerObjects;
        public int chaserId;
        public int roundId;

        public GameSession(string gameSessionId, List<PlayerObject> playerObjects, int chaserId, int roundId)
        {
            this.gameSessionId = gameSessionId;
            this.playerObjects = playerObjects;
            this.chaserId = chaserId;
            this.roundId = roundId;
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
        public int score;

        public PlayerObject(int clientId, string username, float x, float y, bool connected)
        {
            this.clientId = clientId;
            this.username = username;
            this.x = x;
            this.y = y;
            this.connected = connected;
        }
        public PlayerObject(int clientId, string username, float x, float y, bool connected, int score)
        {
            this.clientId = clientId;
            this.username = username;
            this.x = x;
            this.y = y;
            this.connected = connected;
            this.score = score;
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
        public int chaserId;
        public List<int> catchedIdList;

        public PlayerCatch(int chaserId, List<int> catchedIdList)
        {
            this.chaserId = chaserId;
            this.catchedIdList = catchedIdList;
        }
    }

    [Serializable]
    public class GameOver
    {
        public GameSession gameSession;

        public GameOver(GameSession gameSession)
        {
            this.gameSession = gameSession;
        }
    }

}
