using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    bool isInven = false;
    public GameObject inventory;
    bool isMacro = false;
    public GameObject macroChat;
    public GameObject humanUI;
    public GameObject zombieUI;
    public GameObject resultUI;
    //2022-11-18 원빈 수정 - 기존 배열 삭제 
    public GameObject resultItem;
    public GameObject resultPanel;
    public GameObject transitionParticle;

    private int spawnNum = 0;

    PhotonView pv;
    bool modeUI;
    csSoundManager soundManager;

    public GameObject[] HspawnPos;
    public GameObject ZspawnPos;
    private UIManager uiManager;

    //2022-11-18 원빈 추가 DB 
    string GetResultURL = "http://teamzombie.dothome.co.kr/Update.php";
    List<Player> ranking = new List<Player>();
    string[] currentArray = null;
    public int score;
    public string type;
    public bool victory;
    public double time;
    public float tiome;
    public int infection;

    private int myNum;
    private int leftHuman;


    //타이머 구현을 위한 변수
    bool startTimer = false;
    double timerIncrementValue;
    double startTime;
    [SerializeField] double timer = 20;
    ExitGames.Client.Photon.Hashtable CustomeValue;

    //2022-11-22
    //Restart 버튼 누르면 비활성화해둔 RoomPhoton 요소들 살려야 함
    public csRoomPhoton roomPhoton;

    void Awake()
    {
        myNum = GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>().myNum;

        spawnNum = 0;
        time = 0;
        pv = GetComponent<PhotonView>();
        soundManager = GameObject.Find("S_Canvas").transform.GetComponentInChildren<csSoundManager>();
        modeUI = GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>().isHuman;
        uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        pv.RPC("SetUI", PhotonTargets.AllBuffered, null);
        StartCoroutine(this.CreatePlayer());

        //2022-11-22 추가
        roomPhoton = GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>();

        soundManager.PlayBgm(11);

        if (PhotonNetwork.player.IsMasterClient)
        {
            Debug.Log("MASTER START");
            CustomeValue = new ExitGames.Client.Photon.Hashtable();
            startTime = PhotonNetwork.time;
            startTimer = true;
            CustomeValue.Add("StartTime", startTime);
            PhotonNetwork.room.SetCustomProperties(CustomeValue);
        }
        else
        {
            //startTime = double.Parse(PhotonNetwork.room.CustomProperties["StartTime"].ToString());
            //  startTimer = true;
        }
    }


    IEnumerator Start()
    {

        Camera.main.gameObject.SetActive(false);
        //2022-11-18 원빈 추가
        pv.RPC("SetUI", PhotonTargets.AllBuffered, null);
        yield return new WaitForSeconds(1f);
        //StartCoroutine(GetResult(PhotonNetwork.playerName));
        yield return null;

        uiManager.SetTime(startTime, startTimer);

    }

    IEnumerator CreatePlayer()
    {
        if (modeUI == true)
        {
            type = "Human";
            GameObject st = null;
            if (myNum >= 6)
            {
                st = PhotonNetwork.Instantiate("Student", HspawnPos[0].transform.position, Quaternion.identity, 0) as GameObject;
            }
            else
            {
                st = PhotonNetwork.Instantiate("Student", HspawnPos[myNum -1].transform.position, Quaternion.identity, 0) as GameObject;
            }
           
            
            pv.RPC("SetLeftHuman", PhotonTargets.MasterClient, false);
           
            SetPlayerStateAndName(false, st.GetComponent<Student>().pv.owner.NickName, st.GetComponent<Student>().pv.viewID);
        }
        else
        {
            type = "Zombie";
            GameObject zo = PhotonNetwork.Instantiate("Zombie", ZspawnPos.transform.position, Quaternion.identity, 0);
            ShowTransitionParticle(ZspawnPos.transform.position);
            SetPlayerStateAndName(true, zo.GetComponent<ZombieCtrl>().pv.owner.NickName, zo.GetComponent<ZombieCtrl>().pv.viewID);
        }
        yield return null;
    }



    public void Transition(Transform characterBody, int studentViewID)
    {
        //ZombieCtrl zombie = PhotonView.Find(zombieViewID).GetComponent<ZombieCtrl>();
        //Debug.Log("zombieViewID" +zombieViewID);

        double _startTime = double.Parse(PhotonNetwork.room.CustomProperties["StartTime"].ToString());
        double incTimer = 0;

        incTimer = PhotonNetwork.time - _startTime;
        time = (int)incTimer;

        StartChasingBgm(false);
        victory = false;
        pv.RPC("SetLeftHuman", PhotonTargets.MasterClient, true);
        //zombie.pv.RPC("AddKillScore", PhotonTargets.All);

        ChangeStageToZombie(studentViewID);
        StartCoroutine(this.Reborn(characterBody));
    }

    [PunRPC]
    public void SetLeftHuman(bool isMinus)
    {
        if (isMinus)
        {
            Debug.Log("Human count Minus ");
            leftHuman -= 1;
            Debug.Log("left human : " + leftHuman);
            if (leftHuman == 0)
            {
                pv.RPC("ZombieWin", PhotonTargets.All);
            }
        }
        else
        {
            Debug.Log("Human count Plus ");
            leftHuman += 1;
        }
    }


    IEnumerator Reborn(Transform characterBody)
    {
        yield return new WaitForSeconds(3.0f);
        GameObject.Find("RoomPhoton").GetComponent<csRoomPhoton>().isHuman = false;
        modeUI = false;
        SetUI();
        characterBody.root.gameObject.SetActive(false);
        GameObject zombie = PhotonNetwork.Instantiate("Zombie", characterBody.position, Quaternion.identity, 0) as GameObject;
        pv.RPC("ShowTransitionParticle", PhotonTargets.All, characterBody.position);
        //transitionParticle
        zombie.GetComponent<ZombieSoundCtrl>().PlaySound("Transition");
        PhotonNetwork.Destroy(characterBody.root.gameObject);
    }
    [PunRPC]
    void ShowTransitionParticle(Vector3 pos)
    {
        GameObject particle = Instantiate(transitionParticle, pos, Quaternion.Euler(-90,0,0)) as GameObject;

        Destroy(particle, 2.0f);
    }

    //2022-11-18 원빈 수정
    //[PunRPC]
    //void SetUI()
    //{
    //    if (modeUI)
    //    {
    //        humanUI.SetActive(true);
    //        uiManager.mainUi = humanUI;
    //    }
    //    else
    //    {
    //        zombieUI.SetActive(true);
    //        uiManager.mainUi = zombieUI;
    //    }
    //}

    //게임 시작시 자신의 상태를 다른 네트워크 플레이어 상태창에 입력하는 메서드
    public void SetPlayerStateAndName(bool isZombie, string name, int viewID)
    {
        pv.RPC("SetStateName", PhotonTargets.Others, isZombie, name, viewID);
    }

    [PunRPC]
    void SetStateName(bool isZombie, string name, int viewID)
    {
        uiManager.SetOhersStateName(isZombie, name, viewID);
    }

    //다른 플레이어 상태창에 자신의 상태를 바꾸는 메서드
    public void ChangeStageToZombie(int viewID)
    {
        pv.RPC("RPCChangeStageToZombie", PhotonTargets.Others, viewID);
    }
    [PunRPC]
    void RPCChangeStageToZombie(int viewID)
    {
        uiManager.ChangeOthersState(viewID);
    }


    //감염 시 남은 감염시간을 이미지로 보여주기 위한 코루틴 실행 네트워크 함수
    public void SetTransition(int viewID, bool isActive)
    {
        pv.RPC("RPCStartTransition", PhotonTargets.Others, viewID, isActive);
    }
    [PunRPC]
    void RPCStartTransition(int viewID, bool isActive)
    {
        uiManager.StartTransitionImg(viewID, isActive);
    }

    [PunRPC]
    void SetUI()
    {
        if (modeUI)
        {
            humanUI.SetActive(true);
            zombieUI.SetActive(false);
            uiManager.SetMainUi(humanUI);
          
        }
        else
        {
            zombieUI.SetActive(true);
            humanUI.SetActive(false);
            uiManager.SetMainUi(zombieUI);
           
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            resultUI.SetActive(true);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            resultUI.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            uiManager.ShowBigMap(true, modeUI);
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            uiManager.ShowBigMap(false, modeUI);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            uiManager.ShowInventory();
        }

    }


    void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
    }

    public void BackLooby()
    {
        roomPhoton.SendMessage("BackLobby", SendMessageOptions.DontRequireReceiver);
        //PhotonNetwork.LeaveRoom();
    }

    public void BackRoom()
    {
        roomPhoton.SendMessage("Reactive", SendMessageOptions.DontRequireReceiver);
        SceneManager.UnloadSceneAsync("scNetPlay");
        soundManager.PlayBgm(1);
    }

    public void OnMacroChat()
    {
        if (!isMacro)
        {
            isMacro = true;
            macroChat.SetActive(true);
        }
        else
        {
            isMacro = false;
            macroChat.SetActive(false);
        }
    }
    public void OnInvenOpen()
    {
        if (!isInven)
        {
            isInven = true;
            inventory.SetActive(true);
        }
        else
        {
            isInven = false;
            inventory.SetActive(false);
        }
    }

    //2022-11-18 원빈 추가(DB) - 결과창 출력
    IEnumerator GetResult(string _myName)
    {
        WWWForm form = new WWWForm();

        form.AddField("Name", _myName);

        Result();
        form.AddField("Result", score);
        form.AddField("Type", type);
        form.AddField("Victory", victory.ToString());

        WWW resultServer = new WWW(GetResultURL, form);

        yield return resultServer;

        Registors(resultServer);
        //GameResult();
    }
    void Registors(WWW _dataServer)
    {
        currentArray = System.Text.Encoding.UTF8.GetString(_dataServer.bytes).Split(";"[0]);

        for (int i = 0; i <= currentArray.Length - 3; i = i + 2)
        {
            ranking.Add(new Player(currentArray[i], currentArray[i + 1]));
        }
    }

    void GameResult()
    {
        for (int i = 0; i < ranking.Count; i++)
        {
            GameObject obj = Instantiate(resultItem);
            Player pl = ranking[i];

            if (modeUI.ToString() == "True")
            {
                obj.GetComponent<ResultData>().DisplayResultData(pl.myName, pl.myScore + "(+" + score + ")");
                obj.transform.SetParent(resultPanel.transform);
                obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
            else if (modeUI.ToString() == "False")
            {
                obj.GetComponent<ResultData>().DisplayResultData(pl.myName, pl.myScore + "(-" + score + ")");
                obj.transform.SetParent(resultPanel.transform);
                obj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
        }
    }


    //2022-11-23 유진 : 여긴 rpc 빠진 건가요?W
    public void HumanWin()
    {
        Debug.Log("Human win");
        if (type == "Human" && modeUI)
        {//끝까지 살아남은 인간 플레이어
            Debug.Log("I won");
            victory = true;
        }
        else
        { //처음부터 좀비인 플레이어, 중간에 좀비된 플레이어
            Debug.Log("'I Lose");
            victory = false;
        }
        RPCGameOver();
    }

    [PunRPC]
    public void ZombieWin()
    {
        Debug.Log("Zombie win");
        if (type == "Human")
        {//끝까지 살아남은 인간 플레이어
            Debug.Log("I'm Lose");
            victory = false;
            
            Debug.Log("time == 0" + time);
            double _startTime = double.Parse(PhotonNetwork.room.CustomProperties["StartTime"].ToString());

            double incTimer = 0;
            incTimer = PhotonNetwork.time - _startTime;
            time = incTimer;
            Debug.Log("servival time: "+time);
            
        }
        else
        { //처음부터 좀비인 플레이어, 중간에 좀비된 플레이어
            Debug.Log("'I Win");
            victory = true;
        }
        Invoke("RPCGameOver", 3.0f);
    }



    public void RPCGameOver()
    {
        pv.RPC("PlayerSoundOff", PhotonTargets.All);
        StartCoroutine(GetResult(PhotonNetwork.playerName));

        string resultText = (victory == true ? "Win" : "Lose");
        resultUI.transform.Find("Result").Find("WinTxt").GetComponent<Text>().text = resultText;
        resultUI.SetActive(true);
        soundManager.PlayChasingBgm(false);

        PhotonNetwork.room.IsOpen = true;
        PhotonNetwork.room.IsVisible = true;
    }

    [PunRPC]
    void PlayerSoundOff()
    {
        uiManager.PlayerSoundOff();
    }
    public void ShowResult()
    {

    }

    void Result()
    {
        Debug.Log("my type :" + type);
        Debug.Log("my time :" + time);

        if (type == "Human")
        {
            if (victory)
            {
                Debug.Log("Human win");
                score = 10;
            }
            else
            {
                Debug.Log("Human lose");
                 score = (int)((300 - time) / 300 * 10);
            }
        }
        else if (type == "Zombie")
        {
            if (victory)
            {
                Debug.Log("Zombie win");
                score = 10;
            }
            else
            {
                Debug.Log("Zombie lose");
                score = 10 - infection;
            }
        }

        Debug.Log("score" + score);
    }




    public void StartChasingBgm(bool isActive)
    {
        Debug.Log("StartChasingBgm" + isActive);
        soundManager.PlayChasingBgm(isActive);
    }



}
