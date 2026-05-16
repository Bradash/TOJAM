using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-25)]
public class LocationNamer : MonoBehaviour
{
    public string aisle;
    private int locationNumber = 1;
    public bool oldStyle = false;
    
    private void Awake()
    {
        //NameLocations();
        //oldStyle = GameData.difficulty.disOrganizedAisles;
        locationNumber = 1;
        switch (GameData.difficulty.lableType)
        {
            case LableType.RandomEasy:
                NameLocationsRandomEasy();
                break;
            case LableType.RandomHard:
                NameLocationsRandomHard();
                break;
            case LableType.OldStyle:
                NameLocationsOldStyle();
                break;
            case LableType.Organized:
            default:
                NameLocations();
                break;
        }
        
    }
    [ContextMenu("NameLocations")]
    private void NameLocations()
    {
        foreach (Transform rack in transform)
        {

            ItemDisplay[] items = rack.GetComponentsInChildren<ItemDisplay>();
            if (items.Length == 0) continue;
            List<ItemDisplay> displays = SortWithThreshold(items);
            foreach (ItemDisplay display in displays)
            {
                display.location = $"{aisle}{locationNumber}";
                locationNumber++;
                display.UpdatLocationText();
            }
        }
    }
    private void NameLocationsOldStyle()
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
    private void NameLocationsRandomEasy()
    {
        foreach (Transform rack in transform)
        {
            ItemDisplay[] items = rack.GetComponentsInChildren<ItemDisplay>();
            if (items.Length == 0) continue;
            
            List<ItemDisplay> displays = items.ToList();
            displays.Shuffle();
            foreach (ItemDisplay display in displays)
            {
                display.location = $"{aisle}{locationNumber}";
                locationNumber++;
                display.UpdatLocationText();
            }
        }
    }
    private void NameLocationsRandomHard()
    {

            ItemDisplay[] items = transform.GetComponentsInChildren<ItemDisplay>();
            if (items.Length == 0) return;
            
            List<ItemDisplay> displays = items.ToList();
            displays.Shuffle();
            foreach (ItemDisplay display in displays)
            {
                display.location = $"{aisle}{locationNumber}";
                locationNumber++;
                display.UpdatLocationText();
            }
    }
    public List<ItemDisplay> SortWithThreshold(ItemDisplay[] items, float shelfThreshold = 0.2f)
    {
        return items
            .OrderByDescending(i => Mathf.Round(i.transform.position.y / shelfThreshold))
            .ThenByDescending(i => i.transform.position.x)
            .ToList();
    }
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(aisle))
            aisle = name;
    }

    private void Reset()
    {
        OnValidate();
    }
}