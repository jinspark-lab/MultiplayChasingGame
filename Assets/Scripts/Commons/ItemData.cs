using UnityEngine;

public enum ItemEffect
{
    SpeedUp
}

[CreateAssetMenu(fileName = "ItemData", menuName ="Scriptable Object/Item Data", order = int.MaxValue)]
public class ItemData : ScriptableObject
{
    [SerializeField]
    private string id;
    public string Id { get { return id; } }
    [SerializeField]
    private ItemEffect itemEffect;
    public ItemEffect Effect {  get { return itemEffect; } }
}
