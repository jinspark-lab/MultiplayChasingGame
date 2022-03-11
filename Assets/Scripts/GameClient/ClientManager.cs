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

    //public NetClient netClient;

    public GameObject playerPrefab;
    public GameObject chaserPrefab;

    public ClientSessionService clientSessionService;
    public ClientPlayService clientPlayService;
    public GameUIService gameUIService;

    private WebSocketClient webSocketClient;

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
        //netClient = new NetClient();
        //players = new Dictionary<int, GameObject>();
        //usernames = new Dictionary<int, string>();

        clientPlayService = new ClientPlayService();
        clientSessionService = new ClientSessionService();
        gameUIService = new GameUIService();
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
        if (InputManager.Singleton.GetInputKeyDown(KeyCode.F1))
        {
            clientSessionService.ConnectToServer(ClientSessionService.ConnectionInfo.GetLocalConnectionInfo());
        }
        else if (InputManager.Singleton.GetInputKeyDown(KeyCode.F2))
        {
            clientSessionService.DisconnectFromServer();
        }
        else if (InputManager.Singleton.GetInputKeyDown(KeyCode.F3))
        {
            // Play With Dummy. Server makes dummy
            SendDummyRequest();
        }
        else if (InputManager.Singleton.GetInputKeyDown(KeyCode.F4))
        {
            Debug.Log("WebSocket Try");
            webSocketClient = new WebSocketClient("localhost", 3000);
        }
        else if (InputManager.Singleton.GetInputKeyDown(KeyCode.F5))
        {
            webSocketClient.SendWebSocketMessage();
        }
        else if (InputManager.Singleton.GetInputKeyDown(KeyCode.F6))
        {
            webSocketClient.DisconnectWebSocket();
        }
    }


    private void MessageLoop()
    {
        while (!clientSessionService.GetMessageQueue().IsEmpty())
        {
            NetMessage netMessage = clientSessionService.PopMessage();
            if (netMessage.packetType == PacketType.CONNECT)
            {
                clientSessionService.ReceivePlayerConnected(netMessage.clientId, netMessage.message);
            }
            else if (netMessage.packetType == PacketType.DISCONNECT)
            {
                clientSessionService.ReceivePlayerDisconnected(netMessage.clientId, netMessage.message);
            }
            else if (netMessage.packetType == PacketType.GAME_INIT)
            {
                clientPlayService.InitGame(netMessage.message);
            }
            else if (netMessage.packetType == PacketType.PLAYER_MOVEMENT)
            {
                clientPlayService.ReceivePlayerMovement(netMessage.clientId, netMessage.message);
            }
            else if (netMessage.packetType == PacketType.PLAYER_CATCH)
            {
                clientPlayService.ReceivePlayerCatch(netMessage.clientId, netMessage.message);
            }
            else if (netMessage.packetType == PacketType.GAME_OVER)
            {
                clientPlayService.ReceiveGameOver(netMessage.message);
            }
        }
    }

    public int GetPlayerId()
    {
        return clientSessionService.GetNetClient().clientId;
    }

    public GameObject GetPlayerGameObject()
    {
        return clientPlayService.players[GetPlayerId()];
    }

    public GameObject GetPlayerGameObject(int clientId)
    {
        return clientPlayService.players[clientId];
    }

    public GameObject GetPlayerGameObject(string subobject)
    {
        return clientPlayService.players[GetPlayerId()].transform.Find(subobject).gameObject;
    }

    public void SendClientPacket(long packetId, PacketType packetType, string json)
    {
        clientSessionService.GetNetClient().SendData(packetId, packetType, json);
    }

    public void SendDummyRequest()
    {
        SendClientPacket(NetPacket.GeneratePacketIdTimestamp(), PacketType.DUMMY_PLAY, "");
    }

    public void OnApplicationQuit()
    {
        //DisconnectFromServer();
        clientSessionService.DisconnectFromServer();
    }

}
