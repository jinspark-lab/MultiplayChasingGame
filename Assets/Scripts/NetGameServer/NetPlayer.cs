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

    private void OnCollisionStay(Collision other)
    {
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
            Debug.Log(other);

            if (isChaser)
            {
                // TODO: Chaser -> Catch -> Broadcast -> Player -> Catched -> 
                //SimulateCatch();
                int targetId = other.gameObject.GetComponent<NetPlayer>().clientId;
                catchingSet.Add(targetId);
            }

        }
        else if (other.gameObject.tag == "Dummy")
        {
            //Debug.Log("On Trigger Dummy");

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Trigger Exit with : " + other);
        if (other.gameObject.tag == "Player")
        {
            int targetId = other.gameObject.GetComponent<NetPlayer>().clientId;
            if (catchingSet.Contains(targetId))
            {
                catchingSet.Remove(targetId);
            }
        }
        else if (other.gameObject.tag == "Dummy")
        {

        }

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Update per Fixed time-frame
    private void FixedUpdate()
    {
        if (!isCollided)
        {
            SimulateMovement();

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

    private void SimulateMovement()
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

            ServerManager.Singleton.BroadcastPlayerMovement(new GameModel.PlayerMovement(this.clientId, this.transform.position.x, this.transform.position.y));

            //FIXME: Fix this to handle it through Queue
            this.playerInput = null;
        }
    }

    /**
     * SimulateCatch function is only called from Chaser
     */
    private void SimulateCatch()
    {
        int chaserId = this.clientId;
        List<int> playerIdList = new List<int>();
        foreach(int targetId in catchingSet)
        {
            playerIdList.Add(targetId);
        }
        catchingSet.Clear();
        ServerManager.Singleton.BroadcastPlayerCatch(new GameModel.PlayerCatch(this.clientId, chaserId, playerIdList));
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
