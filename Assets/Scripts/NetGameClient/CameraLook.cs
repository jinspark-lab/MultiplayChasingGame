using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public float sensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Late Update is called after update is done
    void LateUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;            //Gather Unity Predefiend MouseX Axis.
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;            //Gather Unity Predefie

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);      //Cannot look further than 90 degrees

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
