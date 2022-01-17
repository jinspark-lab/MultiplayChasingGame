using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyController : MonoBehaviour
{

    public void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("On Triggered " + collision.tag);
        //Destroy(gameObject);
    }

}
