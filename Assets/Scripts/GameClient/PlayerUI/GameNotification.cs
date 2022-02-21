using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameNotification : MonoBehaviour
{
    [SerializeField]
    public GameObject textPrefab;
    [SerializeField]
    public Transform parentContent;
    [SerializeField]
    public Scrollbar scrollbar;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateNotificationText(List<string> textList)
    {
        string notification = "";
        foreach (string line in textList)
        {
            notification += line + "\r\n";
        }

        GameObject cloneText = Instantiate(textPrefab, parentContent);
        cloneText.GetComponent<TextMeshProUGUI>().text = notification;
        scrollbar.value = 0;
    }
}
