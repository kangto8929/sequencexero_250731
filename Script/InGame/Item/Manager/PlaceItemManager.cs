using System.Collections.Generic;
using UnityEngine;

public class PlaceItemManager : MonoBehaviour
{
    public static PlaceItemManager Instance;

    public List<PlaceItemRegion> AllRegions = new();
    public PlaceItemRegion CurrentRegion;

    void Awake()
    {
        Instance = this;

        // 자동으로 지역들을 수집할 수 있음 (또는 수동 등록)
        // allRegions.AddRange(FindObjectsOfType<PlaceItemRegion>());
    }

    // 장소 이동 시 호출하여 현재 지역 업데이트
    public void SetCurrentRegion(PlaceNameType placeNameType)
    {
        CurrentRegion = AllRegions.Find(region => region.RegionType == placeNameType);

/*if (CurrentRegion == null)
{
    Debug.LogWarning($"[PlaceItemManager] {placeNameType} 지역을 찾을 수 없습니다.");
}
else
{
    Debug.Log($"[PlaceItemManager] 현재 지역 설정됨: {CurrentRegion.RegionType}");
}*/
    }

    // 특정 아이템이 있는 지역 목록 찾기
    // 변경 후
public List<PlaceNameType> FindRegionsWithItem(ItemDataSO targetItem)
{
    List<PlaceNameType> matchingRegions = new();

    foreach (var region in AllRegions)
    {
        foreach (var item in region.searchableItems)
        {
            if (item.Item == targetItem)
            {
                matchingRegions.Add(region.RegionType);
                break;
            }
        }
    }

    return matchingRegions;
}


    // 현재 장소에 존재하는 아이템 리스트 가져오기
    public List<PlaceHaveItem> GetCurrentRegionItems()
{
    if (CurrentRegion == null)
    {
        //Debug.LogWarning("[PlaceItemManager] 현재 지역이 설정되지 않았습니다.");
        return new List<PlaceHaveItem>();
    }

    return CurrentRegion.searchableItems;
}

//
 // 현재 장소명을 받아서 CurrentRegion 세팅하는 메서드 예시
    public void SetCurrentRegionByName(string placeName)
{
    PlaceItemRegion[] allRegions = FindObjectsOfType<PlaceItemRegion>();
    foreach (var region in allRegions)
    {
        if (region.RegionType.ToString() == placeName)
        {
            CurrentRegion = region;
            //Debug.Log($"CurrentRegion이 {placeName}으로 설정됨");
            return;
        }
    }
    //Debug.LogWarning($"장소명 '{placeName}'과 일치하는 PlaceItemRegion을 찾지 못함");
}

//추가 이거 없애나 마나??
// 현재 장소에 아이템 추가 (버리기)
public void AddItemToCurrentRegion(ItemDataSO item, int amount)
{
    if (CurrentRegion == null)
    {
        Debug.LogWarning("[PlaceItemManager] 현재 지역이 설정되지 않아 아이템 추가 실패");
        return;
    }

    var placeItems = CurrentRegion.GetComponent<PlaceHaveItems>();
    if (placeItems != null)
    {
        placeItems.IncreaseItemQuantity(item, amount);
       // Debug.Log($"[PlaceItemManager] {CurrentRegion.RegionType}에 {item.itemName} {amount}개 추가됨");
    }
    /*else
    {
        Debug.LogWarning("[PlaceItemManager] CurrentRegion에 PlaceHaveItems 컴포넌트가 없음");
    }*/
}

// 현재 장소에서 아이템 감소 (획득 시)
public void RemoveItemFromCurrentRegion(ItemDataSO item, int amount)
{
    if (CurrentRegion == null)
    {
        //Debug.LogWarning("[PlaceItemManager] 현재 지역이 설정되지 않아 아이템 제거 실패");
        return;
    }

    var placeItems = CurrentRegion.GetComponent<PlaceHaveItems>();
    if (placeItems != null)
    {
        placeItems.DecreaseItemQuantity(item, amount);
        //Debug.Log($"[PlaceItemManager] {CurrentRegion.RegionType}에서 {item.itemName} {amount}개 감소됨");
    }
    /*else
    {
        Debug.LogWarning("[PlaceItemManager] CurrentRegion에 PlaceHaveItems 컴포넌트가 없음");
    }*/
}


}

