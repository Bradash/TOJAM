using UnityEngine;

public class ItemInteraction : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 pos;
    
    public LayerMask layerMask;
    public float interactionDistance;
    public QueryTriggerInteraction queryTriggerInteraction;
    
    public ItemInventory  itemInventory;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        mainCamera = Camera.main;
        pos = new Vector3(0.5f, 0.5f, 0);
    }

    // Update is called once per frame
    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.mouseScrollDelta.y != 0)
        {
            if (Input.mouseScrollDelta.y > 0)
                itemInventory.SelectNextSlot();
            else
                itemInventory.SelectPrevSlot();
        }
        if (Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ViewportPointToRay(pos);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, interactionDistance, layerMask, queryTriggerInteraction)) return;
            ItemDisplay itemDisplay;
            if (hit.collider.TryGetComponent<ItemDisplay>(out itemDisplay))
            {
                ReplaceItemInInventory(itemDisplay);
                return;
            }

            if (!hit.collider.TryGetComponent<Item>(out Item item)) return;
            if (item.TryGetItemDisplay(out itemDisplay))
            {
                ReplaceItemInInventory(itemDisplay);
                return;
            }

            itemInventory.SetItem(item);
        }
    }

    private void ReplaceItemInInventory(ItemDisplay itemDisplay)
    {
        (Item item, int slot) = itemInventory.GetSelectedItem();
        if (!itemDisplay.TryReplaceItem(item, out Item takeItem)) return;
        itemInventory.TryRemoveItemInSlot(slot);
        if (takeItem)
        {
            itemInventory.SetItemInSelectedSlot(takeItem);
        }
    }
}