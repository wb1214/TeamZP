using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Item;
using UnityEngine.EventSystems;
// ItemSlot의 자식 오브젝트. 현재 소지 아이템을 보여줌
public class CurrentItem : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private Image[] icons;
    private ItemType _currType;

    private UIManager uIManager;
    public ItemType currType
    {
        get { return _currType; }
        set
        {
            _currType = value;
            foreach(Image _Icon in icons)
            {
                //Debug.Log(_Icon.name);
                if(_Icon.name == _currType.ToString())
                {
                    if(transform.parent.name == "ItemSlot") uIManager.localPlayer.GetComponent<Student>().pv.RPC("RPCSetActiveItem", PhotonTargets.All, (int)_currType);
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
        uIManager = GameObject.Find("UIManager").GetComponent<UIManager>();
    }



    //마우스 클릭 될 때 발생하는 이벤트 함수
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (_currType != ItemType.None)
            {
                Debug.Log("this item type :" + _currType);


            }

        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currType != ItemType.None)
        {
            DragSlot.instance.dragSlot = this;
            DragSlot.instance.DragSetImage(_currType);
            DragSlot.instance.transform.parent.position = eventData.position;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(_currType != ItemType.None)
        {
            DragSlot.instance.transform.parent.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragSlot.instance.DragSetImage(ItemType.None);
        DragSlot.instance.dragSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("DragEnd");
        if (DragSlot.instance.dragSlot != null && DragSlot.instance.dragSlot.currType != ItemType.None && this.currType != ItemType.None)
        {
            ItemType tempType = currType;
            currType = DragSlot.instance.dragSlot.currType;
            DragSlot.instance.dragSlot.currType = tempType;
        }
    }




}
