using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class PlaceItemRegion : MonoBehaviour
{
    public PlaceNameType RegionType; // enum으로 직접 관리
    public List<PlaceHaveItem> searchableItems = new();

    /*public ItemDataSO GetRandomAvailableItem()
    {
        var available = searchableItems.FindAll(i => i.Quantity > 0);
        if (available.Count == 0) return null;

        var selected = available[UnityEngine.Random.Range(0, available.Count)];
        selected.Quantity--;
        return selected.Item;
    }*/
   /* public ItemDataSO GetRandomAvailableItem()
{
    var available = searchableItems.FindAll(i => i.Quantity > 0);
    if (available.Count == 0) return null;

    var selected = available[UnityEngine.Random.Range(0, available.Count)];
    selected.Quantity--;

    // 장소별 실제 개수도 줄이기
    var placeHave = GetComponent<PlaceHaveItems>();
    placeHave?.DecreaseItemQuantity(selected.Item, 1); // PlaceHaveItems에도 반영

    return selected.Item;
}*/
public ItemDataSO GetRandomAvailableItem()
{
    var available = searchableItems.FindAll(i => i.Quantity > 0);
    if (available.Count == 0) return null;

    var selected = available[UnityEngine.Random.Range(0, available.Count)];

    // 장소별 실제 개수 줄이기 (한 곳에서만 감소 처리)
    var placeHave = GetComponent<PlaceHaveItems>();
    placeHave?.DecreaseItemQuantity(selected.Item, 1);

    return selected.Item;
}



    public Dictionary<string, int> GetCurrentItemStatus()
    {
        Dictionary<string, int> status = new();
        foreach (var i in searchableItems)
        {
            status[i.Item.itemName] = i.Quantity;
        }
        return status;
    }

    //아이템 카운트 줄이기
public void DecreaseItemCount(ItemDataSO item)
{
    var target = searchableItems.Find(i => i.Item.itemName == item.itemName); // 이름 비교 사용
    if (target != null && target.Quantity > 0)
    {
        target.Quantity--;
        Debug.Log($"[감소 성공] '{item.itemName}' 수량 -1 → 남은 수량: {target.Quantity}");
    }
    else
    {
        Debug.LogWarning($"[감소 실패] '{item.itemName}' 수량이 없거나 찾을 수 없음!");
    }
}

//장소 아이템 수량 목록 - 잘 작동중
public void DebugPrintAllItemCounts()
    {
        Debug.Log($"[디버그] {RegionType} 지역의 아이템 수량 목록:");
        foreach (var i in searchableItems)
        {
            Debug.Log($"- {i.Item.itemName} : {i.Quantity}");
        }
    }

//장소 아이템 수량 - 탐정 스킬
public string GetFormattedItemList()
{
    if (searchableItems == null || searchableItems.Count == 0)
        return "아이템이 존재하지 않습니다.";

    System.Text.StringBuilder sb = new();
    foreach (var i in searchableItems)
    {
        string color = (i.Quantity == 0) ? "#888888" : "#FFFFFF";
        sb.AppendLine($"<color={color}>· {i.Item.itemName}: {i.Quantity}개</color>");
    }
    return sb.ToString();
}

// PlaceItemRegion.cs

public void IncreaseItemCount(ItemDataSO item, int amount = 1)
{
    var target = searchableItems.Find(i => i.Item.itemName == item.itemName);
    if (target != null)
    {
        target.Quantity += amount;
        Debug.Log($"[증가 성공] '{item.itemName}' 수량 +{amount} → 현재 수량: {target.Quantity}");
    }
    else
    {
        // 무조건 추가 (지역 제한 없이)
        var newPlaceItem = new PlaceHaveItem
        {
            Item = item,
            Quantity = amount
        };
        searchableItems.Add(newPlaceItem);
        Debug.Log($"[신규 추가] '{item.itemName}' 아이템 추가 및 수량 설정: {amount}");
    }
}

/*public void IncreaseItemCount(ItemDataSO item, int amount = 1)
{
    var target = searchableItems.Find(i => i.Item.itemName == item.itemName);
    if (target != null)
    {
        // 이미 존재하는 아이템이면 수량 증가
        target.Quantity += amount;
        Debug.Log($"[증가 성공] '{item.itemName}' 수량 +{amount} → 현재 수량: {target.Quantity}");
    }
    else
    {
        // 없는 아이템이면 새로 추가
        var newPlaceItem = new PlaceHaveItem
        {
            Item = item,
            Quantity = amount
        };

        // 타입에 따른 분기 예시 (원하는 조건에 맞게 바꾸세요)
        if (RegionType == MovePlaceManager.Instance.CurrentPlaceNameType) 
        {
            // 특정 타입일 때만 추가할 수도 있음
            searchableItems.Add(newPlaceItem);
            Debug.Log($"[신규 추가] '{item.itemName}' 아이템 추가 및 수량 설정: {amount}");
        }
        else
        {
            // 타입 조건에 안 맞으면 추가 안 함 또는 다른 처리
            Debug.LogWarning($"[추가 실패] '{item.itemName}' 아이템은 이 지역({RegionType})에 추가할 수 없습니다.");
        }
    }
}*/



}


