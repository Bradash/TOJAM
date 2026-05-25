using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class ItemInteraction : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 pos;
    
    public LayerMask layerMask;
    public float interactionDistance;
    public QueryTriggerInteraction queryTriggerInteraction;
    
    public ItemInventory  itemInventory;
    public static event Action OnStoreSwapCompleted;
    public PromptDisplay promptDisplay;
    [SerializeField] private Scouter scouter;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        mainCamera = Camera.main;
        pos = new Vector3(0.5f, 0.5f, 0);
    }
#if False
    // Update is called once per frame
    private void Update()
    {
        if (Time.timeScale == 0f) return;
        
        if (Input.mouseScrollDelta.y != 0)
        {
            SelectSlot(Input.mouseScrollDelta.y > 0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectSlot(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSlot(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectSlot(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectSlot(3);
        }

        if (!Input.GetButtonDown("Fire1")) return;
        Interact();
    }
#endif

    private void ReplaceItemInInventory(ItemDisplay itemDisplay)
    {
        (Item item, int slot) = itemInventory.GetSelectedItem();
        if (item && item.storeItem && !itemDisplay.storeDisplay)
        {
            promptDisplay.ShowPrompt();
            return;
        }
        if (!itemDisplay.TryReplaceItem(item, out Item takeItem)) return;
        itemInventory.TryRemoveItemInSlot(slot);
        if (takeItem)
        {
            itemInventory.SetItemInSelectedSlot(takeItem);
        }
    }

    public void Interact()
    {
        if (Time.timeScale == 0f) return;
        Ray ray = mainCamera.ViewportPointToRay(pos);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, layerMask, queryTriggerInteraction)) return;

        if (hit.collider.CompareTag("Car"))
        {
            if (itemInventory.TryRemoveSelectedStoreItem())
            {
                OnStoreSwapCompleted?.Invoke();
            }
            return;
        }

        if (hit.collider.CompareTag("Scouter"))
        {
            if (!scouter)
            {
                scouter = FindFirstObjectByType<Scouter>();
            }
            if (scouter)
            {
                scouter.scouterOnline = true;
                hit.collider.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Interacted with a Scouter item, but the 'scouter' reference is missing on {gameObject.name}!", this);
            }
            return;
        }

        if (hit.collider.TryGetComponent<ItemDisplay>(out ItemDisplay itemDisplay))
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
    public void SelectSlot(bool next)
    {
        if (Time.timeScale == 0f) return;
        if (next)
            itemInventory.SelectNextSlot();
        else
            itemInventory.SelectPrevSlot();
    }
    public void SelectSlot(int slot)
    { 
        if (Time.timeScale == 0f) return;
       itemInventory.SelectSlot(slot);
    }
}