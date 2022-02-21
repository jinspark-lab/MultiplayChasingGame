
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***
 * 
 * Server Manager to use Custom Netcode is here.
 * Write a code for business logic here.
 * [Important] Every server-side Server to Client networking logic should be done through ServerManager.
 * 
 */
public class ServerManager : MonoBehaviour
{
    public static ServerManager Singleton { get; protected set; }

    public bool isLocal = true;
    public GameObject playerPrefab;                 //PlayerPrefab to spawn
    public GameObject chaserPrefab;                 //ChaserPrefab to spawn


    public ServerSessionService serverSessionService;
    public ServerPlayService serverPlayService;

    //private NetServer netServer;

    //public int netGameCapacity = 2;

    //private Dictionary<int, GameObject> gameObjectPool;     //Whole Game Object pool server manages
    //private Dictionary<int, NetPlayer> playerPool;          //Player Objects simulated in the server

    //// Game States
    //public string gameSessionId;
    //public bool isGameStarted;
    //public bool isGameEnded;
    //public int chaserId;
    //public int playerMaxScore;
    //public int roundNo;

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Singleton = this;
    }

    private void Start()
    {
        // Set server FPS to 30.
        Application.targetFrameRate = 30;
        Application.runInBackground = true;

        serverSessionService = new ServerSessionService();
        serverPlayService = new ServerPlayService();

        //// Initialize objects
        //playerPool = new Dictionary<int, NetPlayer>();
        //gameObjectPool = new Dictionary<int, GameObject>();

        //// Server Start
        //netServer = new NetServer();
        //netServer.Start(7777, 0);

        //gameSessionId = "GameSession" + GameMath.GetRandomInt(0, 10000);
        //isGameStarted = false;
        //isGameEnded = false;
        //chaserId = 0;
        //playerMaxScore = int.MinValue;
        //roundNo = 0;
    }

    private void FixedUpdate()
    {
        // Server Tick
        if (!serverPlayService.isGameEnded)
        {
            // Server Management
            if (!serverPlayService.isGameStarted)
            {
                serverPlayService.StartGameSession();
            }
            //

            // Message Loop
            MessageLoop();
            //

            // Check Game End
            if (serverPlayService.isGameStarted && serverPlayService.CheckGameEnd())
            {
                serverPlayService.ProcessGameEnd();
            }
        }
    }

    private void MessageLoop()
    {
        while (!serverSessionService.GetMessageQueue().IsEmpty())
        {
            NetMessage netMessage = serverSessionService.GetMessageQueue().PopMessage();

            if (netMessage.packetType == PacketType.CONNECT)
            {
                serverSessionService.OnPlayerConnected(netMessage.clientId);
            }
            else if (netMessage.packetType == PacketType.DISCONNECT)
            {
                serverSessionService.OnPlayerDisconnected(netMessage.clientId);
            }
            else if (netMessage.packetType == PacketType.PLAYER_INIT)
            {
                serverPlayService.InitPlayerSession(netMessage.clientId);
            }
            else if (netMessage.packetType == PacketType.PLAYER_MOVEMENT)
            {
                GameModel.ControlObject controlObject = JsonUtility.FromJson<GameModel.ControlObject>(netMessage.message);
                serverPlayService.OnPlayerMoved(netMessage.clientId, controlObject);
            }
        }
    }

    ///***
    // *  Start Game Session for each round
    // */
    //private void StartGameSession()
    //{
    //    if (playerPool.Count >= netGameCapacity)
    //    {
    //        this.roundNo += 1;
    //        LogManager.Singleton.WriteLog("Start Game Session with GameSessionId: " + gameSessionId);

    //        List<GameModel.PlayerObject> playerObjects = new List<GameModel.PlayerObject>();
    //        foreach (int playerId in playerPool.Keys)
    //        {
    //            //Make JSON Packet
    //            GameModel.PlayerObject playerObject = new GameModel.PlayerObject(playerId, "Player[" + playerId + "]",
    //                gameObjectPool[playerId].transform.position.x, gameObjectPool[playerId].transform.position.y, true, playerPool[playerId].score);
    //            playerObjects.Add(playerObject);
    //            //
    //        }

    //        GameModel.GameSession gameSession = new GameModel.GameSession(gameSessionId, playerObjects, chaserId, roundNo);
    //        string message = JsonUtility.ToJson(gameSession);
    //        netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.GAME_INIT, message);
    //        isGameStarted = true;
    //    }
    //}

    //public void OnPlayerConnected(int clientId)
    //{
    //    LogManager.Singleton.WriteLog("[ServerManager] On Player Connected. clientId=" + clientId);

    //    //TODO: Validation Logic

    //    //Broadcast Newly joined player's sessionID
    //    netServer.SendMessage(clientId, NetPacket.GeneratePacketIdTimestamp(), PacketType.CONNECT, "" + clientId);
    //}

    //public void OnPlayerDisconnected(int clientId)
    //{
    //    // Remote disconnected player from playerpool
    //    Destroy(playerPool[clientId]);
    //    Destroy(gameObjectPool[clientId]);
    //    //
    //    playerPool.Remove(clientId);
    //    gameObjectPool.Remove(clientId);
    //    //

    //    GameModel.PlayerObject playerObject = new GameModel.PlayerObject(clientId, "Player[" + clientId + "]", 0, 0, false);
    //    string message = JsonUtility.ToJson(playerObject);

    //    netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.DISCONNECT, message);
    //    LogManager.Singleton.WriteLog("[ServerManager] Player[" + clientId + "] disconnected");
    //}

    ///***
    // *  Initialize Player Session
    // */
    //private void InitPlayerSession(int clientId)
    //{
    //    if (!playerPool.ContainsKey(clientId))
    //    {
    //        SpawnPlayer(clientId);
    //    }
    //}

    //private void SpawnPlayer(int clientId)
    //{
    //    float spawnX = (float)GameMath.GetRandomInt((int)InterfaceManager.Singleton.leftBoundary, (int)InterfaceManager.Singleton.rightBoundary);
    //    float spawnY = (float)GameMath.GetRandomInt((int)InterfaceManager.Singleton.downBoundary, (int)InterfaceManager.Singleton.upBoundary);

    //    GameObject newGameObject;
    //    if (chaserId == 0)
    //    {
    //        newGameObject = Instantiate(chaserPrefab, new Vector2(spawnX, spawnY), Quaternion.identity);
    //        chaserId = clientId;
    //    }
    //    else
    //    {
    //        newGameObject = Instantiate(playerPrefab, new Vector2(spawnX, spawnY), Quaternion.identity);
    //    }
    //    NetPlayer newPlayer = newGameObject.GetComponent<NetPlayer>();
    //    newPlayer.clientId = clientId;
    //    newPlayer.isChaser = (newPlayer.clientId == chaserId);
    //    newPlayer.score = 0;

    //    playerPool.Add(clientId, newPlayer);
    //    gameObjectPool.Add(clientId, newGameObject);

    //    LogManager.Singleton.WriteLog("[ServerManager] Player[" + newPlayer.clientId + "] Spawned at pos: " + spawnX + ", " + spawnY);
    //}

    //public void OnPlayerMoved(int clientId, GameModel.ControlObject data)
    //{
    //    //LogManager.Singleton.WriteLog("[ServerManager] Player[" + clientId + "] Moved: " + data);
    //    playerPool[clientId].SetPlayerInput(data);
    //}

    //public void OnPlayerCatched(int chaserId, HashSet<int> catchedPlayerSet)
    //{
    //    foreach (int playerId in playerPool.Keys)
    //    {
    //        //playerPool[playerId].score
    //        if (playerId == chaserId)
    //        {
    //            // Chaser Score ++
    //            playerPool[playerId].score += 100;
    //        }
    //        else
    //        {
    //            if (catchedPlayerSet.Contains(playerId))
    //            {
    //                // Catched Score --
    //                playerPool[playerId].score -= 10;
    //            }
    //            else
    //            {
    //                // Survivor Score ++
    //                playerPool[playerId].score += 10;
    //            }
    //            playerMaxScore = Math.Max(playerPool[playerId].score, playerMaxScore);
    //        }
    //    }
    //    BroadcastPlayerCatch(new GameModel.PlayerCatch(chaserId, new List<int>(catchedPlayerSet)));
    //}

    //public void BroadcastPlayerMovement(GameModel.PlayerMovement data)
    //{
    //    string message = JsonUtility.ToJson(data);
    //    netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_MOVEMENT, message);
    //    //LogManager.Singleton.WriteLog("[ServerManager] Player(" + data.clientId + ") move broadcast : " + message);
    //}

    //public void BroadcastPlayerCatch(GameModel.PlayerCatch data)
    //{
    //    string message = JsonUtility.ToJson(data);
    //    LogManager.Singleton.WriteLog("[ServerManager] Player[" + data.chaserId + "] Catched. message=" + message);
    //    netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_CATCH, message);

    //    // Reset Game Session -> Restart game session with round + 1
    //    isGameStarted = false;
    //}

    //public bool CheckGameEnd()
    //{
    //    return !playerPool.ContainsKey(chaserId) || (playerPool.ContainsKey(chaserId) && playerPool[chaserId].score >= 1000) || playerMaxScore >= 100;
    //}

    //public void ProcessGameEnd()
    //{
    //    List<GameModel.PlayerObject> playerObjects = new List<GameModel.PlayerObject>();
    //    foreach (int playerId in playerPool.Keys)
    //    {
    //        //Make JSON Packet
    //        GameModel.PlayerObject playerObject = new GameModel.PlayerObject(playerId, "Player[" + playerId + "]",
    //            gameObjectPool[playerId].transform.position.x, gameObjectPool[playerId].transform.position.y, true, playerPool[playerId].score);
    //        playerObjects.Add(playerObject);
    //        //
    //    }

    //    GameModel.GameSession gameSession = new GameModel.GameSession(this.gameSessionId, playerObjects, chaserId, roundNo);
    //    string message = JsonUtility.ToJson(gameSession);
    //    LogManager.Singleton.WriteLog("[ServerManager] Game is Over : gameSession=" + gameSession.ToString());
    //    netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.GAME_OVER, message);

    //    isGameEnded = true;
    //    StartCoroutine(CloseAfterTime(10));
    //}

    //public IEnumerator CloseAfterTime(float t)
    //{
    //    yield return new WaitForSeconds(t);
    //    Application.Quit();
    //}

    public void BroadcastClientsPacket(long packetId, PacketType packetType, string json)
    {
        serverSessionService.GetNetServer().Broadcast(packetId, packetType, json);
    }

    public void OnApplicationQuit()
    {
        serverPlayService.ClearPlayerPool();
        LogManager.Singleton.WriteLog("[ServerManager] On Server Quit");
    }
}
