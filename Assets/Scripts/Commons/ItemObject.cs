using UnityEngine;

public class ItemObject : MonoBehaviour
{
    [SerializeField]
    private ItemData itemData;
    public ItemData ItemData { set { itemData = value; } }

}