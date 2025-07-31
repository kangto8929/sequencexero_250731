using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // Button 사용을 위해 추가
using System; // Enum.GetValues를 위해 추가

public class DefenseEquipmentManager : MonoBehaviour
{
    public static DefenseEquipmentManager Instance { get; private set; }

    // 현재 장착된 방어구 오브젝트들을 ItemType별로 관리 (캐릭터에 부착된 3D 프리팹 인스턴스)
    public Dictionary<ItemType, GameObject> EquippedArmorObjs { get; private set; } = new Dictionary<ItemType, GameObject>();

    // 현재 장착된 방어구의 ItemDataSO를 관리 (장착된 아이템의 데이터 자체)
    private Dictionary<ItemType, ItemDataSO> _equippedArmorData = new Dictionary<ItemType, ItemDataSO>();

    [Header("UI Armor Slot Parents (For BagItemPrefab)")]
    // 각 방어구 타입에 맞는 UI 아이콘(BagItemPrefab)이 인스턴스화될 부모 Transform을 여기에 할당합니다.
    // CharacterInfoUI의 각 ArmorSlotUI 내에 연결된 Transform을 여기에 할당합니다.
    // 실제 게임 UI에서 인벤토리 아이템을 드래그해서 넣는 슬롯의 Transform을 연결해야 합니다.
    public Transform HeadArmorSlotUIParent;
    public Transform BodyArmorSlotUIParent;
    public Transform ArmArmorSlotUIParent;
    public Transform LegArmorSlotUIParent;
    public Transform HandArmorSlotUIParent;
    public Transform FeetArmorSlotUIParent;

    // 아이템 타입에 따라 UI 부모를 빠르게 찾아줄 딕셔너리 (초기화 시 매핑)
    private Dictionary<ItemType, Transform> _armorUIParents = new Dictionary<ItemType, Transform>();

    // 각 방어구 슬롯에 현재 표시되는 BagItemPrefab 인스턴스 (UI 전용)
    private Dictionary<ItemType, GameObject> _currentUIArmorIcons = new Dictionary<ItemType, GameObject>();


    private ItemDataSO _currentEquippedArmorData;//현재 장착중인 방어구 데이터터

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // 씬이 전환되어도 장비 정보가 유지되어야 한다면 이 주석을 해제하세요.
        }
        else
        {
            Debug.LogWarning("[DefenseEquipmentManager] 중복된 DefenseEquipmentManager 인스턴스가 발견되어 파괴합니다.", this);
            Destroy(gameObject); // 중복 인스턴스 방지
        }
    }

    private void Start()
    {
        // CharacterInfoUI의 방어력 텍스트 초기화 요청 (이것이 가장 먼저 호출되어야 함)
        CharacterInfoUI.Instance?.InitializeDefenseTMPs();
        InitializeArmorDictionaries();
        InitializeUIParents();
        InitializeUIArmorIcons();

        // 총 방어력 업데이트 (초기화 후 다시 호출하여 최신 상태 반영)
        UpdateTotalDefenseUI(); 

        // 각 방어구 슬롯의 UI 아이콘 초기화 (빈 상태로)
        foreach(var type in _armorUIParents.Keys)
        {
            // UI 슬롯에 아이콘을 비우고 방어력 텍스트를 0으로 설정
            UpdateArmorSlotUI(type, 0, null); 
        }
        Debug.Log("[DefenseEquipmentManager] Start 메서드 완료. 모든 방어구 슬롯 UI 초기화.");
    }

    // 모든 방어구 타입에 대한 딕셔너리 초기화
    private void InitializeArmorDictionaries()
    {
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            if (IsArmorType(type))
            {
                if (!EquippedArmorObjs.ContainsKey(type))
                {
                    EquippedArmorObjs.Add(type, null); // 3D 오브젝트는 초기엔 null
                }
                if (!_equippedArmorData.ContainsKey(type))
                {
                    _equippedArmorData.Add(type, null); // 아이템 데이터도 초기엔 null
                }
            }
        }
        Debug.Log("[DefenseEquipmentManager] 방어구 딕셔너리 초기화 완료.");
    }

    // 각 방어구 UI 부모 Transform을 딕셔너리에 매핑 (BagItemPrefab용)
    private void InitializeUIParents()
    {
        _armorUIParents[ItemType.ArmorHead] = HeadArmorSlotUIParent;
        _armorUIParents[ItemType.ArmorBody] = BodyArmorSlotUIParent;
        _armorUIParents[ItemType.ArmorArm] = ArmArmorSlotUIParent;
        _armorUIParents[ItemType.ArmorLeg] = LegArmorSlotUIParent;
        _armorUIParents[ItemType.ArmorHand] = HandArmorSlotUIParent;
        _armorUIParents[ItemType.ArmorFeet] = FeetArmorSlotUIParent;

        foreach(var entry in _armorUIParents)
        {
            if (entry.Value == null)
            {
                Debug.LogWarning($"[DefenseEquipmentManager] UI Parent for BagItemPrefab ({entry.Key}) is not assigned in the Inspector! Please assign it.");
            }
        }
        Debug.Log("[DefenseEquipmentManager] UI 부모 딕셔너리 초기화 완료.");
    }

    // UI 아이콘 추적 딕셔너리 초기화
    private void InitializeUIArmorIcons()
    {
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
        {
            if (IsArmorType(type))
            {
                if (!_currentUIArmorIcons.ContainsKey(type))
                {
                    _currentUIArmorIcons.Add(type, null); // 초기엔 UI 아이콘 없음
                }
            }
        }
        Debug.Log("[DefenseEquipmentManager] UI 방어구 아이콘 추적 딕셔너리 초기화 완료.");
    }

    public bool IsArmorType(ItemType type)
    {
        return 
            type == ItemType.ArmorHead || type == ItemType.ArmorBody ||
            type == ItemType.ArmorArm || type == ItemType.ArmorLeg ||
            type == ItemType.ArmorHand || type == ItemType.ArmorFeet;
    }

    // --- 방어구 장착 로직 (지연된 이전 아이템 반환 적용) ---
    public void EquipArmor(ItemDataSO itemData, Button linkedButton)
    {
        Debug.Log($"[DefenseEquipmentManager] EquipArmor 호출 시작 - 아이템: {itemData?.itemName}, 타입: {itemData?.ItemType} (현재 프레임: {Time.frameCount})", this);

        if (itemData == null)
        {
            Debug.LogError("[DefenseEquipmentManager] 장착하려는 itemData가 null입니다. 장착 불가.", this);
            return;
        }

        if (!IsArmorType(itemData.ItemType))
        {
            //Debug.LogWarning($"[DefenseEquipmentManager] {itemData.itemName} ({itemData.ItemType})은(는) 방어구 타입이 아닙니다. 장착 불가.", this);
            return;
        }

        // 이전에 장착된 아이템 데이터 저장 (가방으로 되돌리기 위해)
        ItemDataSO previousArmorData = null;
        if (_equippedArmorData.ContainsKey(itemData.ItemType) && _equippedArmorData[itemData.ItemType] != null)
        {
            previousArmorData = _equippedArmorData[itemData.ItemType];
           // Debug.Log($"[DefenseEquipmentManager] 이전 장착 아이템 발견: {previousArmorData.itemName}", this);
        }
        else
        {
            //return;
            Debug.Log($"[DefenseEquipmentManager] {itemData.ItemType}에 이전 장착 아이템이 없습니다.", this);
        }

        // 가방에 있는 아이템 이미지 즉시 삭제
        if (linkedButton != null && linkedButton.transform.childCount > 0)
        {
            GameObject childToDestroy = linkedButton.transform.GetChild(0).gameObject;
            //Debug.Log($"[DefenseEquipmentManager] 가방 아이템 제거 시도: {childToDestroy.name}", childToDestroy);
            Destroy(childToDestroy);
        }
        else if (linkedButton != null)
        {
            //return;
            Debug.LogWarning($"[DefenseEquipmentManager] linkedButton ({linkedButton.name})에 자식 오브젝트(BagItemPrefab)가 없습니다. 제거할 것이 없습니다.", linkedButton);
        }
        else
        {
            //return;
            Debug.LogWarning("[DefenseEquipmentManager] linkedButton이 null이어서 가방 아이템을 제거할 수 없습니다.");
        }


        // 기존 장착된 3D 오브젝트 즉시 파괴
        if (EquippedArmorObjs.ContainsKey(itemData.ItemType) && EquippedArmorObjs[itemData.ItemType] != null)
        {
            GameObject objToDestroy = EquippedArmorObjs[itemData.ItemType];
            //Debug.Log($"[DefenseEquipmentManager] 기존 3D 방어구 모델 파괴 시도: {objToDestroy.name}", objToDestroy);
            Destroy(objToDestroy);
            EquippedArmorObjs[itemData.ItemType] = null;
           // Debug.Log($"[DefenseEquipmentManager] 파괴 후 EquippedArmorObjs[{itemData.ItemType}] 상태: {EquippedArmorObjs[itemData.ItemType] == null}", this);
        }
        else
        {
            //return;
           Debug.Log($"[DefenseEquipmentManager] {itemData.ItemType}에 파괴할 기존 3D 방어구 모델이 없습니다.", this);
        }

        // 새로운 아이템 즉시 장착
        InstallNewArmor(itemData);

    
        //원래 있던 거
        // 이전 아이템을 가방으로 되돌리기 (지연 실행)
        if (previousArmorData != null)
        {
           // Debug.Log($"[DefenseEquipmentManager] 이전 아이템 ({previousArmorData.itemName}) 가방 반환 코루틴 시작.", this);
            StartCoroutine(ReturnArmorToBagDelayed(previousArmorData));
        }

       // Debug.Log($"[DefenseEquipmentManager - 방어구 장착] {itemData.ItemType} 방어구 장착 완료 / 방어력 보너스 +{itemData.DefenseBoost}", this);

        //추가
        _currentEquippedArmorData = itemData;
    }

    // --- 새로운 방어구 즉시 설치 ---
    private void InstallNewArmor(ItemDataSO itemData)
    {
        //Debug.Log($"[DefenseEquipmentManager] InstallNewArmor 호출 시작 - 아이템: {itemData?.itemName}, 타입: {itemData?.ItemType} (현재 프레임: {Time.frameCount})", this);

        // CharacterInfoUI에서 3D 방어구 프리팹이 생성될 부모 Transform을 가져옴
        Transform armorPrefabSpawnParent = CharacterInfoUI.Instance.GetArmorPrefabParent(itemData.ItemType);
        if (armorPrefabSpawnParent == null)
        {
            //Debug.LogError($"[DefenseEquipmentManager] CharacterInfoUI에서 {itemData.ItemType}에 대한 armorPrefabParent를 찾을 수 없습니다. 장착 불가.", this);
            return;
        }
       // Debug.Log($"[DefenseEquipmentManager] armorPrefabSpawnParent 찾음: {armorPrefabSpawnParent.name}", armorPrefabSpawnParent);


        GameObject newArmor3DModel = null;

        
        // 상태 업데이트
        EquippedArmorObjs[itemData.ItemType] = newArmor3DModel;
        _equippedArmorData[itemData.ItemType] = itemData;

        // UI 업데이트
        UpdateArmorSlotUI(itemData.ItemType, itemData.DefenseBoost, itemData.BagItemPrefab);
        UpdateTotalDefenseUI();
        //Debug.Log($"[DefenseEquipmentManager] InstallNewArmor 완료. EquippedArmorObjs[{itemData.ItemType}] 현재: {EquippedArmorObjs[itemData.ItemType]?.name ?? "None"}", this);
    }

    // --- 지연된 아이템 가방 반환 코루틴 ---
    private IEnumerator ReturnArmorToBagDelayed(ItemDataSO itemDataToReturn)
    {
        yield return null; // 다음 프레임까지 대기하여 오브젝트 파괴가 완료될 시간을 줌
        
        //Debug.Log($"[DefenseEquipmentManager] 지연된 가방 반환 시작: {itemDataToReturn?.itemName}", this);
        ReturnArmorToBagSequential(itemDataToReturn);
    }

    // --- 방어구 해제 로직 ---
    public void UnequipArmor(ItemDataSO itemData)
    {
       // Debug.Log($"[DefenseEquipmentManager] UnequipArmor 호출 시작 - 아이템: {itemData?.itemName}, 타입: {itemData?.ItemType}", this);

        if (itemData == null)
        {
            Debug.LogError("[DefenseEquipmentManager] 해제하려는 itemData가 null입니다. 해제 불가.", this);
           // return;
        }

        if (!IsArmorType(itemData.ItemType))
        {
            Debug.LogWarning($"[DefenseEquipmentManager] {itemData.itemName} ({itemData.ItemType})은(는) 방어구 타입이 아닙니다. 해제 불가.", this);
           // return;
        }

        // 현재 장착된 아이템 데이터가 일치하는지 확인
        if (_equippedArmorData.ContainsKey(itemData.ItemType) && _equippedArmorData[itemData.ItemType] == itemData)
        {
            // 장착된 3D 오브젝트 파괴
            if (EquippedArmorObjs.ContainsKey(itemData.ItemType) && EquippedArmorObjs[itemData.ItemType] != null)
            {
                GameObject objToDestroy = EquippedArmorObjs[itemData.ItemType];
               // Debug.Log($"[DefenseEquipmentManager] 해제 시 기존 3D 방어구 모델 파괴 시도: {objToDestroy.name}", objToDestroy);
                Destroy(objToDestroy);
                EquippedArmorObjs[itemData.ItemType] = null;
               // Debug.Log($"[DefenseEquipmentManager] 해제 후 EquippedArmorObjs[{itemData.ItemType}] 상태: {EquippedArmorObjs[itemData.ItemType] == null}", this);
            }
            else
            {
                Debug.Log($"[DefenseEquipmentManager] {itemData.ItemType}에 파괴할 기존 3D 방어구 모델이 없습니다. (이미 없거나 일치하지 않음)", this);
            }

            // 딕셔너리에서 참조 초기화
            _equippedArmorData[itemData.ItemType] = null;

            // UI 슬롯에 아이콘 및 방어력 텍스트 초기화
            UpdateArmorSlotUI(itemData.ItemType, 0, null);
            UpdateTotalDefenseUI();

            // 가방으로 되돌리기 (지연)
           // Debug.Log($"[DefenseEquipmentManager] 해제 아이템 ({itemData.itemName}) 가방 반환 코루틴 시작.", this);
            StartCoroutine(ReturnArmorToBagDelayed(itemData));
            
           // Debug.Log($"[DefenseEquipmentManager - 방어구 해제] {itemData.ItemType} 방어구 해제 완료", this);
        }
        else
        {
            Debug.LogWarning($"[DefenseEquipmentManager - 방어구 해제] 해제하려는 {itemData.itemName} ({itemData.ItemType}) 방어구가 현재 장착되어 있지 않습니다. (장착된 아이템: {_equippedArmorData[itemData.ItemType]?.itemName ?? "없음"})", this);
        }
    }

    // --- UI 슬롯에 아이콘 및 방어력 텍스트 업데이트 ---
    private void UpdateArmorSlotUI(ItemType itemType, int defenseBonus, GameObject bagItemPrefab)
    {
        //Debug.Log($"[DefenseEquipmentManager] UpdateArmorSlotUI 호출 - 타입: {itemType}, 방어력: {defenseBonus}, BagPrefab: {bagItemPrefab?.name ?? "None"}", this);

        if (!_armorUIParents.TryGetValue(itemType, out Transform uiParentTransform) || uiParentTransform == null)
        {
            //Debug.LogWarning($"[DefenseEquipmentManager - UI 업데이트] UI parent for BagItemPrefab ({itemType}) not found or not assigned. UI 업데이트 불가.", this);
            return;
        }
        
        // 기존 UI 아이콘 제거
        if (_currentUIArmorIcons.ContainsKey(itemType) && _currentUIArmorIcons[itemType] != null)
        {
            GameObject oldUIIcon = _currentUIArmorIcons[itemType];
            //Debug.Log($"[DefenseEquipmentManager - UI 업데이트] 기존 UI 아이콘 ({oldUIIcon.name}) 제거 시도.", oldUIIcon);
            Destroy(oldUIIcon);
            _currentUIArmorIcons[itemType] = null;
        }

        // 새 BagItemPrefab 인스턴스화 (null이 아니라면)
        if (bagItemPrefab != null)
        {
            //Debug.Log($"[DefenseEquipmentManager - UI 업데이트] 새 BagItemPrefab ({bagItemPrefab.name}) 생성 시도.", this);
            GameObject newBagItemUI = Instantiate(bagItemPrefab, uiParentTransform);
            newBagItemUI.transform.localPosition = Vector3.zero;
            newBagItemUI.transform.localRotation = Quaternion.identity;
            newBagItemUI.transform.localScale = Vector3.one;
            _currentUIArmorIcons[itemType] = newBagItemUI;
            //Debug.Log($"[DefenseEquipmentManager - UI 업데이트] 새 BagItemPrefab ({newBagItemUI.name}) 생성 완료. 부모: {uiParentTransform.name}", newBagItemUI);
        }
        else
        {
            //return;
            Debug.Log($"[DefenseEquipmentManager - UI 업데이트] BagItemPrefab이 null입니다. UI 아이콘을 비웁니다.", this);
        }

        // CharacterInfoUI에 방어력 텍스트만 업데이트 요청
        if (CharacterInfoUI.Instance != null)
        {
            CharacterInfoUI.Instance.UpdateArmorUI(itemType, defenseBonus); 
            //Debug.Log($"[DefenseEquipmentManager - UI 업데이트] CharacterInfoUI에 {itemType} 방어력 텍스트 업데이트 요청.", this);
        }
        else
        {
            //return;
            Debug.LogError("[DefenseEquipmentManager] CharacterInfoUI.Instance가 null입니다. 방어력 텍스트 업데이트 불가.", this);
        }
    }

    // --- 총 방어력 계산 및 UI 업데이트 ---
    public void UpdateTotalDefenseUI()
    {
        int baseDefense = 0;
        if (CharacterManager.Instance != null && CharacterManager.Instance.CurrentStat != null)
        {
            baseDefense = CharacterManager.Instance.CurrentStat.Defense;
        }
        else
        {
            //return;
            Debug.LogWarning("[DefenseEquipmentManager] CharacterManager.Instance 또는 CurrentStat이 null입니다. 기본 방어력을 가져올 수 없습니다.", this);
        }

        int totalEquippedArmorDefense = 0;
        foreach (var itemData in _equippedArmorData.Values)
        {
            if (itemData != null)
            {
                totalEquippedArmorDefense += itemData.DefenseBoost;
            }
        }

        int newTotalDefense = baseDefense + totalEquippedArmorDefense;
        
        if (CharacterInfoUI.Instance != null)
        {
            CharacterInfoUI.Instance.UpdateTotalDefenseTMP(newTotalDefense);
            //Debug.Log($"[DefenseEquipmentManager - 총 방어력 업데이트] 기본: {baseDefense}, 장착 방어구 보너스: {totalEquippedArmorDefense}, 최종 총합: {newTotalDefense}", this);
        }
        else
        {
            //return;
            Debug.LogWarning("[DefenseEquipmentManager] CharacterInfoUI.Instance가 null입니다. 총 방어력 UI 업데이트 불가.", this);
        }
    }

    // --- 0번부터 순차적으로 빈 슬롯에 아이템 배치 ---
    private void ReturnArmorToBagSequential(ItemDataSO itemDataToReturn)
    {
        //Debug.Log($"[DefenseEquipmentManager] ReturnArmorToBagSequential 시작 - 아이템: {itemDataToReturn?.itemName}", this);

        if (itemDataToReturn == null) 
        {
           Debug.LogWarning("[DefenseEquipmentManager] itemDataToReturn이 null입니다! 가방 반환 불가.", this);
           // return;
        }

        if (itemDataToReturn.BagItemPrefab == null)
        {
            Debug.LogWarning($"[DefenseEquipmentManager] {itemDataToReturn.itemName}의 BagItemPrefab이 null입니다. 가방으로 되돌릴 수 없습니다.", this);
            //return;
        }

        if (Bag.Instance == null || Bag.Instance.BagSlots == null || Bag.Instance.BagSlots.Length == 0)
        {
            Debug.LogError("[DefenseEquipmentManager] Bag.Instance 또는 BagSlots가 null이거나 비어있습니다! 가방 반환 불가.", this);
            //return;
        }

        //Debug.Log($"[DefenseEquipmentManager] BagSlots 순차 검색 시작 - 총 슬롯 개수: {Bag.Instance.BagSlots.Length}", this);

        // 0번부터 순차적으로 빈 슬롯 찾기
        for (int i = 0; i < Bag.Instance.BagSlots.Length; i++)
        {
            Transform slot = Bag.Instance.BagSlots[i];
            //Debug.Log($"[DefenseEquipmentManager] 슬롯 {i} 검사 - 자식 개수: {slot.childCount}", slot);

            if (slot.childCount == 0)
            {
               // Debug.Log($"[DefenseEquipmentManager] 빈 슬롯 발견: 슬롯 {i} - 아이템 배치 시작", slot);
                
                // 빈 슬롯에 아이템 배치
                GameObject bagItem = Instantiate(itemDataToReturn.BagItemPrefab, slot);
                bagItem.transform.localPosition = Vector3.zero;
                bagItem.transform.localRotation = Quaternion.identity;
                bagItem.transform.localScale = Vector3.one;

                // BagItemReference 컴포넌트 설정
                BagItemReference reference = bagItem.GetComponent<BagItemReference>();
                if (reference != null)
                {
                    reference.ItemData = itemDataToReturn;
                    //Debug.Log($"[DefenseEquipmentManager] {itemDataToReturn.itemName} 아이템을 슬롯 {i}에 성공적으로 배치", bagItem);
                }
                else
                {
                    Debug.LogWarning($"[DefenseEquipmentManager] BagItemPrefab '{itemDataToReturn.BagItemPrefab.name}'에 BagItemReference 컴포넌트가 없습니다. 데이터 연결 불가.", bagItem);
                }
                
                return; // 성공적으로 배치했으므로 종료
            }
        }

        // 모든 슬롯이 꽉 참
        //Debug.LogError($"[DefenseEquipmentManager] 모든 가방 슬롯이 꽉 차서 {itemDataToReturn.itemName} 아이템을 배치할 수 없습니다! 인벤토리 공간 부족.", this);
    }

    // --- 기존 방법들 (다른 용도로 유지) ---
    private Transform FindEmptyBagSlot()
    {
        if (Bag.Instance == null || Bag.Instance.BagSlots == null || Bag.Instance.BagSlots.Length == 0) return null;

        foreach (Transform slot in Bag.Instance.BagSlots)
        {
            if (slot.childCount == 0)
            {
                return slot;
            }
        }
        return null;
    }

    private void ReturnArmorToBag(ItemDataSO itemDataToReturn)
    {
        if (itemDataToReturn == null) return;

        if (itemDataToReturn.BagItemPrefab != null)
        {
            Transform emptySlot = FindEmptyBagSlot();
            if (emptySlot != null)
            {
                GameObject bagItem = Instantiate(itemDataToReturn.BagItemPrefab, emptySlot);
                bagItem.transform.localPosition = Vector3.zero;
                bagItem.transform.localRotation = Quaternion.identity;
                bagItem.transform.localScale = Vector3.one;

                BagItemReference reference = bagItem.GetComponent<BagItemReference>();
                if (reference != null)
                {
                    reference.ItemData = itemDataToReturn;
                }
                else
                {
                    //return;
                    Debug.LogWarning($"[DefenseEquipmentManager - 가방 복원] BagItemPrefab '{itemDataToReturn.BagItemPrefab.name}'에 BagItemReference 컴포넌트가 없습니다.");
                }
                //Debug.Log($"[DefenseEquipmentManager - 가방 복원] {itemDataToReturn.itemName} 아이템을 가방 슬롯에 되돌림");
            }
            else
            {
                //return;
                Debug.LogWarning("[DefenseEquipmentManager - 가방 복원] 빈 가방 슬롯이 없어 방어구를 가방으로 되돌릴 수 없습니다!");
            }
        }
        else
        {
            //return;
            Debug.LogWarning($"[DefenseEquipmentManager - 가방 복원] {itemDataToReturn.itemName}의 BagItemPrefab이 null이어서 가방으로 되돌릴 수 없습니다.");
        }
    }

    //추가
    public ItemDataSO GetEquippedArmor(ItemType type)
{
    if (_equippedArmorData.TryGetValue(type, out var data))
    {
        return data;
    }
    return null;
}









 //캐릭터 정보 창에서 버리기
 public void ThrowAwayDefenseFormCharacterInfo(ItemType armorType)
    {
       // Debug.Log($"[DefenseEquipmentManager] ThrowAwayDefenseFormCharacterInfo 호출 - 버릴 방어구 타입: {armorType}", this);

        if (!IsArmorType(armorType))
        {
            //Debug.LogWarning($"[DefenseEquipmentManager] {armorType}은(는) 방어구 타입이 아닙니다. 캐릭터 정보 UI에서 버릴 수 없습니다.", this);
            return;
        }

        ItemDataSO itemToDiscard = null;
        if (_equippedArmorData.TryGetValue(armorType, out itemToDiscard) && itemToDiscard != null)
        {
            // 1. 현재 장착된 3D 방어구 모델 파괴
            if (EquippedArmorObjs.ContainsKey(armorType) && EquippedArmorObjs[armorType] != null)
            {
                GameObject objToDestroy = EquippedArmorObjs[armorType];
                //Debug.Log($"[DefenseEquipmentManager] 캐릭터 정보에서 버리는 중: 3D 방어구 모델 ({objToDestroy.name}) 파괴 시도.", objToDestroy);
                Destroy(objToDestroy);
                EquippedArmorObjs[armorType] = null;
            }

            // 2. _equippedArmorData에서 참조 제거 (장착 해제)
            _equippedArmorData[armorType] = null;
            //Debug.Log($"[DefenseEquipmentManager] {armorType} 슬롯의 장착 데이터 초기화.");

            // 3. UI 업데이트 (슬롯 비우기)
            UpdateArmorSlotUI(armorType, 0, null); // 방어력 0, 아이콘 없음

            // 4. 총 방어력 업데이트
            UpdateTotalDefenseUI();

            // 5. 현재 장소에 아이템 드롭
            if (PlaceItemManager.Instance != null && PlaceItemManager.Instance.CurrentRegion != null)
            {
                // PlaceItemManager에 아이템 드롭을 요청합니다.
                // 이 예시에서는 DecreaseItemCount를 사용했지만, 실제 드롭 로직에 맞게 조정해야 합니다.
                // 예를 들어, PlaceItemManager에 DropItem(ItemDataSO item, Vector3 position) 같은 함수가 있다면 좋습니다.
                PlaceItemManager.Instance.CurrentRegion.IncreaseItemCount(itemToDiscard); // 버리는 것이므로 지역 아이템 수량 증가
                //Debug.Log($"[DefenseEquipmentManager] {itemToDiscard.itemName}을(를) 현재 장소에 드롭했습니다.", this);
            }
            else
            {
                //return;
                Debug.LogWarning("[DefenseEquipmentManager] PlaceItemManager.Instance 또는 CurrentRegion이 없어 아이템을 장소에 드롭할 수 없습니다.", this);
            }

            //Debug.Log($"[DefenseEquipmentManager] {itemToDiscard.itemName} ({armorType}) 방어구를 캐릭터 정보에서 성공적으로 버렸습니다.", this);
        }
        else
        {
            //return;
            Debug.LogWarning($"[DefenseEquipmentManager] {armorType} 슬롯에 장착된 방어구가 없습니다. 버릴 것이 없습니다.", this);
        }
    }
}