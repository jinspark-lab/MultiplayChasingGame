using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePopup : MonoBehaviour
{
    public GameObject popUpBox;

    public delegate void PopUpCallback();

    private PopUpCallback closeCallback;

    // Start is called before the first frame update
    void Start()
    {
        popUpBox.SetActive(false);
    }

    public void Popup(string text, PopUpCallback callback)
    {
        Debug.Log("[GamePopup] Text=" + text);
        popUpBox.SetActive(true);
        this.closeCallback = callback;
    }

    public void ClosePopup()
    {
        popUpBox.SetActive(false);
        closeCallback();
    }
}
