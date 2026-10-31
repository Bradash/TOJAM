using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    [Header("Swapping")]
    [Tooltip("Items allowed to replace this store item. Random replacements must also belong to the scene's Swapped Items set.")]
    public List<ItemData> swappableItems = new List<ItemData>();
    [Header("Prefab")]
    public GameObject itemPrefab;
    public Vector3 itemOffset;
    public Vector3 itemScale =  Vector3.one;
    public bool editScale = false;
    [Header("Sprite")]
    public Sprite itemSprite;
    public Color itemSpriteColor = Color.white;
}
