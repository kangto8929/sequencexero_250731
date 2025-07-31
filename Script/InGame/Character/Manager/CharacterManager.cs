
using TMPro; // TextMeshPro를 사용하기 위해 필요
using UnityEngine;
using System;
using System.Collections.Generic; // Dictionary를 사용하기 위해 필요
using System.Collections;
using JetBrains.Annotations; // IEnumerator를 사용하기 위해 필요

// Enum 정의
// 이 enum들은 별도의 파일 (예: Enums.cs)에 정의하고 using Enums; 를 사용하는 것이
// 더 깔끔하고 관리가 용이합니다. 하지만 여기서는 편의를 위해 포함합니다.
public enum WeaponType
{
    None,
    Magic,
    Sword,
    Blunt,
    Fist,
    Bow,
    Throwing,
    Gun
}

public enum CharacterType
{
    Prophet,
    Murderer,
    Hacker,
    Spy,
    Artist,
    Astronomer,
    Detective,
    SoundEngineer,
    Dealer,
    EnvironmentalEngineer
}

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }

    public PlayerState playerState = new PlayerState();

    public CharacterStatSO[] Characters;

    public Transform CharacterParent;

    private GameObject _currentCharacterModel;

    public CharacterStatSO CurrentStat { get; private set; }
    
    // 장착된 무기 정보를 CharacterManager가 내부적으로 관리하기 위한 필드
    private WeaponType _currentEquippedWeaponType;
public WeaponType CurrentEquippedWeaponType => _currentEquippedWeaponType;
    private int _currentEquippedWeaponPowerBonus; 
    public int CurrentEquippedWeaponPowerBonus =>  _currentEquippedWeaponPowerBonus;
    
    public CharacterType SelectedCharacterType { get; private set; }

    public ChangeButtonThirdImage[] EquipmentButtons;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // 게임 시작 시 한 번만 캐릭터를 선택하고 UI를 초기화합니다.
        SelectCharacter(CharacterType.Detective); 
    }

    /// <summary>
    /// 캐릭터를 선택하고 관련 스탯 및 UI를 초기화합니다.
    /// </summary>
    public void SelectCharacter(CharacterType characterType)
    {
        // UI 초기화는 CharacterInfoUI에서 자체적으로 처리하거나,
        // 필요에 따라 CharacterInfoUI.Instance.InitializeAllUI() 같은 명확한 메서드를 사용하는 것이 좋습니다.
        // 여기서는 중복 호출될 가능성이 있으므로, 필요에 따라 조정하세요.
        if (CharacterInfoUI.Instance != null)
        {
            CharacterInfoUI.Instance.InitializeWeaponUI(); // 무기 UI (아이콘, 총공격력, 숙련도 텍스트) 초기화
            CharacterInfoUI.Instance.InitializeDefenseTMPs(); // 방어구 UI (각 부위 방어력, 총 방어력) 초기화
        }
        else
        {
            Debug.LogError("[CharacterManager] CharacterInfoUI.Instance가 null입니다. UI 초기화 작업을 건너뜱니다. 스크립트 실행 순서를 확인하세요.");
        }

        SelectedCharacterType = characterType;
        
        CharacterStatSO stat = GetCharacterStat(SelectedCharacterType);
        if (stat == null)
        {
            Debug.LogWarning($"[CharacterManager] 캐릭터 타입 {characterType}에 해당하는 스탯을 찾을 수 없습니다.");
            return;
        }
        
        CurrentStat = stat;

        // PlayerState 업데이트 (기본 스탯 적용)
        playerState.MaxHealth = stat.MaxHealth;
        playerState.CurrentHealth = stat.MaxHealth;
        playerState.MaxStamina = stat.MaxStamina;
        playerState.CurrentStamina = stat.MaxStamina;
        playerState.Defense = stat.Defense;

        // 무기 숙련도 설정
        UpdateWeaponSkillsFromStat(stat); 

        // 캐릭터 모델 스폰
        SpawnCharacterModel(); 

        // --- CharacterInfoUI 업데이트 요청 ---
        if (CharacterInfoUI.Instance != null)
        {
            CharacterInfoUI.Instance.ShowCharacterGray(stat.CharacterInfoGrayPrefab); // 캐릭터 정보 이미지
            CharacterInfoUI.Instance.UpdatePlayerDefaultStatsUI(playerState); // 체력, 스태미너 UI 업데이트
            CharacterInfoUI.Instance.UpdateWeaponPowers(playerState.WeaponSkills); // 무기 숙련도 UI 업데이트

            // ⭐ 중요: CharacterStatSO의 기본 무기 UI 아이콘 배치 ⭐
            // DefaultWeaponPrefab을 UI 아이콘 프리팹으로 사용합니다.
            if (stat.DefaultWeaponPrefab != null) // DefaultWeaponPrefab 사용
            {
                CharacterInfoUI.Instance.UpdateEquippedWeaponUI(stat.DefaultWeaponPrefab); // DefaultWeaponPrefab 사용

                //추가
                WeaponEquipmentManager.Instance.EquippedWeaponData = stat.DefaultWeaponPrefab.GetComponent<WeaponItem>().ItemData;
            }
            else
            {
                // 기본 무기 아이콘이 없는 경우, 아이콘을 비웁니다.
                CharacterInfoUI.Instance.UpdateEquippedWeaponUI(null);
                Debug.LogWarning($"[CharacterManager] CharacterStatSO '{stat.name}'에 DefaultWeaponPrefab (UI 아이콘)이 할당되지 않았습니다.");
            }

            // DefenseEquipmentManager가 총 방어력을 업데이트하도록 요청
            if (DefenseEquipmentManager.Instance != null)
            {
                DefenseEquipmentManager.Instance.UpdateTotalDefenseUI();
            }
            else
            {
                Debug.LogWarning("[CharacterManager] DefenseEquipmentManager.Instance가 없습니다. 총 방어력 UI 업데이트 불가.");
                CharacterInfoUI.Instance.UpdateTotalDefenseTMP(playerState.Defense); // 직접 업데이트 (임시)
            }
        }
        
        // 기본 무기 장착 요청 (WeaponEquipmentManager에게) - 3D 모델 및 관련 로직 처리
        // WeaponEquipmentManager는 DefaultWeaponType을 사용하여 AllWeaponSO에서 3D 모델을 가져옵니다.
        if (WeaponEquipmentManager.Instance != null)
        {
            WeaponEquipmentManager.Instance.EquipDefaultWeapon(stat); 
        }
        else
        {
            Debug.LogWarning("[CharacterManager] WeaponEquipmentManager.Instance가 없습니다. 기본 무기 장착 불가.");
        }

        StartCoroutine(DelayedRefreshButtons()); 
    }

    /// <summary>
    /// 무기 장착 없이 캐릭터를 선택합니다 (주로 내부 로직에서 사용).
    /// 이 메서드는 SelectCharacter와 유사하지만 WeaponEquipmentManager를 호출하지 않습니다.
    /// </summary>
    public void SelectCharacterWithoutEquipWeapon(CharacterType characterType)
    {
        SelectedCharacterType = characterType;

        CharacterStatSO stat = GetCharacterStat(SelectedCharacterType);
        if (stat == null)
        {
            Debug.LogWarning($"[CharacterManager] 캐릭터 타입 {characterType}에 해당하는 스탯을 찾을 수 없습니다.");
            return;
        }
        CurrentStat = stat;

        playerState.MaxHealth = stat.MaxHealth;
        playerState.MaxStamina = stat.MaxStamina;
        playerState.CurrentHealth = stat.MaxHealth;
        playerState.CurrentStamina = stat.MaxStamina;
        playerState.Defense = stat.Defense;
        
        UpdateWeaponSkillsFromStat(stat);

        // --- CharacterInfoUI 업데이트 요청 ---
        if (CharacterInfoUI.Instance != null)
        {
            CharacterInfoUI.Instance.ShowCharacterGray(stat.CharacterInfoGrayPrefab);
            CharacterInfoUI.Instance.UpdatePlayerDefaultStatsUI(playerState);
            CharacterInfoUI.Instance.UpdateWeaponPowers(playerState.WeaponSkills);

            // ⭐ 중요: CharacterStatSO의 기본 무기 UI 아이콘 배치 (WithoutEquipWeapon 버전) ⭐
            // DefaultWeaponPrefab을 UI 아이콘 프리팹으로 사용합니다.
            if (stat.DefaultWeaponPrefab != null) // DefaultWeaponPrefab 사용
            {
                CharacterInfoUI.Instance.UpdateEquippedWeaponUI(stat.DefaultWeaponPrefab); // DefaultWeaponPrefab 사용
            }
            else
            {
                CharacterInfoUI.Instance.UpdateEquippedWeaponUI(null);
                Debug.LogWarning($"[CharacterManager] CharacterStatSO '{stat.name}'에 DefaultWeaponPrefab (UI 아이콘)이 할당되지 않았습니다.");
            }

            if (DefenseEquipmentManager.Instance != null)
            {
                DefenseEquipmentManager.Instance.UpdateTotalDefenseUI();
            }
            
            CharacterInfoUI.Instance.UpdateTotalAttackTMP(GetCurrentTotalWeaponPower()); 
        }
        else
        {
            Debug.LogError("[CharacterManager] CharacterInfoUI.Instance가 null입니다. UI 업데이트를 건너뜱니다.");
        }

        StartCoroutine(DelayedRefreshButtons());
    }


    private IEnumerator DelayedRefreshButtons()
    {
        yield return null; 
        foreach (var button in EquipmentButtons) 
        {
            if (button != null)
            {
                button.RefreshSecondWeaponImage();
            }
        }
    }

    private void SpawnCharacterModel()
    {
        if (_currentCharacterModel != null)
            Destroy(_currentCharacterModel);

        if (CurrentStat != null && CurrentStat.CharacterPrefab != null)
        {
            _currentCharacterModel = Instantiate(CurrentStat.CharacterPrefab, CharacterParent);
        }
        else
        {
            Debug.LogWarning("[CharacterManager] CharacterPrefab이 없거나 CurrentStat이 null입니다. 캐릭터 모델을 스폰할 수 없습니다.");
        }
    }

    /// <summary>
    /// 캐릭터 타입에 따른 CharacterStatSO를 반환합니다.
    /// </summary>
    public CharacterStatSO GetCharacterStat(CharacterType characterType)
    {
        foreach (var character in Characters)
        {
            if (character != null && character.CharacterType == characterType)
            {
                return character;
            }
        }
        Debug.LogWarning($"[CharacterManager] CharacterStatSO를 찾을 수 없습니다: {characterType}");
        return null;
    }

    /// <summary>
    /// 무기 숙련도 초기화 (캐릭터 기본 스탯 기반).
    /// CharacterStatSO의 개별 무기 스탯 값을 playerState.WeaponSkills 딕셔너리에 저장합니다.
    /// </summary>
    private void UpdateWeaponSkillsFromStat(CharacterStatSO stat)
    {
        playerState.WeaponSkills.Clear();
        SetWeaponSkill(WeaponType.Magic, stat.Magic);
        SetWeaponSkill(WeaponType.Sword, stat.Sword);
        SetWeaponSkill(WeaponType.Blunt, stat.Blunt);
        SetWeaponSkill(WeaponType.Fist, stat.Fist);
        SetWeaponSkill(WeaponType.Bow, stat.Bow);
        SetWeaponSkill(WeaponType.Throwing, stat.Throwing);
        SetWeaponSkill(WeaponType.Gun, stat.Gun);
    }

    private void SetWeaponSkill(WeaponType type, int baseValue)
    {
        playerState.WeaponSkills[type] = baseValue;
    }



    /// <summary>
    /// 특정 무기 타입의 현재 숙련도(playerState에 저장된 값)를 가져옵니다.
    /// </summary>
    public int GetWeaponPower(WeaponType type)
    {
        if (playerState.WeaponSkills.TryGetValue(type, out int power))
            return power;
        return 0;
    }

    /// <summary>
    /// 현재 캐릭터의 무기 숙련도 중 가장 높은 숙련도를 가진 무기 타입을 반환합니다.
    /// </summary>
    public WeaponType GetHighestAttackPowerWeaponType()
    {
        if (CurrentStat == null || playerState.WeaponSkills == null) return WeaponType.None;

        WeaponType bestWeapon = WeaponType.None;
        int highest = -1;

        foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
        {
            if (type == WeaponType.None) continue;

            int currentPower = GetWeaponPower(type);
            if (currentPower > highest)
            {
                highest = currentPower;
                bestWeapon = type;
            }
        }
        return bestWeapon;
    }

    /// <summary>
    /// 소비성 아이템 사용 로직.
    /// </summary>
    public void UseConsumableItem(ItemDataSO item)
{
    if (item == null) return;

    switch (item.ItemType)
    {
        case ItemType.HealthRecovery:
            playerState.CurrentHealth = Mathf.Min(playerState.MaxHealth, playerState.CurrentHealth + item.HealAmount);
            Debug.Log($"[CharacterManager] 체력 {item.HealAmount} 회복! 현재 체력: {playerState.CurrentHealth}");
            break;

        case ItemType.StaminaRecovery:
            playerState.CurrentStamina = Mathf.Min(playerState.MaxStamina, playerState.CurrentStamina + item.StaminaAmount);
            Debug.Log($"[CharacterManager] 스태미너 {item.StaminaAmount} 회복! 현재 스태미너: {playerState.CurrentStamina}");
            break;
    }

    /*if (CharacterInfoUI.Instance != null)
    {
        CharacterInfoUI.Instance.UpdatePlayerStatsUI(playerState);
    }*/
}


    //캐리터 체력 감소
    public void DecreaseHealth(int damage, Enemy attacker = null)
{
    playerState.CurrentHealth = Mathf.Max(0, playerState.CurrentHealth - damage);
    Debug.Log($"[CharacterManager] 체력 {damage} 감소! 현재 체력: {playerState.CurrentHealth}");

    if (CharacterInfoUI.Instance != null)
    {
        CharacterInfoUI.Instance.UpdatePlayerHealthUI(playerState.CurrentHealth, playerState.MaxHealth);
    }

}






    // 외부에서 무기 숙련도를 증가시키는 메서드 (예: 레벨업, 스킬 획득 등)
    public void IncreaseWeaponSkill(WeaponType weaponType, int amount)
    {
        if (playerState.WeaponSkills.ContainsKey(weaponType))
        {
            playerState.WeaponSkills[weaponType] += amount;
        }
        else
        {
            playerState.WeaponSkills[weaponType] = amount;
        }

        if (CharacterInfoUI.Instance != null)
        {
            CharacterInfoUI.Instance.UpdateWeaponPowers(playerState.WeaponSkills);
            CharacterInfoUI.Instance.UpdateTotalAttackTMP(GetCurrentTotalWeaponPower());
        }
    }


    public int GetCurrentTotalWeaponPower()
    {
        // CharacterInfoUI의 UpdateWeaponPowers가 이미 _equippedWeaponTotalPower를 계산하므로
        // 이 값을 직접 가져오도록 할 수도 있습니다.
        // 또는 여기에서 다시 계산할 수도 있습니다.
        // 여기서는 CharacterInfoUI의 로직에 맞춰 0으로 초기화하고 계산하는 예시.
        int totalPower = 0;
        WeaponType equippedType = WeaponType.None;
        int equippedBonus = 0;

        if (CharacterInfoUI.Instance != null)
        {
            // CharacterInfoUI에서 현재 장착된 무기 타입과 보너스를 가져옴 (internal/private 변수에 접근 불가하므로 public Getter 필요)
            // 임시로 직접 접근하는 방식으로 가정하고, 실제 구현 시 public Getters를 추가하세요.
            // 또는 CharacterManager에서 _currentEquippedWeaponType, _currentEquippedWeaponPowerBonus를 직접 관리하는 것이 좋습니다.
            equippedType = _currentEquippedWeaponType; // CharacterManager의 필드로 관리한다고 가정
            equippedBonus = _currentEquippedWeaponPowerBonus; // CharacterManager의 필드로 관리한다고 가정
        }

        if (playerState != null && playerState.WeaponSkills != null)
        {
            /*if (equippedType != WeaponType.None)
            {
                // 장착된 무기가 있을 경우, 해당 무기 타입의 스킬 + 보너스
                if (playerState.WeaponSkills.TryGetValue(equippedType, out int baseSkill))
                {
                    totalPower = baseSkill + equippedBonus;
                }
            }*/
            if (equippedType == WeaponType.None) // WeaponType.None 일 때 (무기 해제 상태)
        {
            if (CharacterInfoUI.Instance != null)
            {
                (WeaponType lowestType, int lowestSkill) = CharacterInfoUI.Instance.GetLowestWeaponInfo(playerState.WeaponSkills);
                totalPower = lowestSkill;
                Debug.Log($"[CharacterManager] GetCurrentTotalWeaponPower: 현재 무기 없음. 총 공격력을 가장 낮은 숙련도 무기 타입인 '{lowestType}'의 스킬 ({lowestSkill})로 설정.");
            }
        }
            else // WeaponType.None 일 때 (무기 해제 상태)
            {
                // 가장 낮은 무기 숙련도의 공격력을 총 공격력으로 간주
                if (CharacterInfoUI.Instance != null)
                {
                    (WeaponType lowestType, int lowestSkill) = CharacterInfoUI.Instance.GetLowestWeaponInfo(playerState.WeaponSkills);
                    totalPower = lowestSkill;
                }
            }
        }
        return totalPower;
    }

    public void SetEquippedWeaponInfo(WeaponType weaponType, int ignoredPowerBonus)
    {
        Debug.Log($"[CharacterManager] SetEquippedWeaponInfo 호출됨: {weaponType}");

        _currentEquippedWeaponType = weaponType;

        // 실제 공격력 보너스는 WeaponEquipmentManager.Instance.EquippedWeaponItem.ItemData에서 가져와야 함
        int actualAttackBoost = 0;
        if (WeaponEquipmentManager.Instance?.EquippedWeaponItem?.ItemData != null)
        {
            actualAttackBoost = WeaponEquipmentManager.Instance.EquippedWeaponItem.ItemData.AttackBoost;
        }
        _currentEquippedWeaponPowerBonus = actualAttackBoost;

        // UI 업데이트
        if (CharacterInfoUI.Instance != null)
        {

            if (weaponType == WeaponType.None)
            {
                // 가장 낮은 숙련도의 무기 타입 정보를 가져옴
                (WeaponType lowestType, int lowestSkill) = CharacterInfoUI.Instance.GetLowestWeaponInfo(playerState.WeaponSkills);
                Debug.Log($"[CharacterManager] SetEquippedWeaponInfo: 무기 해제됨. 기본 무기 모델을 '{lowestType}'으로 스폰 요청.");
                

                // CharacterInfoUI의 UpdateEquippedWeaponUI를 호출하여 UI 아이콘도 초기화
                CharacterInfoUI.Instance.UpdateEquippedWeaponUI(null); // null을 넘겨 기존 UI 아이콘을 제거
                Debug.Log("******가장 낮은 무기타입으로 출력됨" + lowestType);
                //일단 0번째 거 없앤 다음 실행행


                WeaponEquipmentManager.Instance.NoneSpawnWeaponModel(lowestType);
            }
            else // 무기가 장착된 경우
            {
                // 장착된 무기 UI 아이콘 업데이트
                WeaponEquipmentManager.Instance.ChangeSpawnWeaponModel(weaponType);
                //CharacterInfoUI.Instance.UpdateEquippedWeaponUI(WeaponEquipmentManager.Instance?.EquippedWeaponItem?.);

            }

            // 전체 무기 스킬 및 총 공격력 UI 업데이트
            CharacterInfoUI.Instance.UpdateWeaponUI(
                WeaponEquipmentManager.Instance?.EquippedWeaponItem, 
                playerState.WeaponSkills);
        }
    }
}