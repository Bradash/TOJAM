using System;
using UnityEngine;
[RequireComponent(typeof(SpriteRenderer))]
public class ItemDisplay : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    public Item item;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        Display();
    }

    private void Display()
    {
        if (!_spriteRenderer)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        if (item == null)
        {
            _spriteRenderer.sprite = null;
            return;
        }

        _spriteRenderer.sprite = item.itemSprite;
        _spriteRenderer.flipX = item.flipX;
        _spriteRenderer.flipY = item.flipY;
        _spriteRenderer.color = item.itemColor;
    }


    public Item TakeItem()
    {
        if (item == null) return null;
        Item getItem = item;
        item = null;
        Display();
        return getItem;
    }

    public Item ReplaceItem(Item newItem)
    {
        Item oldItem = item;
        item = newItem;
        Display();
        return oldItem;
    }

    private void OnValidate()
    {
        Display();
    }
}
