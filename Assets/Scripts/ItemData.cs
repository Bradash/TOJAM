using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    [Header("Prefab")]
    public GameObject itemPrefab;
    public Vector3 itemOffset;
    public Vector3 itemScale =  Vector3.one;
    public bool relativeScale = true;
    [Header("Sprite")]
    public Sprite itemSprite;
    public Color itemSpriteColor = Color.white;
}
