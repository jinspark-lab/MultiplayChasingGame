
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
            else if (netMessage.packetType == PacketType.DUMMY_PLAY)
            {
                // Move it to API callback pattern
                serverPlayService.InitDummySession();
            }
        }
    }

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
