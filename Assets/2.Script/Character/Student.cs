using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
using Character;

//학생 클래스
public class Student : MonoBehaviour, ICharacter, IPunObservable
{
    public float RunSpd { get; set; } //달리기 속도
    public bool IsInvulerable { get; set; } //무적시간
    public Transform[] items; // 아이템 오브젝트(기본 비활성화 상태)
    public GameObject bitePos;
    public GameObject bloodProjecter;
    public ParticleSystem bloodParticle;
    public ParticleSystem healParticle;
    public GameObject plagueParticle;

    public int life { get; set; } // 남은 치료 가능 횟수
    private Animator anim;
    private Rigidbody rigid;
    public AudioClip[] clips; //오디오 클립 배열
    public bool isRunning = false;

    // 유진님 추가 변수// ~ 2022-10-28
    public float infectTime = 20.0f; // 감염시간 20초(임시)
    private Vector3 dir; //Run,Walk 등에 쓰인 dir 위로 뺐어요~
    public bool isInjured = false; //부상 상태인지 확인하기 위한 bool형 변수 
    public bool isHealing = false;
    public bool canMove = true; //움직일 수 있는지 확인하기 위한 bool형 변수 
    public float stillTime;

    //이상준이 추가한 변수  ~ 2022-10-29
    private IEnumerator interaction;
    public IEnumerator transition;
    public IEnumerator lookAround;

    private GameObject interactObj;
    private ItemStorage itemStorage;
    private float interactDelay;
    public IItem currentItem;

    public bool isMine;
    private bool isDead;
    private UIManager uiManager;
    private ItemManager itemManager;
    public Transform cameraArm;
    public Transform characterBody;
    public Transform minimap;

    //포톤뷰 변수 
    public PhotonView pv;
    private Vector3 net_currPos;
    private Quaternion net_currRot;
    private int net_anim;

    public StudentSoundCtrl audio;
    private string audioName;
    private int lastHitZombie;

    public LayerMask obstacleMask;

    //2022-11-15 유진 추가(계단 체크)
    //레이저를 쏨
    private Ray ray;
    //레이저에 맞은 물체의 정보 받아오기
    private RaycastHit hitInfo;
    //캐릭터가 올라갈 수 있는 최대 경사각
    float maxSlopeAngle = 45.0f;


    private void OnTriggerEnter(Collider other)
    {//반짝이게 하는 파티클 보이게 하기.(네트워크 처리(RCP) 필요없음)
        if (pv.isMine)
        {
            if (other.gameObject.tag == "ItemStorage" || other.gameObject.tag == "ImdUseItem")
            {
                Vector3 dirToTarget = (other.gameObject.transform.position - characterBody.transform.position).normalized;
                float dstToTarget = Vector3.Distance(characterBody.transform.position, other.gameObject.transform.position);
                if (!Physics.Raycast(characterBody.transform.position + new Vector3(0, 1.5f, 0), dirToTarget, dstToTarget, obstacleMask))
                {
                    Debug.Log(other.gameObject.tag);
                    interactObj = other.gameObject;
                    IsInteractable(true);
                }
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (pv.isMine)
        {
            if (other.gameObject.tag == "ItemStorage" || other.gameObject.tag == "ImdUseItem")
            {
                IsInteractable(false);
                interactObj = null;
            }
        }
    }

    [PunRPC]
    public void AllSounOff()
    {
        audio.Net_SoundOff("audio");
        audio.Net_SoundOff("audioBody");
        audio.enabled = false;
    }

    //2022-11-15 유진 추가
    //현재 캐릭터가 경사면에 있는지를 판별하는 bool형 함수 추가
    public bool IsOnSlope()
    {
        ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out hitInfo, 150.0f))
        {
            //ray를 지면에 쐈을 때, 부딪힌 평면의 법선 벡터(normal)와 Vector3.up 사이의 각도로
            //캐릭터가 경사에 있는지를 확인할 수 있음 (0이면 평지, 아니면 경사)
            var angle = Vector3.Angle(Vector3.up, hitInfo.normal);
            //각도가 0이 아니면서 maxSlopeAngle보다 작거나 같을 때,
            //즉 경사면 위에 있을 때 true 반환
            return angle != 0f && angle <= maxSlopeAngle;
        }

        return false;
    }

    //2022-11-15 유진 추가(캐릭터의 방향 설정)
    public Vector3 SetDirection(Vector3 direction)
    {
        //현재 캐릭터가 서 있는 경사 지형 평면 벡터로 이동 방향 벡터 투영
        return Vector3.ProjectOnPlane(direction, hitInfo.normal).normalized;
    }

    private void Awake()
    {
        bloodParticle.Stop();
        pv = GetComponent<PhotonView>();
        pv.ObservedComponents[0] = this;
        pv.synchronization = ViewSynchronization.UnreliableOnChange;
        gameObject.name = pv.owner.NickName;
        lastHitZombie = 0;
        bloodProjecter.SetActive(false);
        healParticle = transform.Find("HealParticle").GetComponent<ParticleSystem>();
        healParticle.Stop();
        plagueParticle = transform.Find("PlagueParticle").gameObject;
        plagueParticle.SetActive(false);

        isRunning = false;
        isDead = false;
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody>();
        audio = GetComponent<StudentSoundCtrl>();
        audioName = "Walk";
        if (pv.isMine)
        {
            this.gameObject.layer = 0;
            uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
            uiManager.SetPlayer(this.gameObject);
            //GameObject.Find("StageManager").gameObject.GetComponent<StageManager>().SetPlayerStateAndName(false, pv.owner.NickName);
            itemManager = GameObject.FindGameObjectWithTag("ItemManager").GetComponent<ItemManager>();

        }
        if (!pv.isMine)
        {
            cameraArm.gameObject.SetActive(false);
            minimap.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        interactObj = null;
        for (int i = 0; i < items.Length; i++)
        {
            items[i].gameObject.SetActive(false);
        }

        life = 3; //남은 치료 가능 횟수 3설정 감염시 -1
        RunSpd = 3.0f; //임시 달리기 속도
        interactDelay = 0f;

        currentItem = null;

        net_anim = 0;
        if (!pv.isMine)
        {
            StartCoroutine(this.NetAnimSet());
        }
        // SetActiveItem(ItemType.Vaccine);
    }

    IEnumerator NetAnimSet()
    {
        yield return null;

        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            switch (net_anim)
            {
                case 0:
                    anim.SetFloat("Speed", 0);
                    break;
                case 1:
                    anim.SetFloat("Speed", 3);
                    anim.SetBool("Run", false);
                    RunSpd = 3;
                    break;
                case 2:
                    anim.SetFloat("Speed", 11);
                    anim.SetBool("Run", true);
                    RunSpd = 7;
                    break;
                case 3:
                    anim.SetBool("IsStill", true);
                    break;
                case 4:
                    anim.SetTrigger("Bash");
                    break;
                case 5:
                    anim.SetBool("IsStill", false);
                    break;
                case 6:
                    anim.SetTrigger("PickUp");
                    break;
                case 7:
                    anim.SetTrigger("Throw");
                    break;
                case 8:
                    anim.SetTrigger("Drink");
                    break;
                case 9:
                    anim.SetTrigger("Scratch");
                    anim.SetBool("IsInjured", true);
                    break;
                case 10:
                    anim.SetTrigger("Bit");
                    break;
                //case 11:
                //    anim.SetBool("IsInjured", true);
                //    break;
                case 12: // 마시는 애니 + ("IsInjured", false)  가 연속해서 안되서 일단 이렇게 함.
                         // anim.SetTrigger("Drink");
                    anim.SetBool("IsInjured", false);
                    healParticle.Play() ;
                    bloodProjecter.SetActive(false);
                    plagueParticle.SetActive(false);
                    break;
                case 13:
                    anim.SetTrigger("Die");
                    break;
            }
        }

    }


    private void FixedUpdate()
    {    //컴퓨터 버튼 설정
         //1 버튼 : 아이템 사용 
         //E 버튼 : 상호작용 


        //주석 해제 필요 
        //아이템이 물약일 때, 감염 상태일 때
        //버튼 누르면(버튼으로 바꿔야함) GetInjured 코루틴 중지 및 Heal 코루틴 실행
        //if (/*interactObj.type==2&&*/Input.GetKeyDown(KeyCode.E) && isInjured)
        //{
        //Q. 만약 무적 상태로 3초가 지나기 전에 이 코루틴을 사용한다면...
        //IsInvulnerable은 계속 true인 상태가 되므로,,, Healed에 IsInvulnerable = false를 넣어줌




        //StopCoroutine(GetInjured());
        // StartCoroutine(Heal());
        // Debug.Log(infectTime);
        //}
        Run();

        if (pv.isMine)
        {
            //if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") &&
            //   anim.GetFloat("Speed") == 0 && canMove)
            //{
            //    if (!isInjured) //LookAround 코루틴을 타지 않도록 함
            //    {
            //        StartCoroutine(LookAround());
            //    }
            //}
            //else if (anim.GetCurrentAnimatorStateInfo(0).IsName("IdleLookAround")
            //    && anim.GetFloat("Speed") != 0 && canMove)
            //{
            //    anim.SetBool("IsStill", false);
            //    string name = "IsNotStill";
            //    pv.RPC("SetPhotonAnim", PhotonTargets.Others, name);

            //}

            //if (anim.GetCurrentAnimatorStateInfo(1).IsName("Drinking") && net_anim != 8 && currentItem.Durability > 0)
            //{
            //    net_anim = 8;
            //}
            //if (anim.GetCurrentAnimatorStateInfo(1).IsName("IsNotInjured") && net_anim == 8)
            //{
            //    net_anim = 12;
            //}
        }

        //컴퓨터 버튼 설정
        //1 버튼 : 아이템 사용 
        //E 버튼 : 상호작용 
        //Left Shift 버튼: Run / Walk 전환  
    }

    public void Update()
    {
        if (pv.isMine)
        {
            //쉬프트 누르면 걷기/뛰기 모드 전환 가능 
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isRunning = !isRunning;
                //Debug.Log(isRunning);
                if (isRunning)
                {
                    RunSpd = 7.0f;
                    anim.SetBool("Run", true);
                    audioName = "Run";
                }
                else
                {
                    RunSpd = 3.0f;
                    anim.SetBool("Run", false);
                    audioName = "Walk";
                }
            }

            //걷기, 뛰기 애니메이션 전환


            //수정 필요
            //회복약 마실 때, 주울 때, 공격 당했을 때(두 개), 죽을 때 움직이지 못하도록 함
            //if문 안의 조건들 나중에 상호작용 하면서 else로 나누기!(위치,방향 조정 필요)
            if (anim.GetCurrentAnimatorStateInfo(1).IsName("Drinking") ||
                anim.GetCurrentAnimatorStateInfo(0).IsName("PickingUp") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("PickingUp") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Bit") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Scratch") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Die") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Bash") ||
                anim.GetCurrentAnimatorStateInfo(1).IsName("Throw"))
            {
                rigid.velocity = (dir * 0);
                canMove = false;
                //PickingUp의 경우 위치가 안맞아 어색하면 else if로 따로 빼고 위치 설정 변수 추가
                //바닥에 떨어진 아이템 줍는거랑, 사물함에서 꺼내는거 따로 처리
                //물릴 때, 죽을 때 움직이지 못하도록 하기
            }
            else
            {
                canMove = true;
            }


            //3초 이상 가만히 서있을 때 idle 상태를 전환(두리번거리기)
            //코드가 좀 맘에 안듦 ㅠ
            //더 좋은 방법 있으면 수정하기! 

            //else
            //{
            //    stillTime = 3.0f;
            //    //Debug.Log(stillTime);
            //}

            //목숨이 0개가 되거나 감염 시간 내에 물약을 먹지 못했을 경우 사망
            //    if (life == 0 || infectTime <= 0)
            //{
            //    Death();
            //}

            //왼쪽 위 가로 숫자패드 1누를 시 아이템 사용 (PC 기준)
            if (Input.GetKeyDown(KeyCode.Alpha1) && currentItem != null && canMove)
            {
                canMove = false;
                //Debug.Log("item activated");
                currentItem.Use(this.gameObject);
            }

            


        }
    }

    IEnumerator LookAround()
    {
        yield return null;
        float leftTime = Time.time + 3.0f;
        while (true)
        {
            yield return null;
            if (Time.time > leftTime)
            {
                string name = "IsStill";
                pv.RPC("SetPhotonAnim", PhotonTargets.All, name);

                break;
            }
        }
        yield return null;
    }

    // 걷기/달리기 (이동)메서드 
    public void Run()
    {
        if (pv.isMine)
        {
            if (canMove)
            {
                dir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
                anim.SetFloat("Speed", dir != Vector3.zero ? RunSpd : 0);
                if (anim.GetFloat("Speed") != 0)
                {
                    Vector3 camForward = new Vector3(cameraArm.forward.x, 0f, cameraArm.forward.z).normalized;
                    Vector3 camRight = new Vector3(cameraArm.right.x, 0f, cameraArm.right.z).normalized;
                    Vector3 moveDir = camForward * dir.z + camRight * dir.x;

                    //2022-11-15 유진 추가
                    //LookRotation 안에 넣어줄 변수...
                    //if문 안에서 moveDir 값을 바꿔줄 것이기 때문에 따로 추가했습니다
                    //선언 안해도 상관은 없지만 가독성을 위해 추가합니다!
                    Vector3 rotateDir = camForward * dir.z + camRight * dir.x;

                    //내리막길을 자연스럽게 걷도록 함
                    Vector3 gravity = Vector3.down * Mathf.Abs(rigid.velocity.y);

                    //경사면에 있다면
                    if (IsOnSlope())
                    {
                        //계단 위에 있으면 각도를 수정해주고, 아니면 원래 설정한 각도로 감
                        moveDir = SetDirection(moveDir);
                        gravity = Vector3.zero;
                    }

                    rigid.velocity = (moveDir * RunSpd) + gravity;

                    characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, Quaternion.LookRotation(rotateDir), Time.deltaTime * 5.0f);
                    minimap.localRotation = Quaternion.Slerp(minimap.localRotation, Quaternion.LookRotation(rotateDir), Time.deltaTime * 2.5f);
                }
                else
                {
                    rigid.velocity = Vector3.zero;
                }
                net_anim = anim.GetFloat("Speed") != 0 ? (RunSpd < 4.0f ? 1 : 2) : 0;
                string _audioName =  anim.GetFloat("Speed") == 0 ? "None" : audioName;
                audio.PlaySound(_audioName);

                if (net_anim == 0)
                {
                    if (lookAround == null && !anim.GetCurrentAnimatorStateInfo(0).IsName("IdleLookAround"))
                    {
                        //Debug.Log("Start lookaround coroutine");
                        lookAround = this.LookAround();
                        StartCoroutine(lookAround);
                    }
                }
                else
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("IdleLookAround"))
                    {
                        //Debug.Log("Stop lookaround coroutine");
                        lookAround = null;
                        string name = "IsNotStill";
                        pv.RPC("SetPhotonAnim", PhotonTargets.All, name);
                    }
                }
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, net_currPos, Time.deltaTime * 8.0f);
            characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, net_currRot, Time.deltaTime * 5.0f);

        }


    }


    //데미지 처리 메서드. 
    public void TakeDamage(int _viewID , Vector3 vec)
    {
        Vector3 targetPos = Vector3.zero;
        if(_viewID > 999)
        {
            GameObject zombie = PhotonView.Find(_viewID).gameObject.transform.Find("ZombieBody").Find("BitePos").gameObject;
            targetPos = new Vector3(zombie.transform.position.x, zombie.transform.parent.transform.position.y, zombie.transform.position.z);
            //pv.RPC("SetBloodPos", PhotonTargets.All, zombie.transform.position);
        }
        pv.RPC("Injured", PhotonTargets.All, _viewID, targetPos);
    }

    [PunRPC]
    void SetBloodPos(Vector3 pos)
    {
       if(bloodParticle != null)
        {
            bloodParticle.transform.position = pos;
        }
        
    }

    IEnumerator fixPos(Vector3 targetPos, GameObject target = null)
    {
        yield return null;
        float currTime = Time.time;
        Vector3 vec = Vector3.zero;
       
        if (target != null)
        {// 저장소 상호작용
            transform.position = targetPos;
            Debug.Log(target.transform.position);
            Vector3 targetBodyPos = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
            vec = targetBodyPos - transform.position;
            vec.Normalize();
        }
        else
        {
            rigid.isKinematic = true;
        }
        while (true)
        {
           if(target == null)
            {
                transform.position =targetPos;
            }
            yield return null;
            if (vec != Vector3.zero) characterBody.localRotation = Quaternion.Slerp(characterBody.localRotation, Quaternion.LookRotation(vec), Time.deltaTime * 5.0f);
            yield return null;
            if (Time.time > currTime + 1.5f)
            {
                bloodParticle.Stop();
                rigid.isKinematic = false;
                break;
            }
        }
    }

    [PunRPC]
    void Injured(int _viewID , Vector3 targetPos)
    {
        rigid.velocity = Vector3.zero;
        
        if (_viewID > 999)
        {
            StartCoroutine(fixPos(targetPos));
            bloodParticle.Play();
        }

        
        if (pv.isMine)
        {
            lastHitZombie = _viewID;
            Debug.Log("_viewID"+_viewID);
        }

        Debug.Log("Injured method started");
        if (IsInvulerable) return;
        if (isHealing) isHealing = false;
        audio.PlayBodySound("Hit");
        anim.SetTrigger("Scratch");
        anim.SetBool("IsInjured", true);

        StartCoroutine(this.IsInvulerableTime());
        Invoke("OnProjecter", 1.5f);
        
        if (pv.isMine)
        {
            life--; //데미지 입었을 때 life 1 감소 UI로도 표현해주면 좋을듯 함
            if (life <= 0)
            {

                Invoke("Death", 2.5f);
                //return;
            }

            if (!isInjured) //감염 상태도 아니고, 무적도 아닐 때
            {
                Debug.Log("Take Damege");
                StageManager stageManager = GameObject.Find("StageManager").GetComponent<StageManager>();
                stageManager.SetTransition(pv.viewID, true);
                transition = this.GetInjured();
                StartCoroutine(transition);
            }
        }
    }

    void OnProjecter()
    {
        if(bloodProjecter != null)
        {
            bloodProjecter.SetActive(true);
            plagueParticle.SetActive(true);
        }
    }


    IEnumerator IsInvulerableTime()
    {
        Debug.Log("isInvulerableTime");
        IsInvulerable = true;
        yield return null;
        
        float invulerableTime = Time.time + 3.0f;
        while (true)
        {
            yield return null;
            if (Time.time > invulerableTime)
            {
                Debug.Log("Invulerable Time Over");
                IsInvulerable = false;
                break;
            }
        }
    }


    //피격 후 20초동안 감염 상태로 전환되는 메서드
    IEnumerator GetInjured()
    {
        //애니메이션 시간은 Has Exit Time으로 처리했음 
        isInjured = true;

        yield return new WaitForSeconds(3.0f);
        float lastBittenTime = Time.time;
       
        while (true)
        {
            yield return null;
            audio.PlayBodySound("Scary");
            Debug.Log("left Time :" + (10.0f - (Time.time - lastBittenTime)));
            if (Time.time > lastBittenTime + 10.0f)
            {
                if (isInjured)
                {
                    Debug.Log("Dead");
                    Death();
                    break;
                }
            }
        }
        yield return null;
    }

    //좀비로 변이하는 메서드,미완 (수정 필요)
    void Death()
    {
        canMove = false;
        StopAllCoroutines();
        pv.RPC("SetPhotonAnim", PhotonTargets.Others, "Death");
        audio.PlayBodySound("Death");
        anim.SetTrigger("Die");
        SetActiveItem(ItemType.None);
        pv.RPC("RPCSetActiveItem", PhotonTargets.Others, (int)ItemType.None);
        GameObject.Find("StageManager").GetComponent<StageManager>().Transition(characterBody, pv.viewID);
    }

    //감염 상태에서 치료제를 먹고 회복했을 때 메서드
    public void Heal()
    {
        Debug.Log("healStart");
        if (transition != null)
        {
            Debug.Log("Got heal");
            lastHitZombie = 0;
            audio.PlayBodySound("Health");
            StopCoroutine(transition);
            plagueParticle.SetActive(false);
            bloodProjecter.SetActive(false);
            transition = null;
            GameObject.Find("StageManager").GetComponent<StageManager>().SetTransition(pv.viewID, false);
            healParticle.Play();

            isInjured = false;
            anim.SetBool("IsInjured", false);
            net_anim = 12;
        }

    }


    public void StartAnim()
    {
        StartCoroutine(AnimCtrl());
    }

    //공격 메서드
    IEnumerator AnimCtrl()
    {
        yield return new WaitForSeconds(0.1f);
        canMove = false;
        if (currentItem.Type == ItemType.Bat || currentItem.Type == ItemType.Mop) //방망이,대걸레일 때 
        {
            Debug.Log("bat / mop used");
            anim.SetTrigger("Bash");
            string name = "Bash";
            pv.RPC("SetPhotonAnim", PhotonTargets.Others, name);

            if (currentItem.Type == ItemType.Bat) { name = "BatSwing"; } else { name = "MopSwing"; }

            itemManager.pv.RPC("SoundPlay", PhotonTargets.All, transform.position, name );
        }
        else if (currentItem.Type == ItemType.Baseball) //야구공일 때 
        {
            anim.SetTrigger("Throw");
            yield return new WaitForSeconds(0.5f);
            itemManager.pv.RPC("SoundPlay", PhotonTargets.All, transform.position, "BallThrow");
        }
        else if (currentItem.Type == ItemType.Vaccine) //야구공일 때 
        {
            anim.SetTrigger("Drink");
            string name = "Drink";
            pv.RPC("SetPhotonAnim", PhotonTargets.Others, name);
            audio.PlayBodySound("Drink");
        }
        //StartCoroutine(this.CheckDurability());

        ////else if(interactObj.type==6) //구조물 일 때...수정 필요
        ////{
        //    anim.SetTrigger("PicknAttack");
        ////}

    }

    //3초간 가만히 서있으면 주변을 둘러보는 애니매이션으로 전환
   

  

    public void IsInteractable(bool temp) //이상준이  추가한 메서드
    {
        if (temp)
        {
            Debug.Log("interaction started!!");
            uiManager.ShowIntrText(true, interactObj.gameObject.tag);
            interaction = this.Interaction();
            StartCoroutine(interaction);
        }
        else
        {
            Debug.Log("interaction stopped");
            uiManager.ShowIntrText(false, "");
            StopCoroutine(interaction);
        }
    }

    //임시 주위에 상호작용 가능한 ItemStorage 오브젝트가 있을시 발동되는 코루틴 메서드.
    IEnumerator Interaction()
    {//(추가 요소) 화면에 UI도 표시해야됨.
        yield return null;
        while (true)
        {
            yield return new WaitForSeconds( interactDelay);
            interactDelay = interactDelay > 0 ? 0 : interactDelay;
            if (interactObj != null  && interactObj.GetComponent<Collider>().enabled == false)
            {
                Debug.Log("Interact Over");
                IsInteractable(false);
            }
            if(interactObj != null )
            {
                float dis = Vector3.Distance(transform.position, interactObj.transform.position);
                if (Input.GetKeyUp(KeyCode.E) && dis <= 2.5f && canMove)  // 해당 ItemStorage와 2.5f 거리 이내이고 E키를 누를 시
                {

                    switch (interactObj.gameObject.tag)
                    {
                        case "ItemStorage":
                            itemStorage = interactObj.GetComponent<ItemStorage>();
                            if (currentItem == null || itemStorage.SetItemActive(true, this.gameObject))
                            {
                                Debug.Log("Get Item");

                                anim.SetTrigger("PickUp");
                                string name = "PickUp";
                                pv.RPC("SetPhotonAnim", PhotonTargets.All, name);
                                
                                GameObject fixObj = null;
                                if ((fixObj = interactObj.GetComponent<ItemStorage>().fixPos )!= null)
                                {
                                    Vector3 fixPos = new Vector3(fixObj.transform.position.x, transform.position.y, fixObj.transform.position.z);
                                    StartCoroutine(this.fixPos(fixPos, interactObj.transform.GetChild(5).gameObject));
                                }
                                SetActiveItem(itemStorage.Type);
                                IsInteractable(false);
                                break;
                            }
                            else
                            {
                                Debug.Log("Open Item");
                                interactDelay = 2.0f;
                                anim.SetTrigger("PickUp");
                                string name = "PickUp";
                                pv.RPC("SetPhotonAnim", PhotonTargets.All, name);

                                GameObject fixObj = null;
                                if ((fixObj = interactObj.GetComponent<ItemStorage>().fixPos) != null)
                                {
                                    Vector3 fixPos = new Vector3(fixObj.transform.position.x, transform.position.y, fixObj.transform.position.z);
                                    StartCoroutine(this.fixPos(fixPos, interactObj));
                                }
                                //net_anim = 6;
                            }
                            break;

                        case "ImdUseItem":
                            Debug.Log("ImdUseItem");
                            interactObj.GetComponent<IItem>().Use(this.gameObject);
                            IsInteractable(false);
                            yield break;
                    }
                }
            }
        }
    }

    

    //케릭터 손(ItemPos)의 하위 오브젝트들의 SetActive로 관리하는 메서드
    public void SetActiveItem(ItemType type)
    {
        //if (type == ItemType.None) { currentItem = null; }
        if (itemStorage != null) { itemStorage.isTaken = true; itemStorage = null; }
        if (uiManager.CheckHavingSame(type)) {
            Debug.Log("Have Same Item");
            itemManager.pv.RPC("DropItemStorage", PhotonTargets.All, transform.position, (int)type, GetDurability(type)); //소지 아이템 타입, 내구도로 드랍 아이템 생성.
            int durability = interactObj.GetComponent<ItemStorage>().durability; // 생성된 아이템 내구도값 비교
            if (durability != 0) { SetDurability(type, durability); }
            return;
        }
        
        if (interactObj != null) interactObj = null;

        //먼저 Ui 매니저로 인벤토리에 아이템을 추가하고 1번 아이템이 바뀔 경우 현재 스크립트내의 소지 아이템 타입과 비교하여 다를 경우 setActive하기
        ItemType firstSlotType = uiManager.SetItemSlot(type);
        if(currentItem == null || currentItem.Type != firstSlotType)
        {
            pv.RPC("RPCSetActiveItem", PhotonTargets.All, (int)firstSlotType);
        }
    }

    //소지 아이템의 내구도를 변경하는 메서드
    void SetDurability (ItemType type, int _durability)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetComponent<IItem>() != null && items[i].GetComponent<IItem>().Type == type)
            {
                items[i].GetComponent<IItem>().Durability =_durability;
                Debug.Log("Durability :" + items[i].GetComponent<IItem>().Durability);
            }
        }
    }
    //소지 아이템의 내구도 반환 메서드
    int GetDurability (ItemType type)
    {
        int durability = 1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetComponent<IItem>() != null && items[i].GetComponent<IItem>().Type == type)
            {
                durability = items[i].GetComponent<IItem>().Durability;
            }
        }
        return durability;
    }

    [PunRPC]
    void RPCSetActiveItem(int type)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].GetComponent<IItem>() != null && items[i].GetComponent<IItem>().Type == (ItemType)type)
            {
                if (pv.isMine)
                {
                    currentItem = items[i].GetComponent<IItem>();
                }
                items[i].gameObject.SetActive(true);
            }
            else
            {
                items[i].gameObject.SetActive(false);
            }
        }

        if((ItemType)type == ItemType.None && pv.isMine)
        {
            currentItem = null;
        }
    }

    [PunRPC] //야구공 던지는 RPC 메서드
    public void UseBall()
    {
        StartCoroutine(this.ThrowBall());
    }

    IEnumerator ThrowBall()
    {
        anim.SetTrigger("Throw");
        GameObject ballObj = items[3].gameObject;
        ballObj.GetComponent<MeshRenderer>().enabled = false;
        Vector3 spawnPos = ballObj.transform.position;
        GameObject ball = Instantiate(items[3].gameObject, spawnPos, Quaternion.identity);
        ball.transform.parent = this.gameObject.transform;

        yield return new WaitForSeconds(0.8f);
        ball.transform.parent = null;
        ball.transform.forward = characterBody.forward;  //공의 로컬 방향 케릭터의 로컬 방향으로 설정
        ball.GetComponent<SphereCollider>().isTrigger = true;
        ball.GetComponent<SphereCollider>().radius = 4;
        Vector3 currentPos = ball.transform.position;
        while (true)
        {
            yield return null;
            ball.transform.Translate(Vector3.forward * 2.0f);
            float dis = Vector3.Distance(currentPos, ball.transform.position);
            if (dis >= 10)
            { //거리가 10 이상이면 해당 오브젝트 삭제
                Destroy(ball.gameObject);
                break;
            };
        }
        RPCSetActiveItem((int)ItemType.None);
    }

    [PunRPC] // 백신 하위 오브젝트 활성화 오브젝트
    public void PillsActive()
    {
        items[0].gameObject.GetComponent<Vaccine>().SetPillsActive();
    }


    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = pv.instantiationData;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(characterBody.localRotation);
            stream.SendNext(net_anim);
        }
        else { 
            net_currPos = (Vector3)stream.ReceiveNext();
            net_currRot = (Quaternion)stream.ReceiveNext();
            net_anim = (int)stream.ReceiveNext();
            
            //Debug.Log("Read anim" + net_anim);
        }
    }

    [PunRPC]
    void SetPhotonAnim(string aniName)
    { // net_anim이 잘 전달되지 않아서 임의로 만든 애니메이션 RPC 함수

        switch (aniName)
        {
            case "Drink":
                anim.SetTrigger("Drink");
                break;
            case "IsInjured":
                anim.SetBool("IsInjured", false);
                break;
            case "Death":
                anim.SetTrigger("Die");
                break;
            case "IsStill":
                anim.SetBool("IsStill", true);
                break;
            case "IsNotStill":
                anim.SetBool("IsStill", false);
                break;
            case "Bash":
                anim.SetTrigger("Bash");
                break;
            case "PickUp":
                anim.SetTrigger("PickUp");
                break;

        }
    }
}
