using UnityEngine;

public class Scouter : MonoBehaviour
{
    [SerializeField] private GameObject pointerObject;
    
    private Transform pointerTransform;
    public bool scouterOnline;
    private Transform PointerTransform
    {
        get
        {
            if (pointerTransform == null && pointerObject)
            {
                pointerTransform = pointerObject.transform;
            }
            return pointerTransform;
        }
        set => pointerTransform = value;
    }
    
    public void SetLocation(ItemDisplay itemDisplay)
    {
        // 1. Guard clause in case itemDisplay or the pointer doesn't exist
        if (!scouterOnline || itemDisplay == null || PointerTransform == null)
        {
            pointerObject.SetActive(false);
            return;
        }
        pointerObject.SetActive(true);
        // 2. Grab the target item's position
        Vector3 displayLocation = itemDisplay.transform.position;
        
        // 3. Keep the pointer's original height (Y), but match X and Z
        displayLocation.y = PointerTransform.position.y;
        pointerTransform.position = displayLocation;
    }
}