using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    public CurrentItem[] currentItems;

    private ItemSlot itemSlot;
    public int belongings;
    private void Awake()
    {
        currentItems = transform.GetComponentsInChildren<CurrentItem>();
        itemSlot = transform.parent.GetComponentInChildren<ItemSlot>();
    }

    // Start is called before the first frame update
    void Start()
    {
        FreshSlot();
    }

    void FreshSlot()
    {
        belongings = 0;
        for(int i = 0; i< currentItems.Length; i++)
        {
            currentItems[i].currType = ItemType.None;
        }
    }

    public ItemType AddItem(ItemType type)
    { //인벤토리에 아이템을 추가하고 1번 슬롯 아이템 타입을 반환.
        Debug.Log("Add Inventroy");
        for(int i = currentItems.Length -1; i >-1; i--)
        {
            if(currentItems[i].currType == ItemType.None && type != ItemType.None)
            {
                currentItems[i].currType = type;
                belongings++;
                break;
            }
        }
        return currentItems[3].currType;
    }

    public ItemType UseItem()
    {
        currentItems[3].currType = ItemType.None;

        for(int i = currentItems.Length - 1; i > -1; i--)
        {
            if (i == 0) { currentItems[i].currType = ItemType.None;
                break;
            }
            ItemType temp = currentItems[i - 1].currType;
            currentItems[i].currType = temp;
            
        }

        return currentItems[3].currType;
    }

    public bool IsInventoryFull()
    {
        return belongings >= currentItems.Length;
    }

    public bool isHavingSame(ItemType type)
    {
        bool isHaving = false;
        for (int i = currentItems.Length - 1; i >-1; i--)
        {
            Debug.Log(i);
            if (currentItems[i].currType != ItemType.None && currentItems[i].currType == type)
            {
                isHaving = true;
                break;
            }
        }
        return isHaving;
    }

}
