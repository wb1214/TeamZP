using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Item;

public class DragSlot : MonoBehaviour,IPointerClickHandler
{
    static public DragSlot instance;
    public CurrentItem dragSlot;

    private Image[] icons;
    private ItemType _currType;
    public ItemType currType
    {
        get { return _currType; }
        set
        {
            _currType = value;
            foreach (Image _Icon in icons)
            {
                //Debug.Log(_Icon.name);
                if (_Icon.name == _currType.ToString())
                {
                    _Icon.color = new Color(1, 1, 1, 1);
                }
                else
                {
                    _Icon.color = new Color(1, 1, 1, 0);
                }
            }
        }
    }



    private void Awake()
    {
        icons = transform.GetComponentsInChildren<Image>();
    }

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        currType = ItemType.None;
    }

    public void DragSetImage(ItemType type)
    {
        currType = type;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("is Cilcked");
    }

   
}
