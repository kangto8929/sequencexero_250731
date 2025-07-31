using TMPro; // TextMeshPro를 사용하기 위해 필요
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class CharacterInfoUI : MonoBehaviour
{
    public static CharacterInfoUI Instance { get; private set; }

    [System.Serializable]
    public class ArmorSlotUI
    {
        public ItemType armorType;
        public Transform armorPrefabParent; // 장착된 방어구 모델 (3D)이 들어갈 부모 Transform
        public TextMeshProUGUI defenseValueText;

        public Button ArmorButton;
    }

    [Header("Character Info")]
    public Transform CharacterInfoGrayImageParent;
    private GameObject _currentGrayCharacterInfo;

    [Header("Weapon UI")]
    // 장착된 무기 아이템이 주는 공격력 보너스만 표시 (예: +10)
    public TextMeshProUGUI ItemAttackTMP; 
    public Transform AttackIcon; // 장착된 무기 UI 아이콘이 들어갈 부모 Transform

    // 각 무기 타입별 숙련도 텍스트 (예: 검술: 50)
    public TextMeshProUGUI MagicSkillTMP;
    public TextMeshProUGUI SwordSkillTMP;
    public TextMeshProUGUI BluntSkillTMP;
    public TextMeshProUGUI FistSkillTMP;
    public TextMeshProUGUI BowSkillTMP;
    public TextMeshProUGUI ThrowingSkillTMP;
    public TextMeshProUGUI GunSkillTMP;

    public ArmorSlotUI[] armorSlots;

    [Header("Defense UI")]
    public TextMeshProUGUI TotalDefenseTMP; // 최종 합산된 총 방어력
    public int CurrentDefense;

    [Header("Total Attack UI")]
    // 캐릭터의 무기 타입 숙련도 + 장착 무기 보너스를 합산한 최종 총 공격력
    public TextMeshProUGUI TotalAttackTMP; // TotalWeaponPowerTMP에서 TotalAttackTMP로 변경

    [Header("체력 & 스테미너")]

    public int CurrentHealth;
    [SerializeField]private int _maxHealth;
    public TextMeshProUGUI HealthValueTMP;
    public Slider HealthSlider;

    [SerializeField]private int _currentStamina;
    [SerializeField]private int _maxStamina;
    public TextMeshProUGUI StaminaValueTMP;
    public Slider SteminaSlider;

    private Dictionary<WeaponType, TextMeshProUGUI> _weaponSkillTMPs;
    private Dictionary<ItemType, ArmorSlotUI> _armorSlotUIDictionary;
    private Dictionary<WeaponType, string> _weaponTypeNames;

    private GameObject _currentEquippedWeaponUIIcon; 

    private WeaponType _currentEquippedWeaponType = WeaponType.None;
    private int _currentEquippedWeaponBonus = 0;


    //추가
    [Header("캐릭터 정보 창에서 아이템 터치했을 때 나올 것")]
    public GameObject CharacterItemInfoPanel;
    public Transform CharacterItemInfoParent;

    // 팝업 오브젝트 저장용 변수
private GameObject _characterInfoPopupInstance;

//추가
public ItemDataSO EquippedItem;

 private int _equippedWeaponTotalPower; // 장착된 무기 타입의 전체 공격력

    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeWeaponSkillTMPs();
        InitializeArmorSlotUIDictionary();
        InitializeWeaponTypeNames();
    }

   


private void Start()
{
    foreach (var slot in armorSlots)
    {
        if (slot.ArmorButton == null && slot.armorPrefabParent != null)
        {
            slot.ArmorButton = slot.armorPrefabParent.GetComponent<Button>();
        }

        if (slot.ArmorButton != null)
        {
            ItemType type = slot.armorType;
            Button btn = slot.ArmorButton; // 버튼 참조 보존

            // 🔸 Button과 type 둘 다 넘기기
            btn.onClick.AddListener(() => OnArmorButtonClicked(type, btn));
        }
    }

    if (AttackIcon != null)
    {
        Button attackBtn = AttackIcon.GetComponent<Button>();
        if (attackBtn != null)
        {
            attackBtn.onClick.AddListener(() => OnWeaponButtonClicked(attackBtn)); // 🔸 버튼도 전달
        }
    }
}

private void OnArmorButtonClicked(ItemType type, Button clickedButton)
{
    Debug.Log($"[CharacterInfoUI] 클릭된 방어구 타입: {type}");

    var armorItem = DefenseEquipmentManager.Instance.GetEquippedArmor(type);
    if (armorItem != null && armorItem.ArmorPrefab != null)
    {
        var itemData = armorItem.ArmorPrefab.GetComponent<BagItemReference>().ItemData;
        ShowItemInfoPopup(itemData, clickedButton); // 🔸 버튼 같이 넘기기

        Debug.Log("클릭된 버튼의 이름은:" + clickedButton.name);
    }
    else
    {
        Debug.Log($"[CharacterInfoUI] {type}에 장착된 방어구가 없습니다.");
    }
}


private void OnWeaponButtonClicked(Button clickedButton)
{
    Debug.Log("[CharacterInfoUI] 무기 버튼 클릭됨");

    var weaponData = WeaponEquipmentManager.Instance.EquippedWeaponData;
    EquippedItem = weaponData;
    
    if (EquippedItem != null)
    {
        ShowItemInfoPopup(EquippedItem, clickedButton); // 🔸 버튼 같이 넘기기
        Debug.Log("클릭된 버튼의 이름은:" + clickedButton.name);
    }
    else
    {
        Debug.Log("[CharacterInfoUI] 장착된 무기가 없습니다.");
    }
}




public void ShowItemInfoPopup(ItemDataSO itemData, Button linkedButton)
{
    if (itemData == null)
    {
        Debug.LogWarning("[CharacterInfoUI] itemData가 null입니다.");
        return;
    }

    // 패널 열기
    if (CharacterItemInfoPanel != null)
        CharacterItemInfoPanel.SetActive(true);

    // 이전 팝업 제거
    if (_characterInfoPopupInstance != null)
    {
        Destroy(_characterInfoPopupInstance);
        _characterInfoPopupInstance = null;
    }

    // 프리팹 생성
    GameObject popup = Instantiate(ItemSearchManager.Instance.ItemPopupPrefab, CharacterItemInfoParent);
    _characterInfoPopupInstance = popup;
    popup.SetActive(true);

    // 위치 조정 (X = 18.5f)
    RectTransform rect = popup.GetComponent<RectTransform>();
    if (rect != null)
    {
        Vector2 anchored = rect.anchoredPosition;
        anchored.x = 18.5f;
        rect.anchoredPosition = anchored;
    }


    ItemPopupUI itemPopupUI = popup.GetComponent<ItemPopupUI>();
    if (itemPopupUI != null)
    {
        itemPopupUI.XButton?.SetActive(true);
        itemPopupUI.Setup(itemData, ItemPopupContext.CharacterInfo, linkedButton); // ← 여기!
    }

     //ItemSearchManager.Instance.ItemPopupPrefab.GetComponents<ItemPopupUI>().SetItemInfo(itemData);
}


    //여긴 원래 있던 거거
    public void SetInitialAttack(int initialAttack)
    {
        UpdateTotalAttackTMP(initialAttack); 
        Debug.Log($"[CharacterInfoUI] 초기 공격력 설정 (TotalAttackTMP): {initialAttack}");
    }

    public void SetInitialDefense(int initialDefense)
    {
        UpdateTotalDefenseTMP(initialDefense);
        Debug.Log($"[CharacterInfoUI] 초기 방어력 설정: {initialDefense}");
    }

    public void InitializeWeaponTypeNames()
    {
        _weaponTypeNames = new Dictionary<WeaponType, string>
        {
            { WeaponType.Magic, "마법" },
            { WeaponType.Sword, "검술" },
            { WeaponType.Blunt, "둔기" },
            { WeaponType.Fist, "권법" },
            { WeaponType.Bow, "활" },
            { WeaponType.Throwing, "던지기" },
            { WeaponType.Gun, "총" },
            { WeaponType.None, "없음" } 
        };
    }

    public void InitializeWeaponSkillTMPs()
    {
        _weaponSkillTMPs = new Dictionary<WeaponType, TextMeshProUGUI>
        {
            { WeaponType.Magic, MagicSkillTMP },
            { WeaponType.Sword, SwordSkillTMP },
            { WeaponType.Blunt, BluntSkillTMP },
            { WeaponType.Fist, FistSkillTMP },
            { WeaponType.Bow, BowSkillTMP },
            { WeaponType.Throwing, ThrowingSkillTMP },
            { WeaponType.Gun, GunSkillTMP }
        };

        foreach(var entry in _weaponSkillTMPs)
        {
            if (entry.Value == null)
            {
                Debug.LogWarning($"[CharacterInfoUI] {entry.Key}에 해당하는 TextMeshProUGUI가 인스펙터에 연결되지 않았습니다!");
            }
        }
    }

    public void InitializeArmorSlotUIDictionary()
    {
        _armorSlotUIDictionary = new Dictionary<ItemType, ArmorSlotUI>();
        foreach (var slot in armorSlots)
        {
            if (slot.defenseValueText == null)
            {
                Debug.LogWarning($"[CharacterInfoUI] ArmorSlotUI for {slot.armorType} has no DefenseValueText assigned. Defense text will not update.");
            }
            if (slot.armorPrefabParent == null)
            {
                Debug.LogWarning($"[CharacterInfoUI] ArmorSlotUI for {slot.armorType} has no ArmorPrefabParent assigned. 3D armor model may not spawn correctly.");
            }

            if (_armorSlotUIDictionary.ContainsKey(slot.armorType))
            {
               // Debug.LogWarning($"[CharacterInfoUI] Duplicate ArmorSlotUI for ItemType {slot.armorType} found. Only the first one will be used.");
                continue;
            }
            _armorSlotUIDictionary[slot.armorType] = slot;
        }
    }

    public Transform GetArmorPrefabParent(ItemType itemType)
    {
        if (_armorSlotUIDictionary.TryGetValue(itemType, out ArmorSlotUI slotUI))
        {
            if (slotUI.armorPrefabParent == null)
            {
                Debug.LogWarning($"[CharacterInfoUI] ArmorPrefabParent for {itemType} is null in ArmorSlotUI. Check Inspector assignment.");
            }
            return slotUI.armorPrefabParent;
        }
        //Debug.LogWarning($"[CharacterInfoUI] No ArmorSlotUI found for ItemType {itemType}. Cannot get armor prefab parent.");
        return null;
    }

    public void ShowCharacterGray(GameObject prefab)
    {
        if (CharacterInfoGrayImageParent == null)
        {
            //Debug.LogError("[CharacterInfoUI] CharacterInfoGrayImageParent가 연결되지 않았습니다.");
            return;
        }

        if (_currentGrayCharacterInfo != null)
        {
            Destroy(_currentGrayCharacterInfo);
        }
        if (prefab != null)
        {
            _currentGrayCharacterInfo = Instantiate(prefab, CharacterInfoGrayImageParent);
            _currentGrayCharacterInfo.transform.localPosition = Vector3.zero;
            _currentGrayCharacterInfo.transform.localRotation = Quaternion.identity;
            _currentGrayCharacterInfo.transform.localScale = Vector3.one;
        }
        else
        {
            Debug.LogWarning("[CharacterInfoUI] ShowCharacterGray: 전달된 프리팹이 null입니다. 기존 캐릭터 이미지 제거.");
        }
    }

    public void UpdateArmorUI(ItemType itemType, int defenseBonus) 
    {
        if (_armorSlotUIDictionary.TryGetValue(itemType, out ArmorSlotUI slotUI))
        {
            if (slotUI.defenseValueText != null)
            {
                slotUI.defenseValueText.text = defenseBonus > 0 ? $"+ {defenseBonus}" : "0";
            }
            else
            {
                Debug.LogWarning($"[CharacterInfoUI] {itemType} 슬롯의 defenseValueText가 할당되지 않았습니다.");
            }
        }
        else
        {
            Debug.LogWarning($"[CharacterInfoUI] {itemType}에 해당하는 ArmorSlotUI 설정을 찾을 수 없습니다.");
        }
    }

    public void UpdateTotalDefenseTMP(int totalDefense)
    {
        CurrentDefense = totalDefense;
        if (TotalDefenseTMP != null)
        {
            TotalDefenseTMP.text = CurrentDefense.ToString();
            
        }
        else
        {
            Debug.LogWarning("[CharacterInfoUI] TotalDefenseTMP가 할당되지 않았습니다. 총 방어력 UI 업데이트 불가.");
        }
    }

    public void UpdateItemAttackTMP(int itemBonusAttack)
    {
        if (ItemAttackTMP != null)
        {
            ItemAttackTMP.text = itemBonusAttack > 0 ? $"+ {itemBonusAttack}" : "0";
        }
        else
        {
            Debug.LogWarning("[CharacterInfoUI] ItemAttackTMP가 할당되지 않았습니다. 아이템 공격력 보너스 UI 업데이트 불가.");
        }
    }

    public void UpdateTotalAttackTMP(int totalAttack) 
    {
        if (TotalAttackTMP != null) 
        {
            TotalAttackTMP.text = totalAttack.ToString();
        }
        else
        {
            Debug.LogWarning("[CharacterInfoUI] TotalAttackTMP가 할당되지 않았습니다. 총 공격력 UI 업데이트 불가."); 
        }
    }

    public void UpdateEquippedWeaponUI(GameObject equippedWeaponUIIconPrefab) 
    {
        if (AttackIcon == null)
        {
           // Debug.LogWarning("[CharacterInfoUI] AttackIcon Transform이 할당되지 않았습니다. 장착 무기 UI를 업데이트할 수 없습니다.");
            return;
        }

        if (_currentEquippedWeaponUIIcon != null)
        {
            Destroy(_currentEquippedWeaponUIIcon);
            _currentEquippedWeaponUIIcon = null;
        }

        // 무기 정보 초기화
        _currentEquippedWeaponType = WeaponType.None;
        _currentEquippedWeaponBonus = 0;

        if (equippedWeaponUIIconPrefab != null)
        {
            GameObject newWeaponUI = Instantiate(equippedWeaponUIIconPrefab, AttackIcon);
            newWeaponUI.transform.localPosition = Vector3.zero;
            newWeaponUI.transform.localRotation = Quaternion.identity;
            newWeaponUI.transform.localScale = Vector3.one; 
            _currentEquippedWeaponUIIcon = newWeaponUI; 
            
            newWeaponUI.transform.SetSiblingIndex(2); 
            
            //Debug.Log($"[CharacterInfoUI] AttackIcon에 무기 UI 업데이트: {equippedWeaponUIIconPrefab.name} (인덱스 2)");
            
            // WeaponItem 컴포넌트에서 정보 가져오기
            WeaponItem equipItem = equippedWeaponUIIconPrefab.GetComponent<WeaponItem>();
            if (equipItem != null)
            {
                _currentEquippedWeaponType = equipItem.WeaponType;

                // ✅ 여기에서 AttackBoost 사용
                if (equipItem.ItemData != null)
                {
                    _currentEquippedWeaponBonus = equipItem.ItemData.AttackBoost;
                    Debug.Log($"[CharacterInfoUI] 무기 정보 저장 - 타입: {_currentEquippedWeaponType}, AttackBoost: {_currentEquippedWeaponBonus}");

        
                }
                else
                {
                   // Debug.LogWarning($"[CharacterInfoUI] {equipItem.name}의 ItemData가 비어 있습니다.");
                    _currentEquippedWeaponBonus = 0;
                }

                UpdateItemAttackTMP(_currentEquippedWeaponBonus);

                if (CharacterManager.Instance != null && CharacterManager.Instance.playerState != null)
                {
                    UpdateWeaponPowers(CharacterManager.Instance.playerState.WeaponSkills);
                }
            }
            else
            {
               // Debug.LogWarning($"[CharacterInfoUI] {equippedWeaponUIIconPrefab.name}에 WeaponItem 컴포넌트가 없습니다.");
                UpdateItemAttackTMP(0);
            }
        }
        else
        {
            Debug.Log("[CharacterInfoUI] UpdateEquippedWeaponUI: 장착 해제 또는 프리팹이 없어 무기 UI 아이콘을 지웠습니다.");
            UpdateItemAttackTMP(0);

            if (CharacterManager.Instance != null && CharacterManager.Instance.playerState != null)
            {
                UpdateWeaponPowers(CharacterManager.Instance.playerState.WeaponSkills);
            }
        }
    }

    //******플레이어 체력, 스테미너

    public void UpdatePlayerDefaultStatsUI(PlayerState playerState)
    {
        if (playerState == null)
        {
          //  Debug.LogWarning("[CharacterInfoUI] UpdatePlayerStatsUI: playerState가 null입니다.");
            return;
        }

        if (HealthValueTMP != null)
        {
            _maxHealth = playerState.MaxHealth;
            CurrentHealth = playerState.CurrentHealth;
            HealthSlider.maxValue = _maxHealth;
            HealthSlider.value = _maxHealth;
            HealthValueTMP.text = $"{CurrentHealth} / {_maxHealth}";
        }
        else
        {
            Debug.LogWarning("[CharacterInfoUI] HealthValueTMP가 할당되지 않았습니다.");
        }
        
        if (StaminaValueTMP != null)
        {
            _maxStamina = playerState.MaxStamina;
            _currentStamina = playerState.CurrentStamina;
            SteminaSlider.maxValue = _maxStamina;
            SteminaSlider.value = _maxStamina;
            StaminaValueTMP.text = $"{_currentStamina} / {_maxStamina}";
        }
        else
        {
            Debug.LogWarning("[CharacterInfoUI] StaminaValueTMP가 할당되지 않았습니다.");
        }
    }

    //플레이어 체력 갱신신

     public void UpdatePlayerHealthUI(int current, int max)
{
    CurrentHealth = current;
    _maxHealth = max;

    if (HealthValueTMP != null)
    {
        HealthValueTMP.text = $"{CurrentHealth} / {_maxHealth}";
        HealthSlider.maxValue = _maxHealth;
        HealthSlider.value = CurrentHealth;
    }
}










    public void InitializeDefenseTMPs()
    {
        if (_armorSlotUIDictionary != null)
        {
            foreach (var slotUI in _armorSlotUIDictionary.Values)
            {
                if (slotUI.defenseValueText != null)
                {
                    slotUI.defenseValueText.text = "0"; 
                }
                else
                {
                    Debug.LogWarning($"[CharacterInfoUI] InitializeDefenseTMPs: {slotUI.armorType} 슬롯의 defenseValueText가 할당되지 않았습니다.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[CharacterInfoUI] InitializeDefenseTMPs: _armorSlotUIDictionary가 아직 초기화되지 않았습니다.");
        }

        UpdateTotalDefenseTMP(0); // 총 방어력도 초기화
    }





    public void UpdateWeaponUI(WeaponItem equippedWeapon, Dictionary<WeaponType, int> weaponSkills)
    {
       // Debug.Log("무기 장착 확인중");

        // 총 공격력과 무기 숙련도만 업데이트
        if (CharacterManager.Instance != null)
        {
            UpdateTotalAttackTMP(CharacterManager.Instance.GetCurrentTotalWeaponPower());
        }
        UpdateWeaponPowers(weaponSkills);
    }

    // 가장 낮은 무기 숙련도를 가져오는 함수
    public int GetLowestWeaponSkill(Dictionary<WeaponType, int> weaponSkills)
    {
        if (weaponSkills == null || weaponSkills.Count == 0)
        {
            return 0;
        }

        int lowestSkill = int.MaxValue;
        WeaponType lowestType = WeaponType.None;

        foreach (var entry in weaponSkills)
        {
            if (entry.Value < lowestSkill)
            {
                lowestSkill = entry.Value;
                lowestType = entry.Key;
            }
        }

        Debug.Log($"[CharacterInfoUI] 가장 낮은 무기 숙련도: {lowestType} - {lowestSkill}");
        
       // WeaponEquipmentManager.Instance.SpawnWeaponModel(lowestType);
        return lowestSkill;
    }

    // 무기 초기화 시에도 변수 초기화
    public void InitializeWeaponUI()
    {
        if (AttackIcon != null && _currentEquippedWeaponUIIcon != null)
        {
            Destroy(_currentEquippedWeaponUIIcon);
            _currentEquippedWeaponUIIcon = null;
        }

        // 장착된 무기 정보 초기화
        _currentEquippedWeaponType = WeaponType.None;
        _currentEquippedWeaponBonus = 0;

        // 초기화 시 총 공격력과 아이템 공격력 보너스를 0으로 설정
        UpdateTotalAttackTMP(0); 
        UpdateItemAttackTMP(0); 

        if (_weaponSkillTMPs != null)
        {
            foreach(var tmp in _weaponSkillTMPs.Values)
            {
                if(tmp != null) tmp.text = ""; 
            }
        }
    }





    //추가
     public (WeaponType type, int skill) GetLowestWeaponInfo(Dictionary<WeaponType, int> weaponSkills)
    {

        if (weaponSkills == null || weaponSkills.Count == 0)
        {
            Debug.LogWarning("[CharacterInfoUI] GetLowestWeaponInfo: weaponSkills가 null이거나 비어 있습니다. WeaponType.None, 0을 반환합니다.");
            return (WeaponType.None, 0);
        }

        int lowestSkill = int.MaxValue;
        WeaponType lowestType = WeaponType.None; // 가장 낮은 타입 저장 변수

        foreach (var entry in weaponSkills)
        {
            if (entry.Value < lowestSkill)
            {
                lowestSkill = entry.Value;
                lowestType = entry.Key; // 가장 낮은 스킬을 가진 WeaponType 업데이트
            }
        }

        Debug.Log($"[CharacterInfoUI] GetLowestWeaponInfo 결과: 가장 낮은 무기 숙련도 타입은 '{lowestType}' (스킬: {lowestSkill}) 입니다.");
        return (lowestType, lowestSkill); // 타입과 스킬 값을 튜플로 반환
    }


     public void UpdateWeaponPowers(Dictionary<WeaponType, int> weaponSkills)
    {
        if (weaponSkills == null || _weaponSkillTMPs == null || _weaponTypeNames == null)
        {
            return;
        }

        WeaponType equippedType = _currentEquippedWeaponType;
        int equippedBonus = _currentEquippedWeaponBonus;
        
        _equippedWeaponTotalPower = 0; // 매 업데이트마다 초기화

        foreach (var entry in weaponSkills)
        {
            WeaponType currentType = entry.Key;
            int baseSkill = entry.Value;
            int displayPower = baseSkill;

            if (currentType == equippedType && equippedBonus > 0)
            {
                displayPower += equippedBonus;
            }

            // 장착된 무기 타입의 전체 공격력 저장 (또는 없을 경우 0)
            if (currentType == equippedType)
            {
                _equippedWeaponTotalPower = displayPower;
            }
            // 이전에 장착 해제된 경우 TotalAttackTMP가 0으로 업데이트될 수 있으므로,
            // WeaponType.None이 아닐 때만 _equippedWeaponTotalPower를 설정합니다.

            if (_weaponSkillTMPs.TryGetValue(currentType, out TextMeshProUGUI tmp))
            {
                if (tmp != null)
                {
                    string weaponName = _weaponTypeNames.TryGetValue(currentType, out string name) ? name : currentType.ToString();
                    string newText = $"{weaponName}: {displayPower}";
                    tmp.text = newText;
                }
            }
        }

        // 장착된 무기가 없거나 WeaponType.None인 경우
        if (equippedType == WeaponType.None)
        {
            // 가장 낮은 무기 숙련도 정보 가져오기
            (WeaponType lowestType, int lowestSkill) = GetLowestWeaponInfo(weaponSkills);
            _equippedWeaponTotalPower = lowestSkill; // 총 공격력에 가장 낮은 숙련도 값 설정

            Debug.Log($"[CharacterInfoUI] 장착된 무기가 없으므로 가장 낮은 숙련도 ({lowestType})로 설정: {_equippedWeaponTotalPower}");
            
            // 이 시점에서 WeaponEquipmentManager에 모델 스폰을 요청하는 것은 CharacterManager에서 하는 것이 더 적합합니다.
            // CharacterInfoUI는 UI 업데이트에만 집중합니다.
        }

        // TotalAttackTMP를 _equippedWeaponTotalPower로 업데이트
        UpdateTotalAttackTMP(_equippedWeaponTotalPower);
    }
}
