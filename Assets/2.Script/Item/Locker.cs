using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
public class Locker : MonoBehaviour, IItem
{
    private Animator anim;
    private bool isOpen;
    public ItemType Type { get; set; }
    public int Durability { get; set; }
    public float Cooltime { get; set; }

    private ItemManager itemManager;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
    }

    private void Start()
    {
        Type = ItemType.ImdUseItem;
        isOpen = false;
    }

    public void Use(GameObject target)
    {//IItem 인터페이스 의 메서드
        if (!isOpen)
        {
            Debug.Log("isOpen");
            itemManager.SoundPlay(transform.position, "LockerOpen");
            anim.SetTrigger("Open");
            isOpen = true;
        }
    }
}
