using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public enum ItemPopupContext
{
    Inventory,
    CharacterInfo,
    Discovery
}

public class ItemPopupUI : MonoBehaviour
{
    public GameObject XButton;

    [Header("아이템 프리팹이 생성될 부모")]
    public Transform ItemPopupParent;

    [Header("아이템 팝업 버튼 3개")]
    public GameObject GetInstallationReleaseButton; // 실제 버튼 오브젝트 (활성화/비활성화용)
    public Button GetInstallationRelease; // 버튼 컴포넌트
    public TextMeshProUGUI GetInstallationReleaseText;

    public Button Making;
    public TextMeshProUGUI MakingText;

    public Button ThrowAway;
    public TextMeshProUGUI ThrowAwayText; // ThrowAwayText로 가정
    
    public TextMeshProUGUI TopText; // 상단 제목 텍스트

    private ItemDataSO _currentItem;
    private ItemPopupContext _currentContext;
    private Button _linkedButton; // 인벤토리 슬롯과 연결된 버튼 (아이템 제거용)
    public void Setup(ItemDataSO item, ItemPopupContext context, Button clickedButton = null)
{
   /* Debug.Log($"[ItemPopupUI] Setup 시작");
    Debug.Log($"[ItemPopupUI] 아이템: {item?.itemName}");
    Debug.Log($"[ItemPopupUI] 컨텍스트: {context}");
    Debug.Log($"[ItemPopupUI] 전달받은 버튼: {(clickedButton != null ? clickedButton.name : "null")}");
    */
    /*if (clickedButton != null)
    {
        Debug.Log($"[ItemPopupUI] 전달받은 버튼 ID: {clickedButton.GetInstanceID()}");
        Debug.Log($"[ItemPopupUI] 전달받은 버튼 자식 개수: {clickedButton.transform.childCount}");
    }*/

    _currentItem = item;
    _currentContext = context;
    _linkedButton = clickedButton; // 넘겨받은 버튼 저장

   // Debug.Log($"[ItemPopupUI] _linkedButton 저장 완료: {(_linkedButton != null ? _linkedButton.name : "null")}");

    // 기존 팝업 자식 오브젝트 제거 (이전에 열려있던 팝업 내용을 비움)
    ClearItemPopUpChild();

    // 상단 제목 텍스트 설정
    switch (context)
    {
        case ItemPopupContext.Inventory:
        case ItemPopupContext.CharacterInfo:
            TopText.text = "아이템 정보";
            break;
        case ItemPopupContext.Discovery:
            TopText.text = "탐색 성공";
            break;
    }

    // 아이템 프리팹이 있을 경우 생성 (아이템 정보를 보여주는 3D 모델 등)
    if (item.ItemPopupPrefab != null)
    {
        GameObject popupObj = Instantiate(item.ItemPopupPrefab, ItemPopupParent);
        popupObj.transform.localPosition = Vector3.zero;
        popupObj.transform.localRotation = Quaternion.identity; // 회전 추가
        popupObj.transform.localScale = Vector3.one;
        popupObj.SetActive(true);
       // Debug.Log($"[ItemPopupUI] 아이템 프리팹 생성: {popupObj.name}");
    }

    // '장착/해제/사용/획득' 버튼 활성화 여부
    GetInstallationReleaseButton.SetActive(!_currentItem.IsMaterial); // IsMaterial이 true면 제작 재료이므로 장착/사용/획득 불가

    // 버튼 설정 (텍스트 및 클릭 이벤트)
    switch (context)
    {
        case ItemPopupContext.Inventory://가방 내부부
            if (_currentItem.CanEquip)
            {
                // 장착 가능한 아이템 (무기 또는 방어구)
                SetButton(GetInstallationRelease, GetInstallationReleaseText, "장착", OnEquipButtonClicked);
                //Debug.Log("[ItemPopupUI] 장착 버튼 설정 완료");
            }
            else
            {
                // 장착 불가능한 소모성 아이템 등 (물약 등)
                SetButton(GetInstallationRelease, GetInstallationReleaseText, "사용", OnUseClicked);
                //Debug.Log("[ItemPopupUI] 사용 버튼 설정 완료");
            }
            SetButton(Making, MakingText, "제작", OnCraftClicked);
            SetButton(ThrowAway, ThrowAwayText, "버리기", OnDiscardClicked);
            break;

        case ItemPopupContext.CharacterInfo://아이템 장착 중중
            SetButton(GetInstallationRelease, GetInstallationReleaseText, "해제", OnUnequipClicked);
            SetButton(Making, MakingText, "제작", OnCraftClicked);
            SetButton(ThrowAway, ThrowAwayText, "버리기", CharacterInfoOnDiscardClicked);
            break;

        case ItemPopupContext.Discovery://아이템 조사 성공공
            SetButton(GetInstallationRelease, GetInstallationReleaseText, "획득", OnAcquireClicked);
            SetButton(Making, MakingText, "제작", OnCraftClicked);
            SetButton(ThrowAway, ThrowAwayText, "버리기", OnDiscardClicked);
            break;
    }

   // Debug.Log("[ItemPopupUI] Setup 완료");
}

    private void SetButton(Button btn, TMP_Text btnText, string text, Action onClickAction)
    {
        // SFX_Manager.Instance.ButtonSFX(); // 사운드 이펙트 호출 (주석 처리됨)

        btnText.text = text;
        btn.onClick.RemoveAllListeners(); // 기존 리스너 제거
        btn.onClick.AddListener(() =>
        {
           // Debug.Log($"버튼 '{text}' 클릭됨 (버튼 이름: {btn.name})");
            onClickAction(); // 할당된 액션 실행
            // 팝업을 닫는 로직은 각 액션 메서드에서 직접 호출 (OnXButtonClicked())
        });
        btn.gameObject.SetActive(true); // 버튼 활성화
    }

    public void OnEquipButtonClicked()
{
    //Debug.Log($"[ItemPopupUI] OnEquipButtonClicked 시작 - 아이템: {_currentItem?.itemName}");
    //Debug.Log($"[ItemPopupUI] _linkedButton: {(_linkedButton != null ? _linkedButton.name : "null")}");
    
    if (_currentItem == null) 
    {
        Debug.LogWarning("[ItemPopupUI] _currentItem이 null입니다.");
        //return;
    }

    // 아이템 타입에 따라 적절한 장비 매니저에 장착 요청 위임
    if (_currentItem.ItemType == ItemType.Weapon)
    {
        if (WeaponEquipmentManager.Instance != null)
        {
           // Debug.Log($"[ItemPopupUI] 무기 장착 요청: {_currentItem.itemName}");
            
            // ⭐ 실제 장착 메서드 호출 ⭐
            WeaponEquipmentManager.Instance.EquipWeapon(_currentItem, _linkedButton);
            //Debug.Log($"[ItemPopupUI] 무기 장착 요청 완료");
        }
        else
        {
            Debug.LogWarning("[ItemPopupUI] WeaponEquipmentManager.Instance가 없습니다.");
        }
    }
    else if (DefenseEquipmentManager.Instance != null && DefenseEquipmentManager.Instance.IsArmorType(_currentItem.ItemType))
    {
        DefenseEquipmentManager.Instance.EquipArmor(_currentItem, _linkedButton);
        //Debug.Log($"[ItemPopupUI] {_currentItem.ItemType} 방어구 장착 요청: {_currentItem.itemName}");
    }
    else
    {
        Debug.Log($"[ItemPopupUI] 해당 아이템은 장착할 수 없습니다: {_currentItem.itemName}");
    }

    OnXButtonClicked(); // 장착 버튼 클릭 후, 아이템 팝업을 닫음
}


    private void OnUseClicked()
    {
        //Debug.Log($"[ItemPopupUI] 사용 {_currentItem.itemName}");

        // 아이템 사용 로직 (CharacterManager 또는 별도의 ItemUseManager에서 처리)
        // CharacterManager의 메서드를 호출하여 체력/스태미나 회복 로직을 실행하도록 변경
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.UseConsumableItem(_currentItem); 
        }
        else
        {
            Debug.LogWarning("[ItemPopupUI] CharacterManager.Instance가 없습니다.");
        }
        
        // 소비된 아이템은 인벤토리에서 제거 (BagManager가 처리하는 것이 이상적)
        // 일단은 BagItemReference를 통해 직접 제거
        // 이 로직은 InventoryUI 또는 BagManager로 옮겨야 합니다.
        if (_linkedButton != null && _linkedButton.transform.childCount > 0)
        {
            Destroy(_linkedButton.transform.GetChild(0).gameObject); 
        }

        OnXButtonClicked(); // 팝업 닫기
    }

    



    //원래 있던 거

    public void OnUnequipClicked()
    {
       // Debug.Log($"[ItemPopupUI] 해제 {_currentItem.itemName}");

        // 아이템 타입에 따라 적절한 장비 매니저에 해제 요청 위임
        if (_currentItem.ItemType == ItemType.Weapon)
        {
            if (WeaponEquipmentManager.Instance != null)
            {
                // WeaponEquipmentManager.UnequipWeapon은 현재 인수를 받지 않습니다.
                // 만약 특정 아이템을 해제하고 싶다면 WeaponEquipmentManager의 로직 수정 필요.
                // 현재는 단순히 장착된 무기를 "해제"하는 역할.
                WeaponEquipmentManager.Instance.UnequipWeapon(); 
                //Debug.Log($"[ItemPopupUI] 무기 해제 요청: {_currentItem.itemName}");
            }
            else
            {
                Debug.LogWarning("[ItemPopupUI] WeaponEquipmentManager.Instance가 없습니다.");
            }
        }
        else if (DefenseEquipmentManager.Instance != null && DefenseEquipmentManager.Instance.IsArmorType(_currentItem.ItemType))
        {
            // 방어구 해제 요청을 DefenseEquipmentManager에게 위임
            // DefenseEquipmentManager.UnequipArmor는 현재 _currentItem을 받도록 되어 있음.
            DefenseEquipmentManager.Instance.UnequipArmor(_currentItem);
            //Debug.Log($"[ItemPopupUI] {_currentItem.ItemType} 방어구 해제 요청: {_currentItem.itemName}");
        }
        else
        {
            Debug.Log($"[ItemPopupUI] 해당 아이템은 해제할 수 없습니다: {_currentItem.itemName}");
        }

        OnXButtonClicked(); // 팝업 닫기
    }

    private void OnAcquireClicked()
    {
       // Debug.Log($"[ItemPopupUI] 획득 {_currentItem.itemName}");

        // 아이템 획득 로직을 Bag 매니저에 위임
        if (Bag.Instance != null)
        {
            Bag.Instance.AddItemToBag(_currentItem); // Bag 스크립트에 아이템 추가 요청 메서드 필요
        }
        else
        {
            Debug.LogWarning("[ItemPopupUI][획득] Bag.Instance가 null입니다. 아이템을 가방에 추가할 수 없습니다.");
        }

        // 지역에서 아이템 수량 감소 (PlaceItemManager에 위임)
        if (PlaceItemManager.Instance != null && PlaceItemManager.Instance.CurrentRegion != null)
        {
            PlaceItemManager.Instance.CurrentRegion.DecreaseItemCount(_currentItem); 
        }
        else
        {
            Debug.LogWarning("[ItemPopupUI][획득] PlaceItemManager.Instance.CurrentRegion이 null입니다. 수량 감소 실패");
        }

        // 탐색 관련 UI 상태 업데이트 (ItemSearchManager에 위임)
        if (ItemSearchManager.Instance != null)
        {
            ItemSearchManager.Instance.IsSearching = false;
            ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
            //ItemSearchManager.Instance.FinishSearchProcess(); // 탐색 종료 및 버튼 활성화 로직
        }

        OnXButtonClicked(); // 팝업 닫기
    }

    private void OnCraftClicked()
    {
        //Debug.Log($"[ItemPopupUI] 제작 {_currentItem.itemName}");

        // 제작 로직은 CraftingManager와 같은 별도의 매니저에서 처리
        // 예: CraftingManager.Instance.CraftItem(_currentItem);

        // 탐색 관련 UI 상태 업데이트 (ItemSearchManager에 위임)
        if (ItemSearchManager.Instance != null)
        {
            ItemSearchManager.Instance.IsSearching = false;
            ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
            //ItemSearchManager.Instance.FinishSearchProcess(); // 탐색 종료 및 버튼 활성화 로직
        }

        OnXButtonClicked(); // 팝업 닫기
    }

    public void OnDiscardClicked()
    {
       // Debug.Log($"[ItemPopupUI] 버리기 {_currentItem.itemName}");

        // 아이템 버리기 로직 (BagManager 또는 PlaceItemManager에 위임)
        // Bag에서 아이템을 제거하는 로직이 필요
        // 예: Bag.Instance.RemoveItemFromBag(_currentItem);

        // 지역에 아이템 수량 증가 (버린 것이므로) (PlaceItemManager에 위임)
        if (PlaceItemManager.Instance != null && PlaceItemManager.Instance.CurrentRegion != null)
        {
            PlaceItemManager.Instance.CurrentRegion.IncreaseItemCount(_currentItem); 
        }
        else
        {
            Debug.LogWarning("[ItemPopupUI][버리기] PlaceItemManager.Instance.CurrentRegion이 null입니다.");
        }

        // 인벤토리 UI에서 아이템 제거
        // 이 로직은 InventoryUI 또는 BagManager로 옮겨야 합니다.
        if (_linkedButton != null && _linkedButton.transform.childCount > 0)
        {
            Destroy(_linkedButton.transform.GetChild(0).gameObject); 
        }
        
        // 탐색 관련 UI 상태 업데이트 (ItemSearchManager에 위임)
        if (ItemSearchManager.Instance != null)
        {
            ItemSearchManager.Instance.IsSearching = false;
            ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
            //ItemSearchManager.Instance.FinishSearchProcess(); // 탐색 종료 및 버튼 활성화 로직
        }

        OnXButtonClicked(); // 팝업 닫기
    }

    //새로 추가 - 캐릭터 정보에서 버리기
     public void CharacterInfoOnDiscardClicked()
    {
        
        //Debug.Log("[ItemPopupUI] 아이템 정보에서 버리기");
    switch (_currentItem.ItemType)
    {
        case ItemType.Weapon:
            WeaponEquipmentManager.Instance.ThrowAwayWeaponFormCharacterInfo();
             Debug.LogWarning("****무기 버리기****");
            break;

        case ItemType.ArmorHead:
        case ItemType.ArmorBody:
        case ItemType.ArmorArm:
        case ItemType.ArmorLeg:
        case ItemType.ArmorHand:
        case ItemType.ArmorFeet:
            DefenseEquipmentManager.Instance.ThrowAwayDefenseFormCharacterInfo(_currentItem.ItemType);
            //Debug.LogWarning($"****방어구 버리기({_currentItem.itemName})****");
            break;
    }


       OnXButtonClicked(); // 팝업 닫기
    }

    public void OnXButtonClicked()
    {
        // SFX_Manager.Instance.ButtonSFX(); // 사운드 이펙트
        
        // 탐색 관련 UI 상태 업데이트 (ItemSearchManager에 위임)
        if (ItemSearchManager.Instance != null)
        {
            ItemSearchManager.Instance.IsSearching = false;
            ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
            //ItemSearchManager.Instance.FinishSearchProcess(); // 탐색 종료 및 버튼 활성화 로직
        }

        if(CharacterInfoUI.Instance.CharacterItemInfoPanel.activeSelf == true)
        {
            CharacterInfoUI.Instance.CharacterItemInfoPanel.SetActive(false);
        }

        Destroy(gameObject); // 이 팝업 오브젝트 자체를 파괴
    }

    private void ClearItemPopUpChild()
    {
        // ItemPopupParent의 자식들 (아이템 3D 모델 등) 제거
        if (ItemPopupParent != null && ItemPopupParent.childCount > 0)
        {
            foreach (Transform child in ItemPopupParent)
            {
                Destroy(child.gameObject);
            }
        }
        

    }
}