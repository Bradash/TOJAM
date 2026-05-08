using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemSprite;
    public Color itemColor = Color.white;
    public bool flipX = false;
    public bool flipY = false;
}
