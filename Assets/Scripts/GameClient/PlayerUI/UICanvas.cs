using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICanvas : MonoBehaviour
{
    public GameObject resultPopUp;
    public TMP_Text roundText;

    public void UpdatePlayerInfoText(string text)
    {
        roundText.text = text;
    }

    public void OpenResultPopUp(string text, GamePopup.PopUpCallback callback)
    {
        Debug.Log("Open Result Pop Up = " + text);
        resultPopUp.GetComponent<GamePopup>().Popup(text, callback);
    }

    public void OnClickHelpButton()
    {

    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }

}
