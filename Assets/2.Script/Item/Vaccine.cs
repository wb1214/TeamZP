using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Item;
using UnityEngine.UI;

//치료제(물약) 클래스. 
public class Vaccine : MonoBehaviour, IItem
{

    //enum(열거형) 변수. 이 변수값을 비교해서 같으면 use 메서드 실행.
    public ItemType Type { get; set; }
    public int Durability { get; set; }
    public float Cooltime { get; set; }
    public GameObject pills;
    public IEnumerator healing;

    private void Awake()
    {
        Type = ItemType.Vaccine;
    }
    private void Start()
    {
        Durability = 1;
        Cooltime = 0;
    }

    public void Use(GameObject target)
    {//IItem 인터페이스 의 메서드
        if (Durability <= 0) return;

        Student st = target.GetComponent<Student>();
        st.isHealing = true;
        
        st.StartAnim(); //애니메이션 실행
        SetPillsActive();
        st.pv.RPC("PillsActive", PhotonTargets.Others);
   
        healing = this.GetHeal(st);
        StartCoroutine(healing);
        
    }
    public void SetPillsActive()
    {
        pills.SetActive(true);
    }

    IEnumerator GetHeal(Student st)
    {
        yield return new WaitForSeconds(1.0f);
        Durability -= 1;
        float currentTime = Time.time;
        while (true)
        {
            Debug.Log("Left Heal Time"+ (3.0f - (Time.time - currentTime)));
            yield return null;
            if (!st.isHealing)
            {
                Debug.Log("Healing Stopped");
                break;
            }
            else if (Time.time > currentTime + 2.0f)
            {
                Debug.Log("heal");
                st.Heal();
                st.isHealing = false;
              
                break;
            }
        }
        st.pv.RPC("RPCSetActiveItem", PhotonTargets.Others, (int)ItemType.None);
        st.SetActiveItem(ItemType.None);

    }

    //아이템 사용중 피격당할 경우 coroutine 중단하는 기능.

    //SetActive(true) 호출할 때 내구도, Mesh Renderer.endbled = true 로 초기화
    private void OnEnable()
    {
        pills.SetActive(false);
        Durability = 1;
    }
     void OnDisable()
    {
        pills.SetActive(false);
    }

}
