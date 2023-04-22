using UnityEngine;
using UnityEngine.UI;


namespace Item
{
    //아이템 습득시 손에 보이게 활성화
    //Vaccine = 램덤위치에 N분마다 1개씩 생성
    //무기는 최초 N개 생성 후 재생성X
    //N = 미정 
    public enum ItemType
    {
        None,Vaccine, Bat, Mop, Baseball, ImdUseItem//물약(1), 야구 배트(2), 대걸레(1), 소화기(원거리<-미정/ 근거리 내구도 시 2)(1)
    }
    
    public interface IItem
    {
        ItemType Type { get; set; } //아이템 타입 enum형

        int Durability { get; set; }//내구도 변수 

        float Cooltime { get; set; }

        void Use(GameObject target); // 사용시 발동되는 메서드
    }

    public interface IItemStorage
    {
        ItemType Type { get; set; }

        void SetType(ItemType type, int  durability);
    }

}