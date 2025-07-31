using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public enum ItemType
{
    StaminaRecovery,//스테미너 회복
    HealthRecovery,//체력 회복
    ArmorHead,//머리 보호
    ArmorBody,//몸통 보호
    ArmorArm,//팔 보호
    ArmorLeg,//다리 보호
    ArmorHand,//손 보호
    ArmorFeet,//발 보호
    Weapon,//무기

    Material,//재료료
}

public class ItemManager : MonoBehaviour
{
    public List<ItemDataSO> allItems;

    public ItemDataSO GetItemByName(string name)
    {
        return allItems.Find(item => item.itemName == name);
    }

    public void UseItem(ItemDataSO item)
    {
        // 아이템 타입별 동작 분기
        switch(item.ItemType)
        {
            case ItemType.StaminaRecovery:
                // 스테미너 회복 처리
                break;
            case ItemType.HealthRecovery:
                // 체력 회복 처리
                break;
            case ItemType.ArmorHead:
            case ItemType.ArmorBody:
            case ItemType.ArmorArm:
            case ItemType.ArmorLeg:
            case ItemType.ArmorHand:
            case ItemType.ArmorFeet:
                // 방어구 장착 처리
                break;
            case ItemType.Weapon:
                // 무기 장착 처리
                break;
        }
    }
}
