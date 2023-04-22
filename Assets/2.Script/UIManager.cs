using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Item;
using System;

public class UIManager : MonoBehaviour
{
    private Inventory inventory;
    private ItemSlot itemSolt;
    private MovePanel inventoryPanel;

    public List<GameObject> bigMap;
    public GameObject mainUi;

    public Text time;
    private double startTime;
    private bool gameStarted;
    private bool gameOver;

    public List<GameObject> stateList;

    private Dictionary<int, IEnumerator> transitionMap;
    private Image coolTimeImg;
    public GameObject localPlayer;
    bool modeUI;

    private void Awake()
    {
        transitionMap = new Dictionary<int, IEnumerator>();
        bigMap = new List<GameObject>();
        modeUI = GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>().isHuman;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameOver = false;
        if (modeUI)
        {
            inventory = transform.GetComponentInChildren<Inventory>();
            itemSolt = transform.GetComponentInChildren<ItemSlot>();
            inventoryPanel = transform.GetComponentInChildren<MovePanel>();
        }
        if (stateList.Count == 0)
        {
            GameObject statePanel = transform.Find("Room").Find("StatePanel").gameObject;
            for (int i = 0; i < statePanel.transform.childCount; i++)
            {
                stateList.Add(statePanel.transform.GetChild(i).gameObject);
                statePanel.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        time = transform.Find("Room").transform.Find("Time").GetComponent<Text>();
        time.color = Color.white;
    }

    public void SetMainUi(GameObject ui)
    {
        mainUi = ui;

        mainUi.transform.Find("MiniMap").gameObject.SetActive(true);
        mainUi.transform.Find("BigMap").gameObject.SetActive(false);
    }

    public void SetOhersStateName(bool isZombie, string name, int viewID)
    {
        foreach (GameObject stateObj in stateList)
        {
            if (!stateObj.gameObject.activeSelf)
            {
                Debug.Log("caller name : " + name + ", iszombie : " + isZombie);
                stateObj.gameObject.SetActive(true);
                stateObj.gameObject.name = viewID.ToString();
                string OffState = isZombie != true ? "ZombieState" : "HumanState";
                string OnState = isZombie != true ? "HumanState" : "ZombieState";
                stateObj.gameObject.transform.Find(OffState).gameObject.SetActive(false);
                stateObj.gameObject.transform.Find(OnState).gameObject.SetActive(true);
                stateObj.gameObject.GetComponentInChildren<Text>().text = name;
                return;
            }
        }
    }
    public void ChangeOthersState(int viewID)
    {
        if (transitionMap.ContainsKey(viewID))
        {
            IEnumerator transition = transitionMap[viewID];
            StopCoroutine(transition);
            transition = null;
            transitionMap.Remove(viewID);
            foreach (GameObject stateObj in stateList)
            {
                if (stateObj.gameObject.name == viewID.ToString())
                {
                    stateObj.transform.Find("TransitionImg").GetComponent<Image>().fillAmount = 0;
                }
            }
        }
        foreach (GameObject stateObj in stateList)
        {
            if (stateObj.gameObject.name == viewID.ToString())
            {
                Debug.Log("Change State caller viewID : " + stateObj.gameObject.GetComponentInChildren<Text>().text + "is now Zombie!!");
                string OffState = "HumanState";
                string OnState = "ZombieState";
                stateObj.gameObject.transform.Find(OffState).gameObject.SetActive(false);
                stateObj.gameObject.transform.Find(OnState).gameObject.SetActive(true);
                return;
            }
        }
    }

    public void StartTransitionImg(int viewID, bool isActive)
    {
        if (isActive)
        {
            Debug.Log("transition start");
            if (transitionMap.ContainsKey(viewID)) { Debug.Log("Already transition is started!!"); return; }
            foreach (GameObject stateObj in stateList)
            {
                if (stateObj.gameObject.name == viewID.ToString())
                {
                    Debug.Log("Change State caller viewID : " + stateObj.gameObject.GetComponentInChildren<Text>().text + "is transitioned to Zombie!!");

                    Image transitionImg = stateObj.transform.Find("TransitionImg").GetComponent<Image>();
                    IEnumerator transition = this.TransitionImgFill(transitionImg, viewID);
                    StartCoroutine(transition);
                    transitionMap.Add(viewID, transition);
                }
            }
        }
        else
        {
            Debug.Log("need to Find transition and stop!");
            if (transitionMap.ContainsKey(viewID))
            {
                Debug.Log("transition iEnumerator Found!");
                IEnumerator transition = transitionMap[viewID];
                StopCoroutine(transition);
                transition = null;
                transitionMap.Remove(viewID);
                foreach (GameObject stateObj in stateList)
                {
                    if (stateObj.gameObject.name == viewID.ToString())
                    {
                        stateObj.transform.Find("TransitionImg").GetComponent<Image>().fillAmount = 0;
                    }
                }
            }
        }
    }

    IEnumerator TransitionImgFill(Image _image, int viewID)
    {
        float cool = 13.0f;
        float leftTime = 0;
        while (cool > leftTime)
        {
            leftTime += Time.deltaTime;
            _image.fillAmount = /*1.0f -*/ (leftTime / cool);
            yield return new WaitForFixedUpdate();
        }
        ChangeOthersState(viewID);
    }


    public ItemType SetItemSlot(ItemType type)
    {
        ItemType fristSlotType = ItemType.None;
        if (type == ItemType.None)
        {
            fristSlotType = inventory.UseItem();
        }
        else
        {
            fristSlotType = inventory.AddItem(type);
        }

        return fristSlotType;
    }

    public bool CheckHavingSame(ItemType type)
    {
        return inventory.isHavingSame(type);

    }

    public void ShowBigMap(bool isActive, bool modeUI)
    {
        mainUi.transform.Find("MiniMap").gameObject.SetActive(!isActive);
        mainUi.transform.Find("BigMap").gameObject.SetActive(isActive);

    }

    public void ShowIntrText(bool isActive, string tag)
    {
        if (mainUi != null)
        {
            switch (tag)
            {
                case "ItemStorage":
                    mainUi.transform.Find("InteractionText").gameObject.SetActive(isActive);
                    mainUi.transform.Find("InteractionText").gameObject.GetComponentInChildren<Text>().text = "조사 하기";
                    break;
                case "ImdUseItem":
                    mainUi.transform.Find("InteractionText").gameObject.SetActive(isActive);
                    mainUi.transform.Find("InteractionText").gameObject.GetComponentInChildren<Text>().text = "사용 하기";
                    break;
                case "Zombie":
                    mainUi.transform.Find("InteractionText").gameObject.SetActive(isActive);
                    mainUi.transform.Find("InteractionText").gameObject.GetComponentInChildren<Text>().text = "부수기";
                    break;
                default:
                    mainUi.transform.Find("InteractionText").gameObject.SetActive(isActive);
                    mainUi.transform.Find("InteractionText").gameObject.GetComponentInChildren<Text>().text = "";
                    break;
            }

        }
    }

    public void ShowCoolTime(float amount, float cool)
    {
        if (coolTimeImg == null) coolTimeImg = itemSolt.transform.Find("CoolTimeImg").GetComponent<Image>();
        coolTimeImg.fillAmount = 1.0f - (amount / cool);
    }



    public void SetPlayer(GameObject target)
    {
        this.localPlayer = target;
    }

    public void PlayerSoundOff()
    {
        Debug.Log("sound off");
        PhotonNetwork.Destroy(localPlayer);
    }

    public void ShowInventory()
    {
        if (inventory)
            inventoryPanel.MoveToPos();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTime();
        //Debug.Log(transitionMap.Count);
    }

    public void SetTime(double _startTime, bool _gameStarted)
    {
        startTime = _startTime;
        gameStarted = _gameStarted;
    }

    void UpdateTime()
    {   if (gameOver) return;
        if (!gameStarted)
        {

            startTime = double.Parse(PhotonNetwork.room.CustomProperties["StartTime"].ToString());
            if (startTime != 0)
            {
                gameStarted = true;
            }
            else
            {
                return;
            }
        }
        double incTimer = 0;
        double decTimer = 0;
        // Example for a increasing timer
        incTimer = PhotonNetwork.time - startTime;
        // Example for a decreasing timer
        double roundTime = 300.0;

        decTimer = roundTime - incTimer;


        TimeSpan timeSpan = TimeSpan.FromSeconds(decTimer);

        time.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);

        if (decTimer < 0 )
        {
            gameOver = true;
            GameObject.Find("StageManager").GetComponent<StageManager>().HumanWin();
          
        }

    }
}
