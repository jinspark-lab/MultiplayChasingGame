using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/***
 * 
 * Client manager to use NetGame custom netcode.
 * Write a code for client-side network business logic here.
 * [Important] Every Networking method from clientside should be done through ClientManager.
 * 
 */
public class ClientManager : MonoBehaviour
{
    public static ClientManager Singleton { get; protected set; }

    public NetClient netClient;

    public GameObject playerPrefab;
    private Dictionary<int, GameObject> players;
    private Dictionary<int, string> usernames;

    public class ConnectionInfo
    {
        public string address = "127.0.0.1";
        public int port = 7777;
        public string playerSessionId = "";

        public static ConnectionInfo GetLocalConnectionInfo()
        {
            ConnectionInfo connectionInfo = new ConnectionInfo();
            connectionInfo.address = "127.0.0.1";
            connectionInfo.port = 7777;
            return connectionInfo;
        }
    }

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

    // Start is called before the first frame update
    void Start()
    {
        netClient = new NetClient();
        players = new Dictionary<int, GameObject>();
        usernames = new Dictionary<int, string>();
    }

    // Update is called once per frame
    void Update()
    {
        // Interface Loop
        InterfaceLoop();
        //

        // Message Loop
        MessageLoop();
        //

    }

    private void InterfaceLoop()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ConnectToServer(ConnectionInfo.GetLocalConnectionInfo());
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            DisconnectFromServer();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {

        }
    }

    private void MessageLoop()
    {
        while (!netClient.netMessageQueue.IsEmpty())
        {
            NetMessage netMessage = netClient.netMessageQueue.PopMessage();
            if (netMessage.packetType == PacketType.CONNECT)
            {
                ReceivePlayerConnected(netMessage.clientId, netMessage.message);
            }
            else if (netMessage.packetType == PacketType.DISCONNECT)
            {
                ReceivePlayerDisconnected(netMessage.clientId, netMessage.message);
            }
            else if (netMessage.packetType == PacketType.PLAYER_INIT)
            {
                ReceiveGameInit(netMessage.message);
            }
            else if (netMessage.packetType == PacketType.PLAYER_MOVEMENT)
            {
                ReceivePlayerMovement(netMessage.clientId, netMessage.message);
            }
            else if (netMessage.packetType == PacketType.PLAYER_CATCH)
            {
                ReceivePlayerCatch(netMessage.clientId, netMessage.message);
            }
        }
    }

    public void ConnectToServer(ConnectionInfo connectionInfo)
    {
        LogManager.Singleton.WriteLog("[ClientManager] Connect To Server. IP=" + connectionInfo.address + ", Port=" + connectionInfo.port);
        netClient.ConnectToServer(connectionInfo.address, connectionInfo.port);
    }

    public void DisconnectFromServer()
    {
        LogManager.Singleton.WriteLog("[ClientManager] Disconnect...");
        netClient.DisconnectFromServer();
    }

    public void ReceivePlayerConnected(int clientId, string data)
    {
        netClient.clientId = int.Parse(data);           //Session ID -> Client ID
        LogManager.Singleton.WriteLog("[ClientManager] Client[" + netClient.clientId + "] is connected to Server. message:" + data);

        netClient.SendData(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_INIT, "User Info(" + netClient.clientId + ")" );
    }

    public void ReceivePlayerDisconnected(int clientId, string data)
    {
        GameModel.PlayerObject playerObject = JsonUtility.FromJson<GameModel.PlayerObject>(data);
        LogManager.Singleton.WriteLog("[ClientManager] Client[" + playerObject.clientId + "] is disconnected to Server. message:" + data);

        // Remove Player info from client side's player pool.
        players.Remove(playerObject.clientId);
        usernames.Remove(playerObject.clientId);

        // Dispose network resources if the player disconnects.
        if (netClient.clientId == playerObject.clientId && !playerObject.connected)
        {
            netClient.Disconnect();
        }
    }

    public void ReceiveGameInit(string data)
    {
        GameModel.GameSession gameSession = JsonUtility.FromJson<GameModel.GameSession>(data);
        Debug.Log("+++++This ClientId = " + netClient.clientId + "+++++");
        foreach(GameModel.PlayerObject playerObject in gameSession.playerObjects)
        {
            if (playerObject.connected)
            {
                Debug.Log("Player[" + playerObject.clientId + " Initialize - " + playerObject.x + ", " + playerObject.y);
                // Instantiate Prefab on main thread.
                GameObject newPlayer = Instantiate(playerPrefab, new Vector2(playerObject.x, playerObject.y), Quaternion.identity);
                newPlayer.GetComponent<PlayerController>().clientId = playerObject.clientId;
                if (netClient.clientId != playerObject.clientId)
                {
                    // Disable other player camera
                    newPlayer.transform.Find("PlayerCam").gameObject.SetActive(false);
                }
                //
                players.Add(playerObject.clientId, newPlayer);
                usernames.Add(playerObject.clientId, playerObject.username);

                LogManager.Singleton.WriteLog("[ClientManager] Player[" + playerObject.clientId + "] is spawned at pos: " + playerObject.x + ", " + playerObject.y);
            }
        }
    }

    // Client -> Server. Player Movement Inputs
    public void SendTransformInput(bool[] inputs)
    {
        int clientId = netClient.clientId;
        GameModel.ControlObject controlObject = new GameModel.ControlObject(clientId, inputs);
        netClient.SendData(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_MOVEMENT, JsonUtility.ToJson(controlObject));
    }

    // Server -> Client. Player Movement
    public void ReceivePlayerMovement(int clientId, string data)
    {
        GameModel.PlayerMovement playerMovement = JsonUtility.FromJson<GameModel.PlayerMovement>(data);
        LogManager.Singleton.WriteLog("[ClientManager] Client[" + playerMovement.clientId + "] is moved to :" + data);

        players[playerMovement.clientId].GetComponent<PlayerController>().ReceiveMovement(new Vector2(playerMovement.x, playerMovement.y));
    }

    public void ReceivePlayerCatch(int clientId, string data)
    {
        GameModel.PlayerCatch playerCatch = JsonUtility.FromJson<GameModel.PlayerCatch>(data);
        LogManager.Singleton.WriteLog("[ClientManager] Client[" + playerCatch.clientId + "] is catched :" + data + " by " + playerCatch.chaserId);


        //TODO: Catch Implementation
        foreach (int catchedPlayerId in playerCatch.playerIdList)
        {
            //TODO : Display who is caught
        }
        //TODO: Display who will be the next chaser

        //TODO: Stop the world 1sec ?

        //TODO: Scoring
    }

    public void OnApplicationQuit()
    {
        DisconnectFromServer();
    }

}
