using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//미니맵에 마커를 표시하는 스크립트
public class MinmapDetection : MonoBehaviour
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    private GameObject bigMap;
    public GameObject CharacterBody;
    private float currMapIndex; //케릭터의 층 이동을 확인하기 위한 높이값
    public Material[] mapMats;
    private bool isZombie;

    private GameObject marker; // 기본 마커. 플레이어 마커 표시
    private List<GameObject> markers;
    private Color zombieColor;
    private Color studentColor;

    private float bgmOnStack;
    private float bgmOffStack;
    private float extraTime;


    private int targetStack;
    private bool isBgmPlaying;

    public LayerMask targetMask, obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();

    private void Awake()
    {
        bigMap = GameObject.Find("BigMapBoard");
        transform.position = bigMap.transform.position + new Vector3(0, -1, 0);
        currMapIndex = 0;
        bgmOnStack = 0;
        bgmOffStack = 0;

        targetStack = 0;
        isBgmPlaying = false;

        markers = new List<GameObject>();
        marker = transform.Find("Marker").gameObject;
        marker.transform.position = bigMap.transform.position + new Vector3(0, +1.5f, 0);
        //기본 플레이어 수 + 2로 마커 생성. 일단 6으로 상정
        for (int i = 0; i < 6; i++)
        {
            GameObject _marker = Instantiate(marker, bigMap.transform.position + new Vector3(0, +1, 0), Quaternion.identity) as GameObject;            
            _marker.transform.parent = this.gameObject.transform;
            _marker.transform.localScale = new Vector3(2, 0.1f, 2);
            markers.Add(_marker);
            markers[i].SetActive(false);
        }
        marker.SetActive(true); //
        marker.GetComponent<MeshRenderer>().material.color = Color.green;

        if(transform.root.tag == "Zombie")
        {
            isZombie = true;
            zombieColor = Color.white;
            studentColor = Color.red;
            extraTime = 0.4f;
        }
        else
        {
            isZombie = false;
            zombieColor = Color.red;
            studentColor = Color.white;
            extraTime = 0;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    //전방 시야각 내의 관측 가능 오브젝트의 콜라이더가 있을 시 Marker List에 추가하는 메서드
    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] tartsInViewRadius = Physics.OverlapSphere(CharacterBody.transform.position, viewRadius, targetMask);

        for (int i = 0; i < tartsInViewRadius.Length; i++)
        {
            Transform target = tartsInViewRadius[i].transform;

            Vector3 dirToTarget = (target.position - CharacterBody.transform.position).normalized;
            if (target.gameObject.tag == "ItemStorage" && isZombie)
            {
                continue;
            }
            //if(target.gameObject == transform.root.gameObject)
            //{
            //    continue;
            //}
            if (!isZombie)
            {
                if( Vector3.Angle(CharacterBody.transform.forward, dirToTarget) < 360)
                {
                    float dstToTarget = Vector3.Distance(CharacterBody.transform.position, target.transform.position);
                    if (!Physics.Raycast(CharacterBody.transform.position + new Vector3(0, 1.5f, 0), dirToTarget, dstToTarget, obstacleMask))
                    {
                        if(target.gameObject.tag == "Zombie")
                        {
                            targetStack++;
                        } 
                        if (Vector3.Angle(CharacterBody.transform.forward, dirToTarget) < viewAngle / 2)
                        {
                            visibleTargets.Add(target);
                        }
                    }
                }
            }
            else
            {
                if (Vector3.Angle(CharacterBody.transform.forward, dirToTarget) < viewAngle / 2)
                {
                    float dstToTarget = Vector3.Distance(CharacterBody.transform.position, target.transform.position);
                    Debug.Log("dstToTarget : " + dstToTarget);
                    Debug.DrawLine(CharacterBody.transform.position + new Vector3(0, 1.5f, 0), target.transform.position, Color.red, 5.0f);
                    if (!Physics.Raycast(CharacterBody.transform.position + new Vector3(0, 1.5f, 0), dirToTarget, dstToTarget, obstacleMask))
                    {
                        Debug.Log(" found");
                        // Debug.Log(hit.collider.gameObject.layer);
                        Debug.Log("target is visibile");
                        if (target.gameObject.tag == "Student")
                        {
                            targetStack++;
                        }
                        visibleTargets.Add(target);

                    }
                }
            }
            

        }
        SetMarkerActive(visibleTargets.Count);
        SetBgmStack();
    }
    //Marker List의 길이에 따라 Marker 오브젝트를 활성화 시키는 메서드
    void SetMarkerActive(int targetNum)
    {
        
        for(int i =0; i< markers.Count; i++)
        {
            //Debug.Log(i < targetNum);
            markers[i].SetActive( i < targetNum ? true :false);
            
        }
       
    }

    //범위내의 관측 가능 오브젝트의 tag에 따라 Marker의 색 변경, 오브젝트 위치 트래킹
    private void LateUpdate()
    {
       /// Debug.Log("map to body DIs :" + ( CharacterBody.transform.position.y - transform.position.y));
        float height = CharacterBody.transform.position.y - transform.position.y;
        int index = height < 4 ? 0 : 1;
        if (index != currMapIndex) SetMapMr(index);

        transform.position = new Vector3(CharacterBody.transform.position.x, bigMap.transform.position.y + -1, CharacterBody.transform.position.z);
        //marker.transform.position = Vector3.Lerp(marker.transform.position, new Vector3(CharacterBody.transform.position.x, marker.transform.position.y, CharacterBody.transform.position.z), 0.8f);
        if (visibleTargets.Count != 0)
        {
            for(int i = 0; i < visibleTargets.Count; i++)
            {
                if(visibleTargets[i].gameObject != null)
                {
                    switch (visibleTargets[i].gameObject.tag)
                    {
                        case "Zombie":
                            markers[i].GetComponent<MeshRenderer>().material.color = zombieColor;
                            break;
                        case "ItemStorage":
                            markers[i].GetComponent<MeshRenderer>().material.color = Color.blue;
                            break;

                        case "Student":
                            markers[i].GetComponent<MeshRenderer>().material.color = studentColor;
                            break;
                    }
                    Vector3 targetPos = new Vector3(visibleTargets[i].position.x, markers[i].transform.position.y, visibleTargets[i].position.z);
                    markers[i].transform.position = Vector3.Lerp(markers[i].transform.position, targetPos, 0.8f);
                }
            }  
        }
       
        
    }

    void SetBgmStack()
    {
        if (targetStack > 0 )
        {
            targetStack = 0;
            bgmOffStack = 0;
            if (isBgmPlaying) return;
            bgmOnStack+= Time.deltaTime;
            
            if (bgmOnStack >= 0.18f && !isBgmPlaying)
            { isBgmPlaying = true;
                bgmOffStack = 0;
                bgmOnStack = 0;
              GameObject.Find("StageManager").GetComponent<StageManager>().StartChasingBgm(true);
            }
        }
        else
        {
            targetStack = 0;
            bgmOffStack += Time.deltaTime;
            
            if (bgmOffStack >= 0.2f + extraTime && isBgmPlaying)
            {isBgmPlaying = false;
                bgmOnStack = 0;
                bgmOffStack = 0;
                GameObject.Find("StageManager").GetComponent<StageManager>().StartChasingBgm(false);

            }
        }
        
    }

    void SetMapMr(int index)
    {
        Debug.Log("this is Map " + index + 1);
        MeshRenderer bigMapMr = bigMap.GetComponent<MeshRenderer>();
        bigMapMr.material = mapMats[index];
        currMapIndex = index;
    }

}
