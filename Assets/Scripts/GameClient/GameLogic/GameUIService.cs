using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUIService
{
    public void UpdateUICanvas(string message)
    {
        //players[netClient.clientId].transform.Find("UICanvas").gameObject.GetComponent<UICanvas>().UpdatePlayerInfoText(message);
        ClientManager.Singleton.GetPlayerGameObject("UICanvas").GetComponent<UICanvas>().UpdatePlayerInfoText(message);
    }

    public void UpdateGameNotification(List<string> messageList)
    {
        ClientManager.Singleton.GetPlayerGameObject("MessageCanvas/GameNotification").GetComponent<GameNotification>().UpdateNotificationText(messageList);
    }

}
