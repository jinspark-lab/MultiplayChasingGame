using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPlayService
{
    public Dictionary<int, GameObject> players;
    private int gameRound = 0;

    public ClientPlayService()
    {
        players = new Dictionary<int, GameObject>();
        gameRound = 0;
    }

    public void InitGame(string data)
    {
        GameModel.GameSession gameSession = JsonUtility.FromJson<GameModel.GameSession>(data);
        List<string> gameInitMessageList = new List<string>();
        foreach (GameModel.PlayerObject playerObject in gameSession.playerObjects)
        {
            if (playerObject.connected)
            {
                if (!players.ContainsKey(playerObject.clientId))
                {
                    InitGamePlayer(playerObject, gameSession.chaserId);

                    string message = "Player[" + playerObject.clientId + "] is spawned at pos: " + playerObject.x + ", " + playerObject.y;
                    gameInitMessageList.Add(message);
                }
                else
                {
                    players[playerObject.clientId].GetComponent<PlayerController>().playerScore = playerObject.score;
                }
            }
        }
        // Init Game Round
        this.gameRound = gameSession.roundId;
        gameInitMessageList.Add("Round " + gameSession.roundId + " is Started!");

        // Update UI
        ClientManager.Singleton.gameUIService.UpdateUICanvas("Round : " + this.gameRound);
        ClientManager.Singleton.gameUIService.UpdateGameNotification(gameInitMessageList);
    }

    private void InitGamePlayer(GameModel.PlayerObject playerObject, int chaserId)
    {
        GameObject newPlayer;
        if (playerObject.clientId == chaserId)
        {
            // this player is a chaser
            newPlayer = GameObject.Instantiate(ClientManager.Singleton.chaserPrefab, new Vector2(playerObject.x, playerObject.y), Quaternion.identity);
        }
        else
        {
            newPlayer = GameObject.Instantiate(ClientManager.Singleton.playerPrefab, new Vector2(playerObject.x, playerObject.y), Quaternion.identity);
        }

        // Configure Player Object Management
        newPlayer.GetComponent<PlayerController>().clientId = playerObject.clientId;
        newPlayer.GetComponent<PlayerController>().playerName = "Player[" + playerObject.clientId + "]";
        newPlayer.GetComponent<PlayerController>().playerScore = playerObject.score;
        if (ClientManager.Singleton.GetPlayerId() != playerObject.clientId)
        {
            // Disable other player camera
            newPlayer.transform.Find("PlayerCam").gameObject.SetActive(false);
            newPlayer.transform.Find("UICanvas").gameObject.SetActive(false);
            newPlayer.transform.Find("MessageCanvas").gameObject.SetActive(false);
        }
        players.Add(playerObject.clientId, newPlayer);
    }

    public void RemoveGamePlayer(int clientId)
    {
        players.Remove(clientId);
    }


    // Client -> Server. Player Movement Inputs
    public void SendPlayerMovement(bool[] inputs)
    {
        int clientId = ClientManager.Singleton.GetPlayerId();

        GameModel.ControlObject controlObject = new GameModel.ControlObject(clientId, inputs);
        ClientManager.Singleton.SendClientPacket(NetPacket.GeneratePacketIdTimestamp(), PacketType.PLAYER_MOVEMENT, JsonUtility.ToJson(controlObject));
    }

    // Server -> Client. Player Movement
    public void ReceivePlayerMovement(int clientId, string data)
    {
        GameModel.PlayerMovement playerMovement = JsonUtility.FromJson<GameModel.PlayerMovement>(data);
        //LogManager.Singleton.WriteLog("[ClientManager] Client[" + playerMovement.clientId + "] is moved to :" + data);

        ClientManager.Singleton.GetPlayerGameObject(playerMovement.clientId).GetComponent<PlayerController>().ReceiveMovement(new Vector2(playerMovement.x, playerMovement.y));
    }

    public void ReceivePlayerCatch(int clientId, string data)
    {
        GameModel.PlayerCatch playerCatch = JsonUtility.FromJson<GameModel.PlayerCatch>(data);

        string catchedPlayerList = "";
        for (int i = 0; i < playerCatch.catchedIdList.Count; i++)
        {
            string pid = (i > 0) ? "," + playerCatch.catchedIdList[i] : "" + playerCatch.catchedIdList[i];
            catchedPlayerList += pid;
        }

        string catchMessage = "Client[" + catchedPlayerList + "] is catched :" + data + " by " + playerCatch.chaserId;

        List<string> catchedMessageList = new List<string>();
        catchedMessageList.Add(catchMessage);
        ClientManager.Singleton.gameUIService.UpdateGameNotification(catchedMessageList);
    }


    public void ReceiveGameOver(string data)
    {
        GameModel.GameOver gameOver = JsonUtility.FromJson<GameModel.GameOver>(data);

        ClientManager.Singleton.GetPlayerGameObject("UICanvas").GetComponent<UICanvas>().OpenResultPopUp("Game Result", () => Application.Quit());
    }
}
