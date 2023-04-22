﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
using Character;
using UnityEngine.UI;

//무기 클래스
public class Baseball : MonoBehaviour , IItem
{ //enum(열거형) 변수. 이 변수값을 비교해서 같으면 use 메서드 실행.
    public ItemType Type { get; set; }
    public Image Image { get; set; }
    public int Durability { get; set; }
    public float Cooltime { get; set; }
    private MeshRenderer ms;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Zombie" && !collision.gameObject.GetComponent<ICharacter>().IsInvulerable)
        {
            Debug.Log("Zombie Damaged ome");
            collision.gameObject.GetComponent<ICharacter>().TakeDamage(1,Vector3.zero);
        }
    }


    private void Awake()
    {
        ms = GetComponent<MeshRenderer>();
        Type = ItemType.Baseball;
    }

    private void Start()
    {
        
        Durability = 1;
        Cooltime = 0;
    }

    public void Use(GameObject target)
    {//IItem 인터페이스 의 메서드
        if(Durability <= 0) return;

        Student st = target.GetComponent<Student>();
        st.StartAnim(); //애니메이션 실행
        st.pv.RPC("UseBall", PhotonTargets.Others);
        StartCoroutine(ThrowBall(st)); // 공을 생성, 던짐.
        
        Durability -= 1;
    }

    // 던지기용 공을 생성하고 던지고, 삭제하는 메서드
    IEnumerator ThrowBall(Student st)
    {
        ms.enabled = false;
        Vector3 spawnPos = transform.position;
        GameObject ball = Instantiate(this.gameObject, spawnPos, Quaternion.identity);
        ball.transform.parent = this.gameObject.transform;
       
        yield return new WaitForSeconds(0.8f);
        ball.transform.parent = null;
        ball.transform.forward = st.characterBody.forward;  //공의 로컬 방향 케릭터의 로컬 방향으로 설정
        ball.GetComponent<SphereCollider>().isTrigger = true;
        ball.GetComponent<SphereCollider>().radius = 4;
        Vector3 currentPos = ball.transform.position;
        while (true)
        {
            yield return null;
            ball.transform.Translate(Vector3.forward * 2.0f);
            float dis = Vector3.Distance(currentPos, ball.transform.position);
            if (dis >= 10) { //거리가 10 이상이면 해당 오브젝트 삭제
                Destroy(ball.gameObject);
                break;
            };
        }
        st.SetActiveItem(ItemType.None);
    }

    //SetActive(true) 호출할 때 내구도, Mesh Renderer.endbled = true 로 초기화
    private void OnEnable()
    {
        ms.enabled = true;
        Durability = 1;
    }

}
