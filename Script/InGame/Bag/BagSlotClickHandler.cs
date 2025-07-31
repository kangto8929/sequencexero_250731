using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class BagSlotClickHandler : MonoBehaviour
{
    public GameObject PopupInstance;

    public void OnBagSlotClicked(Button button)
    {
        Debug.Log($"[BagSlotClickHandler] OnBagSlotClicked 시작");
        Debug.Log($"[BagSlotClickHandler] 클릭된 버튼: {button.name}");
        Debug.Log($"[BagSlotClickHandler] 버튼 오브젝트 ID: {button.GetInstanceID()}");
        Debug.Log($"[BagSlotClickHandler] 버튼 Transform: {button.transform.name}");
        
        // Bag.Instance의 BagSlots에서 몇 번째 슬롯인지 확인
        if (Bag.Instance != null && Bag.Instance.BagSlots != null)
        {
            for (int i = 0; i < Bag.Instance.BagSlots.Length; i++)
            {
                if (Bag.Instance.BagSlots[i] == button.transform)
                {
                    Debug.Log($"[BagSlotClickHandler] 클릭된 슬롯 인덱스: {i}");
                    break;
                }
            }
        }

        //SFX_Manager.Instance.ButtonSFX();

        Transform parent = ItemSearchManager.Instance.DiscoveryParent;

        // 버튼에 아이템이 없으면 리턴
        if (button.transform.childCount == 0) 
        {
            Debug.LogWarning($"[BagSlotClickHandler] 버튼 '{button.name}'에 자식이 없어서 리턴");
            return;
        }

        Debug.Log($"[BagSlotClickHandler] 버튼 '{button.name}'의 자식 개수: {button.transform.childCount}");

        Transform child = button.transform.GetChild(0);
        Debug.Log($"[BagSlotClickHandler] 첫 번째 자식: {child.name}");
        
        BagItemReference reference = child.GetComponent<BagItemReference>();
        if (reference == null || reference.ItemData == null) 
        {
            Debug.LogWarning($"[BagSlotClickHandler] BagItemReference가 null이거나 ItemData가 null입니다.");
            return;
        }

        Debug.Log($"[BagSlotClickHandler] 아이템 데이터: {reference.ItemData.itemName}");

        var itemSearchManager = ItemSearchManager.Instance;
        if (itemSearchManager == null || itemSearchManager.ItemPopupPrefab == null || itemSearchManager.DiscoveryParent == null)
        {
            Debug.LogWarning("ItemSearchManager 설정이 누락되었습니다.");
            return;
        }

        // ✅ PlaceSelectionHandler 자식이 존재하는지 체크 (탐색 팝업과 충돌 방지)
        foreach (Transform t in itemSearchManager.DiscoveryParent)
        {
            if (t.GetComponentInChildren<PlaceSelectionHandler>() != null)
            {
                Debug.Log("PlaceSelectionHandler에서 만든 오브젝트가 이미 존재하므로 팝업을 생성하지 않습니다.");
                return;
            }
        }

        // ✅ 자식이 있을 경우 처리
        if (parent.childCount > 0)
        {
            Transform firstChild = parent.GetChild(0);
            ItemPopupUI popupUI = firstChild.GetComponent<ItemPopupUI>();

            if (popupUI != null && popupUI.XButton != null)
            {
                if (!popupUI.XButton.activeSelf)
                {
                    Debug.LogWarning("X버튼이 비활성화된 상태로 이미 자식이 존재하므로 프리팹을 생성할 수 없습니다.");
                    return;
                }

                // X버튼이 활성화 되어 있다면 기존 자식 삭제
                Debug.Log("[BagSlotClickHandler] 기존 팝업 삭제");
                Destroy(firstChild.gameObject);
            }
            else
            {
                Debug.LogWarning("0번째 자식에 ItemPopupUI 또는 XButton이 없습니다.");
                return;
            }
        }

        // ✅ 팝업 생성
        Debug.Log("[BagSlotClickHandler] 새 팝업 생성 시작");
        PopupInstance = Instantiate(itemSearchManager.ItemPopupPrefab, itemSearchManager.DiscoveryParent);
        PopupInstance.SetActive(true);
        Debug.Log("팝업 생성");

        // ✅ X 버튼 활성화
        ItemPopupUI itemPopupUI = PopupInstance.GetComponent<ItemPopupUI>();
        if (itemPopupUI != null && itemPopupUI.XButton != null)
        {
            itemPopupUI.XButton.SetActive(true);
            Debug.Log($"[BagSlotClickHandler] Setup 호출 - 버튼 전달: {button.name} (ID: {button.GetInstanceID()})");
            itemPopupUI.Setup(reference.ItemData, ItemPopupContext.Inventory, button);
        }
        else
        {
            Debug.LogWarning("ItemPopupUI 또는 XButton이 없습니다.");
        }

        // ✅ 탐색 버튼 비활성화
        itemSearchManager.IsSearching = false;
        itemSearchManager.SearchButton.gameObject.SetActive(false);

        // ✅ 팝업 설정 (중복 Setup 호출 - 이 부분은 위에서 이미 했으므로 제거하거나 수정 필요)
        if (itemPopupUI != null)
        {
            // 이미 위에서 Setup을 호출했으므로 중복 호출을 피하거나, 
            // 위의 Setup 호출을 제거하고 여기서만 호출하도록 수정
            // itemPopupUI.Setup(reference.ItemData, ItemPopupContext.Inventory, button); // 중복이므로 주석 처리

            if (itemPopupUI.GetInstallationReleaseButton != null)
            {
                itemPopupUI.GetInstallationReleaseButton.SetActive(!reference.ItemData.IsMaterial);
            }
            else
            {
                Debug.LogWarning("GetInstallationReleaseButton이 할당되어 있지 않습니다.");
            }
        }
        else
        {
            Debug.LogWarning("팝업에 ItemPopupUI 컴포넌트가 없습니다.");
        }

        Debug.Log("[BagSlotClickHandler] OnBagSlotClicked 완료");
    }
}

/*using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class BagSlotClickHandler : MonoBehaviour
{
    public GameObject PopupInstance;

    public void OnBagSlotClicked(Button button)
    {
        //SFX_Manager.Instance.ButtonSFX();

        Transform parent = ItemSearchManager.Instance.DiscoveryParent;

        // 버튼에 아이템이 없으면 리턴
        if (button.transform.childCount == 0) return;

        Transform child = button.transform.GetChild(0);
        BagItemReference reference = child.GetComponent<BagItemReference>();
        if (reference == null || reference.ItemData == null) return;

        var itemSearchManager = ItemSearchManager.Instance;
        if (itemSearchManager == null || itemSearchManager.ItemPopupPrefab == null || itemSearchManager.DiscoveryParent == null)
        {
            Debug.LogWarning("ItemSearchManager 설정이 누락되었습니다.");
            return;
        }

        // ✅ PlaceSelectionHandler 자식이 존재하는지 체크 (탐색 팝업과 충돌 방지)
        foreach (Transform t in itemSearchManager.DiscoveryParent)
        {
            if (t.GetComponentInChildren<PlaceSelectionHandler>() != null)
            {
                Debug.Log("PlaceSelectionHandler에서 만든 오브젝트가 이미 존재하므로 팝업을 생성하지 않습니다.");
                return;
            }
        }

        // ✅ 자식이 있을 경우 처리
        if (parent.childCount > 0)
        {
            Transform firstChild = parent.GetChild(0);
            ItemPopupUI popupUI = firstChild.GetComponent<ItemPopupUI>();

            if (popupUI != null && popupUI.XButton != null)
            {
                if (!popupUI.XButton.activeSelf)
                {
                    Debug.LogWarning("X버튼이 비활성화된 상태로 이미 자식이 존재하므로 프리팹을 생성할 수 없습니다.");
                    return;
                }

                // X버튼이 활성화 되어 있다면 기존 자식 삭제
                Destroy(firstChild.gameObject);
            }
            else
            {
                Debug.LogWarning("0번째 자식에 ItemPopupUI 또는 XButton이 없습니다.");
                return;
            }
        }

        // ✅ 팝업 생성
        PopupInstance = Instantiate(itemSearchManager.ItemPopupPrefab, itemSearchManager.DiscoveryParent);
        PopupInstance.SetActive(true);
        Debug.Log("팝업 생성");

        // ✅ X 버튼 활성화
        ItemPopupUI itemPopupUI = PopupInstance.GetComponent<ItemPopupUI>();
        if (itemPopupUI != null && itemPopupUI.XButton != null)
        {
            itemPopupUI.XButton.SetActive(true);
            itemPopupUI.Setup(reference.ItemData, ItemPopupContext.Inventory, button);
        }
        else
        {
            Debug.LogWarning("ItemPopupUI 또는 XButton이 없습니다.");
        }

        // ✅ 탐색 버튼 비활성화
        itemSearchManager.IsSearching = false;
        itemSearchManager.SearchButton.gameObject.SetActive(false);

        // ✅ 팝업 설정
        if (itemPopupUI != null)
        {
            itemPopupUI.Setup(reference.ItemData, ItemPopupContext.Inventory);

            if (itemPopupUI.GetInstallationReleaseButton != null)
            {
                itemPopupUI.GetInstallationReleaseButton.SetActive(!reference.ItemData.IsMaterial);
            }
            else
            {
                Debug.LogWarning("GetInstallationReleaseButton이 할당되어 있지 않습니다.");
            }
        }
        else
        {
            Debug.LogWarning("팝업에 ItemPopupUI 컴포넌트가 없습니다.");
        }
    }
}
*/