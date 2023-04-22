using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
public class Door : MonoBehaviour, IItem
{
    private Animator anim;
    public bool isOpened;
    private ItemManager itemManager;
    public int arrayIndex;
    public bool isInside;
    public ItemType Type { get; set; }
    public int Durability { get; set; }
    public float Cooltime { get; set; }

    private IEnumerator openCo;
    public IEnumerator breakCo;


    private void Awake()
    {
        openCo = null;
        Cooltime = 0;
        anim = transform.parent.GetComponentInChildren<Animator>();
        Durability = 3;
        itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
    }

    private void Start()
    {
        Type = ItemType.ImdUseItem;
        isOpened = false;
    }


    public void SetIndex(int num)
    {
        this.arrayIndex = num;
    }

    public void Use(GameObject target)
    {//IItem 인터페이스 의 메서드
        switch (target.gameObject.tag)
        {
            case "Zombie":
                Vector3 myPos = transform.TransformPoint(transform.position);
                Vector3 targetPos = transform.TransformPoint(target.transform.position);
                Debug.Log(myPos);
                Debug.Log(targetPos);
                Vector3 dir = (targetPos - myPos).normalized;
                Debug.Log(dir);
                isInside = dir.z > 0 ? false : true;
                Debug.Log("IS INSIDE " + isInside);
                break;
            default:
                break;

        }

        if (target.gameObject.name != "ItemManager" && target.gameObject.tag == "Student")
        {
            if (openCo != null) return;
            if (openCo == null)
            {
                openCo = this.OpenCoroutine();
                StartCoroutine(openCo);
                itemManager.pv.RPC("UseDoor", PhotonTargets.Others, this.arrayIndex);
            }
        }
        else if (target.gameObject.name != "ItemManager" && target.gameObject.tag == "Zombie")
        {
            if (breakCo != null) return;
            if (breakCo == null)
            {
                breakCo = this.TakeDamageCo(isInside);
                StartCoroutine(breakCo);
                itemManager.pv.RPC("TakeDmgDoor", PhotonTargets.Others, this.arrayIndex, isInside);
            }
               
        }
        else
        {
            OpenDoor();
        }
    }


    void OpenDoor()
    {
        Debug.Log("OpenDoor");
        if (!isOpened)
        {
            itemManager.SoundPlay(transform.position, "DoorOpen");
            anim.SetTrigger("Open");
            isOpened = true;
        }
        else
        {
            itemManager.SoundPlay(transform.position, "DoorClose");
            anim.SetTrigger("Close");
            isOpened = false;
        }

    }

    IEnumerator OpenCoroutine()
    {
        yield return null;
        GetComponent<Collider>().enabled = false;
        Debug.Log("OpenDoor");
        if (!isOpened)
        {
            itemManager.SoundPlay(transform.position, "DoorOpen");
            anim.SetTrigger("Open");
            isOpened = true;
        }
        else
        {
            itemManager.SoundPlay(transform.position, "DoorClose");
            anim.SetTrigger("Close");
            isOpened = false;
        }
        yield return new WaitForSeconds(2.0f);
        GetComponent<Collider>().enabled = true;
        openCo = null;
    }
    public void TakeDamage(bool isInside)
    {
        Debug.Log("TakeDamage");
        Durability -= 1;
        if (Durability == 0)
        {
            string name = isInside == false ? "Broken1" : "Broken2";
            anim.SetTrigger(name);
            Cooltime = 0; GetComponent<Collider>().enabled = false;
            itemManager.SoundPlay(transform.position, "DoorTakeDmg");

            return;
        }
        anim.SetTrigger("TakeDmg");
        itemManager.SoundPlay(transform.position, "DoorTakeDmg");

    }
    IEnumerator TakeDamageCo(bool isInside)
    {
        yield return null;
        Debug.Log("TakeDamage");
        Durability -= 1;
        if (Durability == 0)
        {
            string name = isInside == false ? "Broken1" : "Broken2";
            anim.SetTrigger(name);
            Cooltime = 0; GetComponent<Collider>().enabled = false;
            itemManager.SoundPlay(transform.position, "DoorTakeDmg");

            breakCo = null;
            yield break;
        }
        anim.SetTrigger("TakeDmg");
        itemManager.SoundPlay(transform.position, "DoorTakeDmg");
        yield return new WaitForSeconds(3.0f);
        breakCo = null;
    }
}