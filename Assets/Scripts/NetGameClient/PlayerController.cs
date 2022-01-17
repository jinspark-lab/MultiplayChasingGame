using System.Collections;
using UnityEngine;

/***
 * Clientside Component to control player object movement.
 * This script is attached to player Prefab.
 * 
 */
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public int clientId;

    public int inputCheckTime = 0;

    private Rigidbody2D rigidbody;
    private bool isCollided = false;
    public float speed = 20.0f;

    private bool isSpeedUp = false;

    public Animator playerAnimator;


    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private bool isLocalPlayer()
    {
        return ClientManager.Singleton.netClient.clientId == this.clientId;
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

        }
        else if (other.gameObject.tag == "Dummy")
        {
            Debug.Log("On Trigger Dummy");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Trigger Exit with : " + collision);
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

    private void FixedUpdate()
    {
        if (isLocalPlayer())
        {
            if (!Input.anyKey)
            {
                inputCheckTime = inputCheckTime + 1;
                if (inputCheckTime >= 100)
                {
                    // If No input every 100 frame, hits to 0
                    inputCheckTime = 0;
                }

                // FIXME: Fix animation logic to position change
                // Set Movement Animation false
                playerAnimator.SetBool("IsMoving", false);
            }
            else
            {
                // Movement
                MovePlayer();
                //

                // Set Movement Animation true
                playerAnimator.SetBool("IsMoving", true);
                inputCheckTime = 0;
            }
        }
    }

    private void MovePlayer()
    {
        bool up = Input.GetKey(KeyCode.W);
        bool down = Input.GetKey(KeyCode.S);
        bool left = Input.GetKey(KeyCode.A);
        bool right = Input.GetKey(KeyCode.D);

        if (EnvironmentManager.Singleton.isTesting)
        {
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

        }
        else
        {
            bool[] inputs = new bool[4];
            inputs[(int)GameModel.ControlObject.ControlInput.UP] = up;
            inputs[(int)GameModel.ControlObject.ControlInput.DOWN] = down;
            inputs[(int)GameModel.ControlObject.ControlInput.LEFT] = left;
            inputs[(int)GameModel.ControlObject.ControlInput.RIGHT] = right;
            ClientManager.Singleton.SendTransformInput(inputs);
        }

    }

    /***
     *  Receive Server-simulated player movement and replay.
     */
    public void ReceiveMovement(Vector2 pos)
    {
        this.transform.position = pos;
    }
}
