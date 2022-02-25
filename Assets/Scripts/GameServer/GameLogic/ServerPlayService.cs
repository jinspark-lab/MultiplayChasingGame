using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPlayService
{

    public int netGameCapacity = 2;

    private Dictionary<int, GameObject> gameObjectPool;     //Whole Game Object pool server manages
    private Dictionary<int, NetPlayer> playerPool;          //Player Objects simulated in the server

    // Game States
    public string gameSessionId;
    public bool isGameStarted;
    public bool isGameEnded;
    public int chaserId;
    public int playerMaxScore;
    public int roundNo;

    private const int dummyId = 1001;

    public ServerPlayService()
    {
        // Initialize objects
        playerPool = new Dictionary<int, NetPlayer>();
        gameObjectPool = new Dictionary<int, GameObject>();

        gameSessionId = "GameSession" + GameMath.GetRandomInt(0, 10000);
        isGameStarted = false;
        isGameEnded = false;
        chaserId = 0;
        playerMaxScore = int.MinValue;
        roundNo = 0;
    }

    public bool IsPlayerDummy(int clientId)
    {
        return clientId == dummyId;
    }

    public bool IsPlayerExist(int clientId)
    {
        return playerPool.ContainsKey(clientId);
    }

    /***
     *  Initialize Player Session
     */
    public void InitPlayerSession(int clientId)
    {
        if (!playerPool.ContainsKey(clientId))
        {
            SpawnPlayer(clientId);
        }
    }

    public void InitDummySession()
    {
        if (!playerPool.ContainsKey(dummyId))
        {
            SpawnPlayer(dummyId);
        }
    }

    private void SpawnPlayer(int clientId)
    {
        float spawnX = (float)GameMath.GetRandomInt((int)InterfaceManager.Singleton.leftBoundary, (int)InterfaceManager.Singleton.rightBoundary);
        float spawnY = (float)GameMath.GetRandomInt((int)InterfaceManager.Singleton.downBoundary, (int)InterfaceManager.Singleton.upBoundary);

        GameObject newGameObject;
        if (chaserId == 0)
        {
            newGameObject = GameObject.Instantiate(ServerManager.Singleton.chaserPrefab, new Vector2(spawnX, spawnY), Quaternion.identity);
            chaserId = clientId;
        }
        else
        {
            newGameObject = GameObject.Instantiate(ServerManager.Singleton.playerPrefab, new Vector2(spawnX, spawnY), Quaternion.identity);
        }
        NetPlayer newPlayer = newGameObject.GetComponent<NetPlayer>();
        newPlayer.clientId = clientId;
        newPlayer.isChaser = (newPlayer.clientId == chaserId);
        newPlayer.score = 0;

        playerPool.Add(clientId, newPlayer);
        gameObjectPool.Add(clientId, newGameObject);

        LogManager.Singleton.WriteLog("[ServerManager] Player[" + newPlayer.clientId + "] Spawned at pos: " + spawnX + ", " + spawnY);
    }

    /***
     *  Start Game Session for each round
     */
    public void StartGameSession()
    {
        if (playerPool.Count >= netGameCapacity)
        {
            this.roundNo += 1;
            LogManager.Singleton.WriteLog("Start Game Session with GameSessionId: " + gameSessionId);

            List<GameModel.PlayerObject> playerObjects = new List<GameModel.PlayerObject>();
            foreach (int playerId in playerPool.Keys)
            {
                //Make JSON Packet
                GameModel.PlayerObject playerObject = new GameModel.PlayerObject(playerId, "Player[" + playerId + "]",
                    gameObjectPool[playerId].transform.position.x, gameObjectPool[playerId].transform.position.y, true, playerPool[playerId].score);
                playerObjects.Add(playerObject);
                //
            }

            GameModel.GameSession gameSession = new GameModel.GameSession(gameSessionId, playerObjects, chaserId, roundNo);
            string message = JsonUtility.ToJson(gameSession);
            //netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.GAME_INIT, message);
            ServerManager.Singleton.BroadcastClientsPacket(NetPacket.GeneratePacketIdTimestamp(), PacketType.GAME_INIT, message);
            isGameStarted = true;
        }
    }


    public void OnPlayerMoved(int clientId, GameModel.ControlObject data)
    {
        //LogManager.Singleton.WriteLog("[ServerManager] Player[" + clientId + "] Moved: " + data);
        playerPool[clientId].SetPlayerInput(data);
    }

    public void OnPlayerCatched(int chaserId, HashSet<int> catchedPlayerSet)
    {
        foreach (int playerId in playerPool.Keys)
        {
            //playerPool[playerId].score
            if (playerId == chaserId)
            {
                // Chaser Score ++
                playerPool[playerId].score += 100;
            }
            else
            {
                if (catchedPlayerSet.Contains(playerId))
                {
                    // Catched Score --
                    playerPool[playerId].score -= 10;
                }
                else
                {
                    // Survivor Score ++
                    playerPool[playerId].score += 10;
                }
                playerMaxScore = Math.Max(playerPool[playerId].score, playerMaxScore);
            }
        }
        BroadcastPlayerCatch(new GameModel.PlayerCatch(chaserId, new List<int>(catchedPlayerSet)));
    }

    public void BroadcastPlayerMovement(GameModel.PlayerMovement data)
    {
        string message = JsonUtility.ToJson(data);
        //netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_MOVEMENT, message);
        ServerManager.Singleton.BroadcastClientsPacket(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_MOVEMENT, message);

        //LogManager.Singleton.WriteLog("[ServerManager] Player(" + data.clientId + ") move broadcast : " + message);
    }

    public void BroadcastPlayerCatch(GameModel.PlayerCatch data)
    {
        string message = JsonUtility.ToJson(data);
        LogManager.Singleton.WriteLog("[ServerManager] Player[" + data.chaserId + "] Catched. message=" + message);
        //netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_CATCH, message);
        ServerManager.Singleton.BroadcastClientsPacket(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_CATCH, message);

        // Reset Game Session -> Restart game session with round + 1
        isGameStarted = false;
    }

    public bool CheckGameEnd()
    {
        return !playerPool.ContainsKey(chaserId) || (playerPool.ContainsKey(chaserId) && playerPool[chaserId].score >= 1000) || playerMaxScore >= 100;
    }

    public void ProcessGameEnd()
    {
        List<GameModel.PlayerObject> playerObjects = new List<GameModel.PlayerObject>();
        foreach (int playerId in playerPool.Keys)
        {
            //Make JSON Packet
            GameModel.PlayerObject playerObject = new GameModel.PlayerObject(playerId, "Player[" + playerId + "]",
                gameObjectPool[playerId].transform.position.x, gameObjectPool[playerId].transform.position.y, true, playerPool[playerId].score);
            playerObjects.Add(playerObject);
            //
        }

        GameModel.GameSession gameSession = new GameModel.GameSession(this.gameSessionId, playerObjects, chaserId, roundNo);
        string message = JsonUtility.ToJson(gameSession);
        LogManager.Singleton.WriteLog("[ServerManager] Game is Over : gameSession=" + gameSession.ToString());
        //netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.GAME_OVER, message);
        ServerManager.Singleton.BroadcastClientsPacket(NetPacket.GeneratePacketIdTimestamp(), PacketType.GAME_OVER, message);

        isGameEnded = true;
        //StartCoroutine(CloseAfterTime(10));
    }

    public void HandlePlayerDisconnect(int clientId)
    {
        // Remote disconnected player from playerpool
        GameObject.Destroy(playerPool[clientId]);
        GameObject.Destroy(gameObjectPool[clientId]);
        //
        playerPool.Remove(clientId);
        gameObjectPool.Remove(clientId);
        //
    }

    public void ClearPlayerPool()
    {
        playerPool.Clear();
        gameObjectPool.Clear();
    }

    //public IEnumerator CloseAfterTime(float t)
    //{
    //    yield return new WaitForSeconds(t);
    //    Application.Quit();
    //}

}
