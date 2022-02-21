using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform target;
    private Transform cameraTransform;

    // Start is called before the first frame update
    void Start()
    {
        cameraTransform = GetComponent<Transform>();
    }

    // Called after Update
    void LateUpdate()
    {
        if (!target)
        {
            GameObject targetObject = GameObject.FindGameObjectWithTag("Player");
            if (targetObject != null)
            {
                target = targetObject.transform;
            }
            else
            {
                return;
            }
        }
        cameraTransform.position = new Vector3(target.position.x, target.position.y, target.position.z - 5.0f);
        cameraTransform.LookAt(target);
    }
}
