using System;
using System.Text;
using WebSocketSharp;

/***
 * 
 * Module is designed to use w multiple objects.
 * Chatting module & Matchmaking & Ranking logic can use with multiple objects.
 * 
 */
public class WebSocketClient
{
    private WebSocket ws;


    public WebSocketClient(String url, int port)
    {
        ws = new WebSocket("ws://" + url + ":" + port);
        ws.OnMessage += (sender, e) =>
        {
            LogManager.Singleton.WriteLog("[WebClient] WebSocket Message : " + e.Data);

        };

        ws.Connect();
        LogManager.Singleton.WriteLog("[WebSocketClient] Connected");
    }

    public void SendWebSocketMessage()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Send("Hello");
        }
    }

    public void DisconnectWebSocket()
    {
        if (ws != null && ws.IsAlive)
        {
            LogManager.Singleton.WriteLog("[WebClient] WebSocket Disconnected... ");
            ws.Close();
        }
    }
}
