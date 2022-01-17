using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    SpeedUp
}

public class ItemSpawner : MonoBehaviour
{
    [SerializeField]
    private List<ItemData> itemDataList;

    [SerializeField]
    private GameObject itemPrefab;

    // Start is called before the first frame update
    void Start()
    {
        //for (int i=0; i<itemDataList.Count; i++)
        //{
        //    SpawnItem((ItemType)i);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ItemObject SpawnItem(ItemType itemType)
    {
        int x = UnityEngine.Random.Range(-30, 30);
        int z = UnityEngine.Random.Range(-30, 30);
        Vector3 spawnPosition = new Vector3(x, 2, z);

        LogManager.Singleton.WriteLog("Spawning - " + spawnPosition);
        var newItem = Instantiate(itemPrefab, spawnPosition, Quaternion.identity).GetComponent<ItemObject>();
        newItem.ItemData = itemDataList[(int)itemType];
        return newItem;
    }
}
