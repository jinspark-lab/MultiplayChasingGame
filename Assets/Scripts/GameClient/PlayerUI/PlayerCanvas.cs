using UnityEngine;
using UnityEngine.UI;

/***
 * 
 * This canvas should be rendered by main camera, when it is instantiated.
 * 
 */
[RequireComponent(typeof(Canvas))]
public class PlayerCanvas : MonoBehaviour
{
    private Canvas canvas;
    public Text playerLabel;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        playerLabel.text = gameObject.transform.parent.gameObject.GetComponent<PlayerController>().playerName + "(" + gameObject.transform.parent.gameObject.GetComponent<PlayerController>().playerScore + ")";
    }
}
