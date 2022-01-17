using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Singleton { get; protected set; }

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

    public bool isTesting;          // Variable to represent whether the application is run for testing. (Only for Client testing)
    public bool isLocal;            // Variable to represent whether the application is hosted on localhost. Hosting Mode + Localhost

}
