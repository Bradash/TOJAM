using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemSet", menuName = "Scriptable Objects/ItemSet")]
public class ItemSet : ScriptableObject
{
    public List<ItemData> items = new List<ItemData>();
}
