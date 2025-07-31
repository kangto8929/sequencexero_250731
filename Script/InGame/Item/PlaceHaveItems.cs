using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
[System.Serializable]
public class PlaceHaveItem
{
    public ItemDataSO Item;
    public int Quantity;
}


public class PlaceHaveItems : MonoBehaviour
{
    public List<PlaceHaveItem> items = new List<PlaceHaveItem>();



    // 🟢 획득: 아이템 수량 감소 - 작동 중
   public void DecreaseItemQuantity(ItemDataSO item, int amount)
    {
        var placeHaveItem = items.Find(x => x.Item == item);

        if (placeHaveItem != null)
        {
            placeHaveItem.Quantity -= amount;
            if (placeHaveItem.Quantity < 0)
                placeHaveItem.Quantity = 0;
        }
        else
        {
            // 아이템이 이 장소에 없는데 줄이려고 하면, 경고
            Debug.LogWarning($"[감소 실패] '{item.itemName}' 수량이 없거나 찾을 수 없음!");
        }

        UpdateUI();
    }

    // PlaceHaveItems.cs에서 처리

public void IncreaseItemQuantity(ItemDataSO item, int amount)
{
    var placeHaveItem = items.Find(x => x.Item == item);

    if (placeHaveItem != null)
    {
        placeHaveItem.Quantity += amount;
    }
    else
    {
        PlaceHaveItem newItem = new PlaceHaveItem();
        newItem.Item = item;
        newItem.Quantity = amount;
        items.Add(newItem);
        Debug.Log($"[PlaceHaveItems] 신규 아이템 추가: {item.itemName} x{amount}");
    }

    UpdateUI();
}


    // ✅ 아이템 수량 증가 (예: 아이템 버리기)
    /*public void IncreaseItemQuantity(ItemDataSO item, int amount)
    {
        var placeHaveItem = items.Find(x => x.Item == item);

        if (placeHaveItem != null)
        {
            placeHaveItem.Quantity += amount;
        }
        else
        {
            PlaceHaveItem newItem = new PlaceHaveItem();
            newItem.Item = item;
            newItem.Quantity = amount;
            items.Add(newItem);
        }

        UpdateUI();
    }*/

    //현재 장소에 총 몇 개의 아이템이 있는지 알려주는 디버그 - 장소 이동하면 바로 출력됨
    private void UpdateUI()
    {
        // UI 갱신 로직이 있다면 여기에 작성하세요
        Debug.Log($"[UpdateUI] 아이템 수량 갱신됨. 총 {items.Count}개 아이템 보유");
    }
}
