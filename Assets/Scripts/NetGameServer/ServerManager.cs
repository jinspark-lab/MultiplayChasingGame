
using System;
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

    private NetServer netServer;

    public int netGameCapacity = 2;

    private Dictionary<int, GameObject> gameObjectPool;     //Whole Game Object pool server manages
    private Dictionary<int, NetPlayer> playerPool;          //Player Objects simulated in the server

    // Game States
    public bool isGameStarted;
    public int currentChaser;
    public int nextChaser;

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

        // Initialize objects
        playerPool = new Dictionary<int, NetPlayer>();
        gameObjectPool = new Dictionary<int, GameObject>();

        // Server Start
        netServer = new NetServer();
        netServer.Start(7777, netGameCapacity);

        isGameStarted = false;
    }

    private void FixedUpdate()
    {
        // Server Tick

        // Server Management
        if (!isGameStarted)
        {
            StartGameSession();
        }
        //

        // Message Loop
        MessageLoop();
        //
    }

    private void MessageLoop()
    {
        while (!netServer.netMessageQueue.IsEmpty())
        {
            NetMessage netMessage = netServer.netMessageQueue.PopMessage();

            if (netMessage.packetType == PacketType.CONNECT)
            {
                OnPlayerConnected(netMessage.clientId);
            }
            else if (netMessage.packetType == PacketType.DISCONNECT)
            {
                OnPlayerDisconnected(netMessage.clientId);
            }
            else if (netMessage.packetType == PacketType.PLAYER_INIT)
            {
                OnPlayerInit(netMessage.clientId);
            }
            else if (netMessage.packetType == PacketType.PLAYER_MOVEMENT)
            {
                GameModel.ControlObject controlObject = JsonUtility.FromJson<GameModel.ControlObject>(netMessage.message);
                OnPlayerMoved(netMessage.clientId, controlObject);
            }
            else if (netMessage.packetType == PacketType.PLAYER_CATCH)
            {
                //GameModel.ControlObject controlObject = JsonUtility.FromJson<GameModel.ControlObject>(netMessage.message);
            }
        }
    }


    private void StartGameSession()
    {
        if (playerPool.Count >= netGameCapacity)
        {
            List<GameModel.PlayerObject> playerObjects = new List<GameModel.PlayerObject>();
            foreach (int playerId in playerPool.Keys)
            {
                //Make JSON Packet
                GameModel.PlayerObject playerObject = new GameModel.PlayerObject(playerId, "Player[" + playerId + "]",
                    gameObjectPool[playerId].transform.position.x, gameObjectPool[playerId].transform.position.y, true);
                playerObjects.Add(playerObject);
                //
            }
            GameModel.GameSession gameSession = new GameModel.GameSession(playerObjects);
            string message = JsonUtility.ToJson(gameSession);
            netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_INIT, message);
            isGameStarted = true;
        }
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
        // Remote disconnected player from playerpool
        // TODO: Check whether to manage NetObject in server
        Destroy(playerPool[clientId]);
        Destroy(gameObjectPool[clientId]);
        //
        playerPool.Remove(clientId);
        gameObjectPool.Remove(clientId);
        //

        GameModel.PlayerObject playerObject = new GameModel.PlayerObject(clientId, "Player[" + clientId + "]", 0, 0, false);
        string message = JsonUtility.ToJson(playerObject);

        netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.DISCONNECT, message);
        LogManager.Singleton.WriteLog("[ServerManager] Player[" + clientId + "] disconnected");
    }

    private void OnPlayerInit(int clientId)
    {
        if (!playerPool.ContainsKey(clientId))
        {
            SpawnPlayer(clientId);
        }
    }

    private void SpawnPlayer(int clientId)
    {
        float spawnX = (float)GameMath.GetRandomInt((int)InterfaceManager.Singleton.leftBoundary, (int)InterfaceManager.Singleton.rightBoundary);
        float spawnY = (float)GameMath.GetRandomInt((int)InterfaceManager.Singleton.downBoundary, (int)InterfaceManager.Singleton.upBoundary);

        GameObject newGameObject = Instantiate(playerPrefab, new Vector2(spawnX, spawnY), Quaternion.identity);
        NetPlayer newPlayer = newGameObject.GetComponent<NetPlayer>();
        newPlayer.clientId = clientId;

        playerPool.Add(clientId, newPlayer);
        gameObjectPool.Add(clientId, newGameObject);

        LogManager.Singleton.WriteLog("[ServerManager] Player[" + newPlayer.clientId + "] Spawned at pos: " + spawnX + ", " + spawnY);
    }

    public void OnPlayerMoved(int clientId, GameModel.ControlObject data)
    {
        //LogManager.Singleton.WriteLog("[ServerManager] Player[" + clientId + "] Moved: " + data);
        playerPool[clientId].SetPlayerInput(data);
    }

    public void BroadcastPlayerMovement(GameModel.PlayerMovement data)
    {
        string message = JsonUtility.ToJson(data);
        netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_MOVEMENT, message);
        //LogManager.Singleton.WriteLog("[ServerManager] Player(" + data.clientId + ") move broadcast : " + message);
    }

    public void BroadcastPlayerCatch(GameModel.PlayerCatch data)
    {
        // Designate Next Chaser from Server
        int nextChaserId = data.playerIdList[GameMath.GetRandomInt(0, data.playerIdList.Count)];
        data.nextChaserId = nextChaserId;
        this.nextChaser = nextChaserId;
        //

        string message = JsonUtility.ToJson(data);
        netServer.Broadcast(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_CATCH, message);
    }

    public void OnApplicationQuit()
    {
        playerPool.Clear();
        gameObjectPool.Clear();
        LogManager.Singleton.WriteLog("[ServerManager] On Server Quit");
    }

}
