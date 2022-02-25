using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/***
 * Server-side GameObject for Player Prefab
 * 
 */
[RequireComponent(typeof(Rigidbody2D))]
public class NetPlayer : MonoBehaviour
{
    public int clientId;
    public int score;

    private Rigidbody2D rigidbody;
    private bool isCollided = false;
    public float speed = 20.0f;
    private bool isSpeedUp = false;

    public bool isChaser = false;
    private HashSet<int> catchingSet;

    private GameModel.ControlObject playerInput;

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        catchingSet = new HashSet<int>();
    }

    private void OnCollisionEnter(Collision other)
    {
        StartCoroutine(WaitForMovementAfterCollision());
    }

    private void OnCollisionExit(Collision other)
    {
        isCollided = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Item")
        {
            Debug.Log(other);
            StartCoroutine(WaitForIterBuffDuration());
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "Player")
        {
            if (isChaser)
            {
                int targetId = other.gameObject.GetComponent<NetPlayer>().clientId;
                Debug.Log("Chaser Trigger Enter. targetId=" + targetId);
                catchingSet.Add(targetId);
            }
            else
            {
                Debug.Log("Player Trigger Enter. targetId=" + other.gameObject.GetComponent<NetPlayer>().clientId);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            int targetId = other.gameObject.GetComponent<NetPlayer>().clientId;
            if (catchingSet.Contains(targetId))
            {
                catchingSet.Remove(targetId);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Update per Fixed time-frame
    private void FixedUpdate()
    {
        if (ServerManager.Singleton.serverPlayService.isGameStarted)
        {
            if (ServerManager.Singleton.serverPlayService.IsPlayerDummy(this.clientId))
            {
                SimulateDummyMovement();
            }
            else
            {
                SimulatePlayerMovement();
            }

            if (isChaser)
            {
                SimulateCatch();
            }
        }
    }

    public void SetPlayerInput(GameModel.ControlObject controlObject)
    {
        this.playerInput = controlObject;
    }

    private void SimulatePlayerMovement()
    {
        if (playerInput != null)
        {
            bool up = playerInput.inputs[(int)GameModel.ControlObject.ControlInput.UP];
            bool down = playerInput.inputs[(int)GameModel.ControlObject.ControlInput.DOWN];
            bool left = playerInput.inputs[(int)GameModel.ControlObject.ControlInput.LEFT];
            bool right = playerInput.inputs[(int)GameModel.ControlObject.ControlInput.RIGHT];

            float horizontal = 0.0f;
            float vertical = 0.0f;

            if (up && transform.position.y <= InterfaceManager.Singleton.upBoundary)
            {
                vertical += 1.0f;
            }
            if (down && transform.position.y >= InterfaceManager.Singleton.downBoundary)
            {
                vertical -= 1.0f;
            }
            if (left && transform.position.x >= InterfaceManager.Singleton.leftBoundary)
            {
                horizontal -= 1.0f;
            }
            if (right && transform.position.x <= InterfaceManager.Singleton.rightBoundary)
            {
                horizontal += 1.0f;
            }

            Vector2 Move = new Vector2(horizontal, vertical);
            Move *= speed * Time.deltaTime;
            this.transform.Translate(Move);

            //Debug.Log("[NetPlayer] Player[" + clientId + "] SImulate Movement pos: " + this.transform.position);

            ServerManager.Singleton.serverPlayService.BroadcastPlayerMovement(new GameModel.PlayerMovement(this.clientId, this.transform.position.x, this.transform.position.y));
            this.playerInput = null;
        }
    }

    /***
     * 
     * 
     * SimulateDummyMovement -> MessageQueue -> ServerPlayService.OnPlayerMoved -> SimulatePlayerMovement()
     * 
     */
    public void SimulateDummyMovement()
    {
        bool up = GameMath.GetRandomInt(0, 10) % 2 == 0;
        bool down = GameMath.GetRandomInt(0, 10) % 2 == 0;
        bool left = GameMath.GetRandomInt(0, 10) % 2 == 0;
        bool right = GameMath.GetRandomInt(0, 10) % 2 == 0;


        float horizontal = 0.0f;
        float vertical = 0.0f;

        if (up && transform.position.y <= InterfaceManager.Singleton.upBoundary)
        {
            vertical += 2.0f;
        }
        if (down && transform.position.y >= InterfaceManager.Singleton.downBoundary)
        {
            vertical -= 2.0f;
        }
        if (left && transform.position.x >= InterfaceManager.Singleton.leftBoundary)
        {
            horizontal -= 2.0f;
        }
        if (right && transform.position.x <= InterfaceManager.Singleton.rightBoundary)
        {
            horizontal += 2.0f;
        }

        Vector2 Move = new Vector2(horizontal, vertical);
        Move *= speed * Time.deltaTime;
        this.transform.Translate(Move);

        ServerManager.Singleton.serverPlayService.BroadcastPlayerMovement(new GameModel.PlayerMovement(this.clientId, this.transform.position.x, this.transform.position.y));
    }

    /**
     * SimulateCatch function is only called from Chaser
     */
    private void SimulateCatch()
    {
        if (catchingSet.Count > 0)
        {
            int chaserId = this.clientId;
            HashSet<int> playerIdSet = new HashSet<int>();
            foreach (int targetId in catchingSet)
            {
                playerIdSet.Add(targetId);
            }
            catchingSet.Clear();

            if (playerIdSet.Count > 0)
            {
                ServerManager.Singleton.serverPlayService.OnPlayerCatched(chaserId, playerIdSet);
            }
        }
    }

    IEnumerator WaitForMovementAfterCollision()
    {
        isCollided = true;
        Debug.Log("Is Collided Start - " + isCollided);
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Is Collided End - " + isCollided);
        isCollided = false;
    }

    IEnumerator WaitForIterBuffDuration()
    {
        isSpeedUp = true;
        Debug.Log("Speed Up Start");
        yield return new WaitForSeconds(5.0f);
        Debug.Log("Speed Up End");
        isSpeedUp = false;
    }

}
