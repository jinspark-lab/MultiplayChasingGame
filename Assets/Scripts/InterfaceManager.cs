using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Singleton { get; protected set; }

    // Game Interface

    // Field Boundary
    public float upBoundary;
    public float downBoundary;
    public float leftBoundary;
    public float rightBoundary;

    private void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Singleton = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        upBoundary = 5.0f;
        downBoundary = -5.0f;
        leftBoundary = -9.0f;
        rightBoundary = 9.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
