﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
using Character;
using UnityEngine.UI;

//무기 클래스
public class Mop : MonoBehaviour , IItem
{ //enum(열거형) 변수. 이 변수값을 비교해서 같으면 use 메서드 실행.
    public ItemType Type { get; set; }
    public int Durability { get; set; }
    public float Cooltime { get; set; }

    private Collider col;

    private IEnumerator coolingtime;
    private UIManager uIManager;

    private void Awake()
    {
        Type = ItemType.Mop;
    }
    private void Start()
    {
        uIManager = GameObject.Find("UIManager").GetComponent<UIManager>();
        Durability = 2;
        Cooltime = 5.0f;
        col = GetComponent<Collider>();
        col.enabled = false;
    }
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Zombie" && !collision.gameObject.GetComponent<ICharacter>().IsInvulerable)
        {
            Debug.Log("Zombie Damaged ome");
            collision.gameObject.GetComponent<ICharacter>().TakeDamage(1,Vector3.zero);
        }
    }


    public void Use(GameObject target)
    {//IItem 인터페이스 의 메서드
        if (coolingtime != null || Durability <= 0) return;
        col.enabled = true;
        Student st = target.GetComponent<Student>();
        coolingtime = this.Cooling(st, Cooltime);
        StartCoroutine(coolingtime);
        st.StartAnim();
        Durability -= 1;
    }

    IEnumerator Cooling(Student st,float cool)
    {
        yield return null;

        float leftTime = 0;
        while (cool > leftTime)
        {
            leftTime += Time.deltaTime;
            uIManager.ShowCoolTime(leftTime, cool);
            yield return new WaitForFixedUpdate();
        }
        col.enabled = false;
        coolingtime = null;

        if (Durability <= 0) {
            Durability = 2;
            st.pv.RPC("RPCSetActiveItem", PhotonTargets.Others, (int)ItemType.None);
            st.SetActiveItem(ItemType.None);
            col.enabled = false;
        }; 
    }

    //SetActive(true) 호출할 때 내구도 초기화
    private void OnEnable()
    {
      //  Durability = 2;
    }

    private void OnDisable()
    {
        coolingtime = null;
    }
}
