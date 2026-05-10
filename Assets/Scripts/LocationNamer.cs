using System;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-25)]
public class LocationNamer : MonoBehaviour
{
    public string aisle;
    private int locationNumber = 1;

    private void Awake()
    {
        NameLocations();
    }
    [ContextMenu("NameLocations")]
    private void NameLocations()
    {
        foreach (Transform rack in transform)
        {
            foreach (Transform child in rack)
            {
                if (!child.TryGetComponent(out ItemDisplay display)) continue;
                display.location = $"{aisle}{locationNumber}";
                locationNumber++;
            }
        }
    }

    private void OnValidate()
    {
       if (string.IsNullOrWhiteSpace(aisle))
           aisle = name;
    }
}
