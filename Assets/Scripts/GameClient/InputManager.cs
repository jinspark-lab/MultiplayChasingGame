using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Singleton { get; protected set; }

    public bool isFocused;

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
        isFocused = false;
    }

    public bool GetControlKey(KeyCode code)
    {
        return !isFocused && Input.GetKey(code);
    }

    public bool GetInputKeyDown(KeyCode code)
    {
        return Input.GetKeyDown(code);
    }

    public void SetFocusLock(bool focus)
    {
        this.isFocused = focus;
    }

}
