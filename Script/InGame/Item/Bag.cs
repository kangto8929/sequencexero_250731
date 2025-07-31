
using UnityEngine;
using System.Linq; // FindEmptyBagSlot에서 Count()를 사용하기 위해 추가
using System.Collections.Generic; // Dictionary를 사용할 경우를 대비해 추가 (현재는 사용 안함)

public class Bag : MonoBehaviour
{
    public static Bag Instance;   // 싱글톤으로 접근
    public Transform[] BagSlots = new Transform[9]; // 9개의 장착 슬롯

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 가방 UI도 씬 전환 시 유지될 수 있도록 할 수 있습니다.
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool AddItemToBag(ItemDataSO itemData)
    {
        if (itemData == null || itemData.BagItemPrefab == null)
        {
            Debug.LogWarning("[Bag] 가방에 추가할 아이템 데이터 또는 BagItemPrefab이 누락되었습니다.");
            return false;
        }

        Transform emptySlot = FindEmptyBagSlot();

        if (emptySlot != null)
        {
            GameObject bagItemUI = Instantiate(itemData.BagItemPrefab, emptySlot);
            bagItemUI.transform.localPosition = Vector3.zero;
            bagItemUI.transform.localRotation = Quaternion.identity;
            bagItemUI.transform.localScale = Vector3.one;

            // BagItemReference 컴포넌트에 ItemData를 설정합니다.
            // BagItemPrefab에는 BagItemReference 컴포넌트가 붙어있어야 합니다.
            BagItemReference reference = bagItemUI.GetComponent<BagItemReference>();
            if (reference != null)
            {
                reference.ItemData = itemData;
                Debug.Log($"[Bag] '{itemData.itemName}' 아이템이 가방에 추가되었습니다. (슬롯: {System.Array.IndexOf(BagSlots, emptySlot)})");
                return true;
            }
            else
            {
                Debug.LogError($"[Bag] {itemData.BagItemPrefab.name}에 BagItemReference 컴포넌트가 없습니다. 아이템 데이터를 연결할 수 없습니다.");
                Destroy(bagItemUI); // 연결 실패 시 생성된 UI 오브젝트 파괴
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"[Bag] '{itemData.itemName}'을(를) 추가할 빈 가방 슬롯이 없습니다.");
            // 가방이 꽉 찼을 경우 추가 UI 피드백 (예: "가방이 꽉 찼습니다!")
            return false;
        }
    }

    /// <summary>
    /// 가방에서 특정 ItemDataSO에 해당하는 아이템을 제거합니다.
    /// UI에서 해당 아이템 아이콘을 파괴합니다.
    /// </summary>
    /// <param name="itemDataToRemove">제거할 아이템의 ItemDataSO.</param>
    /// <returns>아이템이 성공적으로 제거되었으면 true, 찾지 못했으면 false를 반환합니다.</returns>
    public bool RemoveItemFromBag(ItemDataSO itemDataToRemove)
    {
        if (itemDataToRemove == null)
        {
            Debug.LogWarning("[Bag] 제거할 아이템 데이터가 null입니다.");
            return false;
        }

        foreach (Transform slot in BagSlots)
        {
            if (slot.childCount > 0)
            {
                BagItemReference reference = slot.GetChild(0).GetComponent<BagItemReference>();
                if (reference != null && reference.ItemData == itemDataToRemove)
                {
                    Destroy(slot.GetChild(0).gameObject);
                    Debug.Log($"[Bag] '{itemDataToRemove.itemName}' 아이템이 가방에서 제거되었습니다.");
                    return true;
                }
            }
        }
        Debug.LogWarning($"[Bag] 가방에서 '{itemDataToRemove.itemName}' 아이템을 찾을 수 없습니다.");
        return false;
    }


    /// <summary>
    /// 가방에서 빈 슬롯을 찾아 반환합니다.
    /// </summary>
    /// <returns>빈 슬롯의 Transform, 없으면 null을 반환합니다.</returns>
    private Transform FindEmptyBagSlot()
    {
        foreach (Transform slot in BagSlots)
        {
            if (slot.childCount == 0) // 자식이 없으면 빈 슬롯
            {
                return slot;
            }
        }
        return null; // 빈 슬롯 없음
    }

    /// <summary>
    /// 가방에 특정 ItemDataSO를 가진 아이템이 있는지 확인합니다.
    /// </summary>
    public bool ContainsItem(ItemDataSO itemData)
    {
        foreach (Transform slot in BagSlots)
        {
            if (slot.childCount > 0)
            {
                BagItemReference reference = slot.GetChild(0).GetComponent<BagItemReference>();
                if (reference != null && reference.ItemData == itemData)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public int GetItemCount(ItemDataSO itemData)
    {
        int count = 0;
        foreach (Transform slot in BagSlots)
        {
            if (slot.childCount > 0)
            {
                BagItemReference reference = slot.GetChild(0).GetComponent<BagItemReference>();
                if (reference != null && reference.ItemData == itemData)
                {
                    count++;
                }
            }
        }
        return count;
    }
}
