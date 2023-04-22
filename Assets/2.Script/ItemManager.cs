using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;

public class ItemManager : Photon.MonoBehaviour
{
    public PhotonView pv;
    public GameObject itemStorage;
    public ParticleSystem shineParticle;

    public ItemType[] itemSets;
    private Queue<ItemType> itemQueue;
    public GameObject[] rootContainer;
    public GameObject[] containers;
    public List<GameObject> itemStorages;
    GameObject fixPos;

    public GameObject[] rootdoors;
    public GameObject FDoor;
    public GameObject BDoor;
    public List<GameObject> OpenableDoors;


    private GameObject itemSpawnPoints;
    private GameObject dropItemPoints;
    private csSoundManager soundManager;
    int DropStorageNum = 999;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        pv.ObservedComponents[0] = this;
        pv.synchronization = ViewSynchronization.UnreliableOnChange;
        itemSpawnPoints = GameObject.Find("ItemSpawnPoints");
        dropItemPoints = GameObject.Find("DropItemPoints");
        soundManager = GameObject.FindGameObjectWithTag("SoundManager").GetComponent<csSoundManager>();
        fixPos = new GameObject("FixPos");

        OpenableDoors = new List<GameObject>();
        itemStorages = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        SetItemTypeQueue();
        CreateItemStorage();
        CreateDoors();
    }

    //퍼블릭 설정한 List의 ItemType값 큐에 넣는 메서드
    void SetItemTypeQueue()
    {//처음 무기 제한된 갯수 생성, 후에 백신 주기적 생성.
        itemQueue = new Queue<ItemType>();
        for(int i = 0; i < itemSets.Length; i++)
        {
            itemQueue.Enqueue(itemSets[i]);
        }
    }

    void CreateDoors()
    {
        for (int i = 0; i < rootdoors.Length; i++)
        {
            GameObject door = rootdoors[i].gameObject.name == "F_Door" ? FDoor : BDoor;
          
            GameObject openableDoor = Instantiate(door, rootdoors[i].transform.position, rootdoors[i].transform.localRotation, rootdoors[i].transform.parent) as GameObject;

            Debug.Log("openableDoor != null"+openableDoor != null);
            Destroy(rootdoors[i]);

            if (openableDoor != null)
            {
                openableDoor.GetComponentInChildren<Door>().SetIndex(i);
                OpenableDoors.Add(openableDoor);
            }
        }
    }

    [PunRPC]
    public void UseDoor(int index)
    {
        OpenableDoors[index].GetComponentInChildren<IItem>().Use(this.gameObject);
    }

    [PunRPC]
    public void TakeDmgDoor(int index, bool isInside)
    {
        OpenableDoors[index].GetComponentInChildren<Door>().TakeDamage(isInside);
    }

    [PunRPC]
    public void SoundPlay(Vector3 pos , string name)
    {
        soundManager.PlayEffect(pos, name);
    }


    // 맵에 아이템 저장소 생성 메서드.
    void CreateItemStorage()
    {
        //자연스럽게 컨테이너를 넣기 위해서 컨테이너 오브젝트와 같이 생성. 프리펍으로 넣은 맵 안의 컨테이너는 제거.
        for(int i = 0; i< rootContainer.Length; i++)
        {
            GameObject oneStorage = Instantiate(itemStorage, rootContainer[i].transform.position, rootContainer[i].transform.localRotation, itemSpawnPoints.transform);
            GameObject _fixPos = Instantiate(fixPos, rootContainer[i].transform.position + rootContainer[i].transform.forward, rootContainer[i].transform.localRotation, oneStorage.transform) as GameObject;
            oneStorage.GetComponent<ItemStorage>().SetFixPos(_fixPos);
            ItemType nextItem = ItemType.Vaccine;
            if ( itemQueue.Count != 0)
            {
                nextItem = itemQueue.Dequeue();
            }

            GameObject container = Instantiate(GetContainer(rootContainer[i].name), rootContainer[i].transform.position, rootContainer[i].transform.localRotation, oneStorage.transform);

            oneStorage.GetComponent<ItemStorage>().SetType(nextItem);
            oneStorage.GetComponent<ItemStorage>().SetContainer(container, i);
            itemStorages.Add(oneStorage);
            Destroy(rootContainer[i]);
        }


        //떨어진 아이템 저장소도 생성하여 트리거 비활성화함.
        for (int j = 0; j < 3; j++)
        {
            GameObject oneDropStorage = Instantiate(itemStorage, transform.position, Quaternion.identity, dropItemPoints.transform);

            Instantiate(shineParticle, transform.position, Quaternion.identity, oneDropStorage.transform);

            GameObject nullContainer = null;
            oneDropStorage.GetComponent<ItemStorage>().SetContainer(nullContainer, DropStorageNum--);
            oneDropStorage.GetComponent<ItemStorage>().SetType(ItemType.None);
            oneDropStorage.GetComponent<BoxCollider>().size = new Vector3(2, 1, 2);
            oneDropStorage.GetComponent<BoxCollider>().center = new Vector3(0, 0, 0);
            oneDropStorage.GetComponent<BoxCollider>().enabled = false;

            // oneDropStorage.SetActive(false);
        }
    }

    [PunRPC]
    public void ShowItem(int index, bool isActive)
    {
        if(index < 900)
        {
            itemStorages[index].GetComponent<ItemStorage>().SetItemActive(isActive, this.gameObject);
        }
        else
        {
            for (int i = 0; i < dropItemPoints.transform.childCount; i++)
            {
                if (dropItemPoints.transform.GetChild(i).GetComponent<ItemStorage>().index == index)
                {
                    Transform dropItem = dropItemPoints.transform.GetChild(i);
                    dropItem.GetComponent<ItemStorage>().SetItemActive( false, this.gameObject);
                    break;
                }
            }
        }
       
    }

    //[PunRPC]
    //void SetStorageParent(int oneStorage,int rootIndex, int type)
    //{
    //    Debug.Log("SetStorageParent");
    //    GameObject storage = PhotonView.Find(oneStorage).gameObject;
    //    //GameObject container = PhotonView.Find(oneContainer).gameObject;

    //    GameObject container =  Instantiate(GetContainer(rootContainer[rootIndex].name), rootContainer[rootIndex].transform.position, rootContainer[rootIndex].transform.localRotation, storage.transform);

    //    storage.GetComponent<ItemStorage>().SetType((ItemType)type);
    //    storage.transform.parent = itemSpawnPoints.transform;
    //    storage.GetComponent<ItemStorage>().SetContainer(container);
    //    Destroy(rootContainer[rootIndex]);
    //}

    //[PunRPC]
    //void SetDStorageParent(int oneDStorage)
    //{
    //    GameObject oneDropStorage = PhotonView.Find(oneDStorage).gameObject;
    //    ParticleSystem oneParticle = Instantiate(shineParticle, transform.position, Quaternion.identity);

    //    oneDropStorage.transform.parent = dropItemPoints.transform;
    //    oneParticle.transform.parent = oneDropStorage.transform;
    //    GameObject nullContainer = null;
    //    oneDropStorage.GetComponent<ItemStorage>().SetContainer(nullContainer,999);
    //    oneDropStorage.GetComponent<ItemStorage>().SetType(ItemType.None);
    //    oneDropStorage.GetComponent<BoxCollider>().size = new Vector3(2, 1, 2);
    //    oneDropStorage.GetComponent<BoxCollider>().enabled = false;

    //}

    //[PunRPC]
    //void SetDoorParent(int oneDoor, int rootDoor)
    //{
    //    GameObject door = PhotonView.Find(oneDoor).gameObject;
    //    door.transform.parent = rootdoors[rootDoor].transform.parent;
    //    Destroy(rootdoors[rootDoor]);
    //}

    GameObject GetContainer(string name)
    {
        GameObject container = null;
        for (int i = 0; i < containers.Length; i++)
        {
            if (containers[i].name == name)
            {
                container = containers[i].gameObject;
                break;
            }
        }
        return container;
    }

    //떨어진 아이템

    [PunRPC]
    public void DropItemStorage(Vector3 pos, int type, int durability)
    {
        Debug.Log("ITEM DROPPTED");
        Transform dropItem = null; 
        for (int i = 0; i < dropItemPoints.transform.childCount; i++)
        {
            if (dropItemPoints.transform.GetChild(i) != null)
            {
                dropItem = dropItemPoints.transform.GetChild(i);
                dropItem.position = pos;
                dropItem.GetComponent<ItemStorage>().SetType((ItemType)type, durability);
                dropItem.GetComponent<ItemStorage>().SetItemActive( true, this.gameObject);
                dropItem.GetComponent<ItemStorage>().shineParticle.Play();
                dropItem.GetComponent<ItemStorage>().isTaken = false;
                dropItem.GetComponent<BoxCollider>().enabled = true;
                break;
            }
        }

        if(dropItem == null)
        {
            //3개 이상 바닥에 떨어질 시 로직 구현 필요
        }


        
    }
    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
