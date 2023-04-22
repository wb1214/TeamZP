using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
public class ItemStorage : MonoBehaviour, IItemStorage
{
    [field: SerializeField]
    public ItemType Type { get; set; }
    public GameObject fixPos;
    public int durability;

    public Transform[] itemSamples;
    public ParticleSystem shineParticle;
    public IItem container;
    public int index;

    private ItemManager itemManager;

    public bool isTaken; // 현재 아이템을 플레이어가 획득시 true 반환 변수(확인용)
    //public bool itemActivated; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Student" && !isTaken && shineParticle.isStopped && !(transform.root.name == "DropItemPoints"))
        {
            shineParticle.Play(); //로컬만 실행. 
        }
    }

    private void OnTriggerStay(Collider other)
    { //트리거에 들어온 학생 오브젝트
        if (other.gameObject.tag == "Student" && isTaken)
        {
            //범위내의 다른 Student 오브젝트에  RCP 호출 구현필요.
            // other.gameObject.GetComponent<Student>().IsInteractable(false);


            // 네트워크 호출
            //if(transform.root.name == "DropItemPoints")
            //{
            //    Debug.Log("dropped item");
            //    gameObject.SetActive(false);
            //}

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Student" && !isTaken && shineParticle.isPlaying && !(transform.root.name == "DropItemPoints"))
        {
            shineParticle.Stop(); //로컬만 실행. 
        }
    }

    private void Awake()
    {
        container = null;
        itemManager = GameObject.Find("ItemManager").GetComponent<ItemManager>();
        itemSamples = new Transform[transform.childCount];
        fixPos = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            itemSamples[i] = transform.GetChild(i);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < itemSamples.Length; i++) itemSamples[i].gameObject.SetActive(false);

    }

    //Item Manager 에서 해당 메서드를 호출하여 Pos마다 type 배정함.
    public void SetType(ItemType type, int _durability = 0)
    {
        this.Type = type;
        if (type == ItemType.None)
        {
            GetComponent<Collider>().enabled = false;
            shineParticle.Stop();
            isTaken = false;
        }
        this.durability = _durability;
    }

    public bool SetItemActive(bool isActive, GameObject target)
    {

        if (target.gameObject.name != "ItemManager") itemManager.pv.RPC("ShowItem", PhotonTargets.Others, this.index, isActive);
        if (!isActive) SetType(ItemType.None);
        bool itemActivated = false; // 현재 스폰된 아이템의 SetActive확인용 변수

        for (int i = 0; i < itemSamples.Length; i++)
        {
            if (isActive && itemSamples[i].gameObject.name == this.Type.ToString())
            {
                Debug.Log("itemActivated");
                itemActivated = itemSamples[i].gameObject.activeSelf;
                itemSamples[i].gameObject.SetActive(true);
            }
            else
            {
                itemSamples[i].gameObject.SetActive(false);
            }
        }
        if (container != null)
        {
            container.Use(this.gameObject);
        }

        Debug.Log("itemActivated : " + itemActivated);
        return itemActivated;
    }

    public void SetContainer(GameObject _container, int _index)
    {
        this.index = _index;
        if (_container == null)
        {
            shineParticle = transform.GetComponentInChildren<ParticleSystem>();
            shineParticle.Stop();
        }
        else
        {
            container = _container.GetComponent<IItem>();
            shineParticle = _container.transform.Find("ShineParticle").GetComponent<ParticleSystem>();
            shineParticle.Stop();
        }
    }

    public void SetFixPos(GameObject obj)
    {
        fixPos = obj;
    }

    private void Update()
    {
        if (isTaken /*&& !(transform.root.name == "DropItemPoints")*/ && this.Type != ItemType.None)
        {
            SetItemActive(false, this.gameObject);
        }
    }

    void UseContainer()
    {
        container.Use(this.gameObject);
    }

}
