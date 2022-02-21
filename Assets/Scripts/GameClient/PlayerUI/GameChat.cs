using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameChat : MonoBehaviour
{
    [SerializeField]
    public GameObject textPrefab;
    [SerializeField]
    public Transform parentContent;
    [SerializeField]
    public Scrollbar scrollbar;
    [SerializeField]
    public TMP_InputField inputField;

    // Start is called before the first frame update
    void Start()
    {
        inputField.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        InputManager.Singleton.SetFocusLock(inputField.isFocused);
        if (InputManager.Singleton.GetInputKeyDown(KeyCode.Return) && inputField.isFocused == false)
        {
            inputField.ActivateInputField();
        }
        else if (InputManager.Singleton.GetInputKeyDown(KeyCode.Escape) && inputField.isFocused == true)
        {
            inputField.DeactivateInputField();
        }
    }

    // Hit the button Enter, call this method to update
    public void OnEndEditEventMethod()
    {
        if (InputManager.Singleton.GetInputKeyDown(KeyCode.Return))
        {
            UpdateChat();
        }
    }

    // Update the input text on the form
    public void UpdateChat()
    {
        if (inputField.text.Equals("")) return;

        GameObject cloneText = Instantiate(textPrefab, parentContent);
        cloneText.GetComponent<TextMeshProUGUI>().text = inputField.text;
        inputField.text = "";
        scrollbar.value = 0;
    }

}
