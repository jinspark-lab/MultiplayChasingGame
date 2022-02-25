using UnityEngine;

public class GameLobby : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void RequestMatchmaking()
    {
        LogManager.Singleton.WriteLog("[GameLobby] Request Matchmaking");

    }

    public void CloseApplication()
    {
        Application.Quit();
    }

}
