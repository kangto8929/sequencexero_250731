using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI; // Button 사용을 위해 추가
using System; // Enum.GetValues를 위해 추가

public class WeaponEquipmentManager : MonoBehaviour
{
    public static WeaponEquipmentManager Instance { get; private set; }

    [Header("Weapon Data")]
    public WeaponSO AllWeaponSO; // 인스펙터에서 WeaponSO ScriptableObject를 할당해주세요!

    [Header("Equipment Points")]
    public Transform EquipPointTransform; // 무기 3D 모델이 배치될 Transform (손 등)

    // 현재 장착된 WeaponItem 인스턴스를 저장하여 CharacterManager에 전달할 수 있도록 합니다.
    public WeaponItem EquippedWeaponItem { get; private set; } 
    public WeaponType EquippedWeaponType { get; private set; } = WeaponType.None;
    public int EquippedWeaponPowerBonus { get; private set; } = 0;

    // 현재 장착된 무기의 ItemDataSO를 관리 (DefenseEquipmentManager처럼)
    // [SerializeField]
    public ItemDataSO EquippedWeaponData = null;

   
    private GameObject _currentEquippedWeaponModel; // 현재 장착된 3D 무기 모델
    [SerializeField]
    public ItemDataSO PreviousWeaponData;// = null;

    [SerializeField]
    public GameObject _prefabToSpawn;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (AllWeaponSO == null)
        {
            return;
            //Debug.LogError("[WeaponEquipmentManager] AllWeaponSO가 할당되지 않았습니다! 인스펙터에서 할당해주세요.");
        }
    }

    public void EquipDefaultWeapon(CharacterStatSO characterStat)
{
    if (characterStat == null)
    {
        //Debug.LogError("[WeaponEquipmentManager] EquipDefaultWeapon: characterStat이 null입니다.");
        return;
    }

    // 기본 무기 설정
    EquippedWeaponItem = null; 
    EquippedWeaponType = characterStat.DefaultWeaponType;
    EquippedWeaponPowerBonus = 0; // 기본 무기 자체는 보너스 공격력이 없다고 가정

    // --- 3D 모델 스폰 로직: 기본 무기 모델 스폰 ---
    SpawnWeaponModel(EquippedWeaponType);// ***************


    // CharacterManager에 현재 장착된 무기 정보 전달
    if (CharacterManager.Instance != null)
    {
        CharacterManager.Instance.SetEquippedWeaponInfo(EquippedWeaponType, EquippedWeaponPowerBonus);
    }

    
    
    
}

    public void EquipWeapon(ItemDataSO weaponItemData, Button linkedButton)
    {


        //Debug.Log($"[WeaponEquipmentManager] EquipWeapon 시작 - 아이템 데이터: {weaponItemData?.itemName}, 버튼: {linkedButton?.name}");

        if (weaponItemData == null)
        {
           // Debug.LogWarning("[WeaponEquipmentManager] EquipWeapon: 장착할 ItemDataSO가 null입니다. 무기를 해제합니다.");
            UnequipWeapon();
            return;
        }

        // 이전에 장착된 무기 데이터 저장 (가방으로 되돌리기 위해)
        if (CharacterInfoUI.Instance.AttackIcon.childCount >= 3)
{
    EquippedWeaponData = CharacterInfoUI.Instance.AttackIcon.GetChild(2).GetComponent<WeaponItem>().ItemData;
    PreviousWeaponData = EquippedWeaponData;
}
        /*if(CharacterInfoUI.Instance.AttackIcon.GetChild(2) != null)
        {
            EquippedWeaponData = CharacterInfoUI.Instance.AttackIcon.GetChild(2).GetComponent<WeaponItem>().ItemData;
            //원래 있던 거
            PreviousWeaponData = EquippedWeaponData;
        }*/

         if(CharacterInfoUI.Instance.AttackIcon.childCount < 3)
        {
            PreviousWeaponData = null;
            //Debug.LogWarning("[WeaponEquipmentManager] 이전 장착 무기가 없습니다 (기본 무기 또는 빈 상태).");
        }

        
        
        if (PreviousWeaponData != null)
        {
            Debug.Log($"[WeaponEquipmentManager] 이전 장착 무기 발견: {PreviousWeaponData.itemName}");
        }
        else
        {
            return;
           // Debug.Log("[WeaponEquipmentManager] 이전 장착 무기가 없습니다 (기본 무기 또는 빈 상태).");
        }

        // 가방 아이콘 제거
        if (linkedButton != null && linkedButton.transform.childCount > 0)
        {
            GameObject childToDestroy = linkedButton.transform.GetChild(0).gameObject;
            //Debug.Log($"[WeaponEquipmentManager] 가방 아이템 제거: {childToDestroy.name}");
            Destroy(childToDestroy);
        }
        else if (linkedButton != null)
        {
           // Debug.LogWarning($"[WeaponEquipmentManager] linkedButton ({linkedButton.name})에 자식 오브젝트(BagItemPrefab)가 없습니다. 제거할 것이 없습니다.");
        }

        // 기존 3D 무기 모델 파괴
        ClearEquippedWeaponModel();
        //Debug.Log("[WeaponEquipmentManager] 기존 3D 무기 모델 파괴됨 (있다면).");

        // 새로운 무기 즉시 장착
        InstallNewWeapon(weaponItemData);

        // 이전 무기를 가방으로 되돌리기 (지연 실행)
        if (PreviousWeaponData != null)
        {
           // Debug.Log($"[WeaponEquipmentManager] 이전 무기 ({PreviousWeaponData.itemName}) 가방 반환 코루틴 시작.");
            StartCoroutine(ReturnWeaponToBagDelayed(PreviousWeaponData));
        }

        //Debug.Log($"[WeaponEquipmentManager] {weaponItemData.itemName} ({EquippedWeaponType}) 장착 완료. 공격력 보너스: {EquippedWeaponPowerBonus}");
    }

    // --- 새로운 무기 즉시 설치 ---
    private void InstallNewWeapon(ItemDataSO weaponItemData)
    {
       // Debug.Log($"[WeaponEquipmentManager] InstallNewWeapon 호출 시작 - 아이템: {weaponItemData?.itemName}, 타입: {weaponItemData?.WeaponType}");

        // 무기 상태 업데이트
        EquippedWeaponType = weaponItemData.WeaponType;
        EquippedWeaponPowerBonus = weaponItemData.AttackBoost;
        EquippedWeaponItem = null; // WeaponItem은 별도 관리
        EquippedWeaponData = weaponItemData; // 현재 장착된 무기 데이터 저장

        // CharacterManager에 무기 정보 전달
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.SetEquippedWeaponInfo(EquippedWeaponType, EquippedWeaponPowerBonus);

            // UI 아이콘 업데이트
            if (weaponItemData.BagItemPrefab != null)
            {
                CharacterInfoUI.Instance.UpdateEquippedWeaponUI(weaponItemData.ItemPopupPrefab);
            }
            else
            {
               // Debug.LogWarning($"[WeaponEquipmentManager] {weaponItemData.itemName}의 BagItemPrefab이 null입니다. 무기 UI 아이콘 업데이트 불가.");
                CharacterInfoUI.Instance.UpdateEquippedWeaponUI(null);
            }
        }

        // 캐릭터 정보창 무기 아이콘 및 TMP 업데이트
        CharacterInfoUI.Instance.UpdateEquippedWeaponUI(weaponItemData.WeaponPrefab);
        
        // 3D 모델 스폰
        //SpawnWeaponModel(EquippedWeaponType);//**************************

        //Debug.Log($"[WeaponEquipmentManager] InstallNewWeapon 완료. 현재 장착 무기: {EquippedWeaponData?.itemName ?? "None"}");
    }

    // --- 지연된 무기 가방 반환 코루틴 ---
    private IEnumerator ReturnWeaponToBagDelayed(ItemDataSO weaponDataToReturn)
    {
        yield return null; // 다음 프레임까지 대기하여 오브젝트 파괴가 완료될 시간을 줌
        
        //Debug.Log($"[WeaponEquipmentManager] 지연된 가방 반환 시작: {weaponDataToReturn?.itemName}");
        ReturnWeaponToBagSequential(weaponDataToReturn);
    }

    public void UnequipWeapon()
    {
        //Debug.Log("[WeaponEquipmentManager] UnequipWeapon 시작");

        // 현재 장착된 무기 데이터 저장 (가방으로 되돌리기 위해)
        ItemDataSO currentWeaponData = EquippedWeaponData;

        // 무기 상태 초기화
        EquippedWeaponItem = null;
        EquippedWeaponType = WeaponType.None;
        EquippedWeaponPowerBonus = 0;
       EquippedWeaponData= null;

        // CharacterManager에 무기 해제 정보 전달
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.SetEquippedWeaponInfo(WeaponType.None, 0);
        }

        // CharacterInfoUI 업데이트 - 무기 해제 시 가장 낮은 숙련도로 TotalAttackTMP 업데이트
        if (CharacterInfoUI.Instance != null)
        {
            CharacterInfoUI.Instance.UpdateEquippedWeaponUI(null); // 무기 UI 아이콘 제거
            
            // 무기 숙련도 업데이트 (가장 낮은 숙련도로 TotalAttackTMP 설정)
            if (CharacterManager.Instance != null && CharacterManager.Instance.playerState != null)
            {
                CharacterInfoUI.Instance.UpdateWeaponPowers(CharacterManager.Instance.playerState.WeaponSkills);
            }
        }

        // 3D 모델 제거
        ClearEquippedWeaponModel();

        // 이전 무기를 가방으로 되돌리기 (지연 실행)
        if (currentWeaponData != null)
        {
          //  Debug.Log($"[WeaponEquipmentManager] 해제된 무기 ({currentWeaponData.itemName}) 가방 반환 코루틴 시작.");
            StartCoroutine(ReturnWeaponToBagDelayed(currentWeaponData));
        }
        
        Debug.Log("[WeaponEquipmentManager] 무기 해제 완료. TotalAttackTMP가 가장 낮은 숙련도로 업데이트됨.");
    }


    // --- 0번부터 순차적으로 빈 슬롯에 무기 배치 ---
    private void ReturnWeaponToBagSequential(ItemDataSO weaponDataToReturn)
    {
       // Debug.Log($"[WeaponEquipmentManager] ReturnWeaponToBagSequential 시작 - 아이템: {weaponDataToReturn?.itemName}");

        if (weaponDataToReturn == null) 
        {
            Debug.LogWarning("[WeaponEquipmentManager] weaponDataToReturn이 null입니다! 가방 반환 불가.");
           // return;
        }

        if (weaponDataToReturn.BagItemPrefab == null)
        {
            Debug.LogWarning($"[WeaponEquipmentManager] {weaponDataToReturn.itemName}의 BagItemPrefab이 null입니다. 가방으로 되돌릴 수 없습니다.");
           // return;
        }

        if (Bag.Instance == null || Bag.Instance.BagSlots == null || Bag.Instance.BagSlots.Length == 0)
        {
            Debug.LogError("[WeaponEquipmentManager] Bag.Instance 또는 BagSlots가 null이거나 비어있습니다! 가방 반환 불가.");
         //   return;
        }

        //Debug.Log($"[WeaponEquipmentManager] BagSlots 순차 검색 시작 - 총 슬롯 개수: {Bag.Instance.BagSlots.Length}");

        // 0번부터 순차적으로 빈 슬롯 찾기
        for (int i = 0; i < Bag.Instance.BagSlots.Length; i++)
        {
            Transform slot = Bag.Instance.BagSlots[i];
           // Debug.Log($"[WeaponEquipmentManager] 슬롯 {i} 검사 - 자식 개수: {slot.childCount}");

            if (slot.childCount == 0)
            {
                //Debug.Log($"[WeaponEquipmentManager] 빈 슬롯 발견: 슬롯 {i} - 무기 배치 시작");
                
                // 빈 슬롯에 무기 배치
                GameObject bagItem = Instantiate(weaponDataToReturn.BagItemPrefab, slot);
                bagItem.transform.localPosition = Vector3.zero;
                bagItem.transform.localRotation = Quaternion.identity;
                bagItem.transform.localScale = Vector3.one;

                // BagItemReference 컴포넌트 설정
                BagItemReference reference = bagItem.GetComponent<BagItemReference>();
                if (reference != null)
                {
                    reference.ItemData = weaponDataToReturn;
                    //Debug.Log($"[WeaponEquipmentManager] {weaponDataToReturn.itemName} 무기를 슬롯 {i}에 성공적으로 배치");
                }
                else
                {
                    Debug.LogWarning($"[WeaponEquipmentManager] BagItemPrefab '{weaponDataToReturn.BagItemPrefab.name}'에 BagItemReference 컴포넌트가 없습니다. 데이터 연결 불가.");
                }
                
                return; // 성공적으로 배치했으므로 종료
            }
        }

        // 모든 슬롯이 꽉 참
        Debug.LogError($"[WeaponEquipmentManager] 모든 가방 슬롯이 꽉 차서 {weaponDataToReturn.itemName} 무기를 배치할 수 없습니다! 인벤토리 공간 부족.");
    }


    public void SpawnWeaponModel(WeaponType typeToSpawn)
    {

        //ClearEquippedWeaponModel();

        if (AllWeaponSO == null)
        {
            Debug.LogError("[WeaponEquipmentManager] AllWeaponSO가 할당되지 않았습니다. 3D 모델 스폰 불가.");
            //return;
        }
        if (EquipPointTransform == null)
        {
            Debug.LogError("[WeaponEquipmentManager] EquipPointTransform이 할당되지 않았습니다. 3D 모델 스폰 불가.");
            //return;
        }

        
        switch (typeToSpawn)
        {
            //아이템 해제하고 가장 낮은 숙련도 이미지 넣으려고 하는데 마법을 스폰했습니다까지는 나오는데 프리팹이 안 생김
            case WeaponType.Magic:
                _prefabToSpawn = AllWeaponSO.MagicPrefab;
                //Debug.LogWarning("마법을 스폰했습니다");
                break;
            case WeaponType.Sword:
                _prefabToSpawn = AllWeaponSO.SwordPrefab;
                //Debug.LogWarning("검을 스폰했습니다");
                break;
            case WeaponType.Blunt:
                _prefabToSpawn = AllWeaponSO.BluntPrefab;
                break;
            case WeaponType.Fist:
                _prefabToSpawn = AllWeaponSO.FistPrefab;
               //Debug.LogWarning("주먹을 스폰했습니다");
                break;
            case WeaponType.Bow:
                _prefabToSpawn = AllWeaponSO.BowPrefab;
                //Debug.LogWarning("활을 스폰했습니다");
                break;
            case WeaponType.Throwing:
                _prefabToSpawn = AllWeaponSO.ThrowingPrefab;
                break;
            case WeaponType.Gun:
                _prefabToSpawn = AllWeaponSO.GunPrefab;
                //Debug.LogWarning("총을 스폰했습니다");
                break;
            case WeaponType.None:
                return; 
            default:
               // Debug.LogWarning($"[WeaponEquipmentManager] 알 수 없는 WeaponType '{typeToSpawn}'입니다. 3D 모델 스폰 불가.");
                return;
        }



        if (_prefabToSpawn != null)
        {
            _currentEquippedWeaponModel = Instantiate(_prefabToSpawn, EquipPointTransform);
            _currentEquippedWeaponModel.transform.SetSiblingIndex(0);
            //Debug.Log($"[WeaponEquipmentManager] {typeToSpawn} 3D 모델을 스폰했습니다.");

            
            
        }
        else
        {
            //return;
            Debug.LogWarning($"[WeaponEquipmentManager] WeaponType '{typeToSpawn}'에 해당하는 3D 프리팹이 AllWeaponSO에 할당되지 않았습니다. 3D 모델을 스폰하지 않습니다.");
        }
    } 

    private void ClearEquippedWeaponModel()
    {
        if (_currentEquippedWeaponModel != null)
        {
            Destroy(_currentEquippedWeaponModel);
            _currentEquippedWeaponModel = null;
        }

    }


    //현재 무기 빈 상태일 때
    public void ChangeSpawnWeaponModel(WeaponType typeToSpawn)
    {

         GameObject childToDestroy = EquipPointTransform.GetChild(0).gameObject;
         //Debug.Log($"**********[WeaponEquipmentManager] 0번째 가방 아이템 제거 시도: {childToDestroy.name}", childToDestroy);
         Destroy(childToDestroy);

        
        switch (typeToSpawn)
        {
            //아이템 해제하고 가장 낮은 숙련도 이미지 넣으려고 하는데 마법을 스폰했습니다까지는 나오는데 프리팹이 안 생김
            case WeaponType.Magic:
                _prefabToSpawn = AllWeaponSO.MagicPrefab;
                //Debug.LogWarning("마법을 스폰했습니다");
                break;
            case WeaponType.Sword:
                _prefabToSpawn = AllWeaponSO.SwordPrefab;
                //Debug.LogWarning("검을 스폰했습니다");
                break;
            case WeaponType.Blunt:
                _prefabToSpawn = AllWeaponSO.BluntPrefab;
                break;
            case WeaponType.Fist:
                _prefabToSpawn = AllWeaponSO.FistPrefab;
                //Debug.LogWarning("주먹을 스폰했습니다");
                break;
            case WeaponType.Bow:
                _prefabToSpawn = AllWeaponSO.BowPrefab;
                //Debug.LogWarning("활을 스폰했습니다");
                break;
            case WeaponType.Throwing:
                _prefabToSpawn = AllWeaponSO.ThrowingPrefab;
                break;
            case WeaponType.Gun:
                _prefabToSpawn = AllWeaponSO.GunPrefab;
               // Debug.LogWarning("총을 스폰했습니다");
                break;
            case WeaponType.None:
                return; 
            default:
                //Debug.LogWarning($"[WeaponEquipmentManager] 알 수 없는 WeaponType '{typeToSpawn}'입니다. 3D 모델 스폰 불가.");
                return;
        }

        if (_prefabToSpawn != null)
        {
            
            //if (EquipPointTransform.childCount > 0)
            //{
              //  Transform firstChild = EquipPointTransform.GetChild(0);
               // Destroy(firstChild.gameObject);
               // StartCoroutine(DelayedSpawnWeaponModel(typeToSpawn));
            //}
            _currentEquippedWeaponModel = Instantiate(_prefabToSpawn, EquipPointTransform);
    _currentEquippedWeaponModel.transform.SetSiblingIndex(0);

   // Debug.Log($"이예이예이예예[WeaponEquipmentManager] {typeToSpawn} 3D 모델을 스폰했습니다.");
            
        }
        else
        {
            //return;
            Debug.LogWarning($"[WeaponEquipmentManager] WeaponType '{typeToSpawn}'에 해당하는 3D 프리팹이 AllWeaponSO에 할당되지 않았습니다. 3D 모델을 스폰하지 않습니다.");
        }
    }

    /*private IEnumerator DelayedSpawnWeaponModel(WeaponType typeToSpawn)
{

    yield return null;

    // 프리팹 생성 및 장착
    _currentEquippedWeaponModel = Instantiate(_prefabToSpawn, EquipPointTransform);
    _currentEquippedWeaponModel.transform.SetSiblingIndex(0);

    Debug.Log($"이예이예이예예[WeaponEquipmentManager] {typeToSpawn} 3D 모델을 스폰했습니다.");
}*/

 public void NoneSpawnWeaponModel(WeaponType typeToSpawn)
    {
         GameObject childToDestroy = EquipPointTransform.GetChild(0).gameObject;
         Debug.Log($"**********[없애고 새로 만든다앗앗] 0번째 가방 아이템 제거 시도: {childToDestroy.name}", childToDestroy);
         Destroy(childToDestroy);

        
        switch (typeToSpawn)
        {
            //아이템 해제하고 가장 낮은 숙련도 이미지 넣으려고 하는데 마법을 스폰했습니다까지는 나오는데 프리팹이 안 생김
            case WeaponType.Magic:
                _prefabToSpawn = AllWeaponSO.MagicPrefab;
                //Debug.LogWarning("마법을 스폰했습니다");
                break;
            case WeaponType.Sword:
                _prefabToSpawn = AllWeaponSO.SwordPrefab;
                //Debug.LogWarning("검을 스폰했습니다");
                break;
            case WeaponType.Blunt:
                _prefabToSpawn = AllWeaponSO.BluntPrefab;
                break;
            case WeaponType.Fist:
                _prefabToSpawn = AllWeaponSO.FistPrefab;
                //Debug.LogWarning("주먹을 스폰했습니다");
                break;
            case WeaponType.Bow:
                _prefabToSpawn = AllWeaponSO.BowPrefab;
                //Debug.LogWarning("활을 스폰했습니다");
                break;
            case WeaponType.Throwing:
                _prefabToSpawn = AllWeaponSO.ThrowingPrefab;
                break;
            case WeaponType.Gun:
                _prefabToSpawn = AllWeaponSO.GunPrefab;
                //Debug.LogWarning("총을 스폰했습니다");
                break;
            case WeaponType.None:
                return; 
            default:
                //Debug.LogWarning($"[WeaponEquipmentManager] 알 수 없는 WeaponType '{typeToSpawn}'입니다. 3D 모델 스폰 불가.");
                return;
        }

        StartCoroutine(DelayedSpawnWeaponModel(typeToSpawn));
            
    }

    private IEnumerator DelayedSpawnWeaponModel(WeaponType typeToSpawn)
{

    yield return null;

    // 프리팹 생성 및 장착
    _currentEquippedWeaponModel = Instantiate(_prefabToSpawn, EquipPointTransform);
    _currentEquippedWeaponModel.transform.SetSiblingIndex(0);

    //Debug.Log($"이예이예이예예[WeaponEquipmentManager] {typeToSpawn} 3D 모델을 스폰했습니다.");
}










//캐릭터 정보 창에서 버리기
    public void ThrowAwayWeaponFormCharacterInfo()
    {
       // Debug.Log("[WeaponEquipmentManager] UnequipWeapon 시작");

        // 현재 장착된 무기 데이터 저장 (가방으로 되돌리기 위해)
        ItemDataSO currentWeaponData = EquippedWeaponData;

        // 무기 상태 초기화
        EquippedWeaponItem = null;
        EquippedWeaponType = WeaponType.None;
        EquippedWeaponPowerBonus = 0;
       EquippedWeaponData= null;

        // CharacterManager에 무기 해제 정보 전달
        if (CharacterManager.Instance != null)
        {
            CharacterManager.Instance.SetEquippedWeaponInfo(WeaponType.None, 0);
        }

        // CharacterInfoUI 업데이트 - 무기 해제 시 가장 낮은 숙련도로 TotalAttackTMP 업데이트
        if (CharacterInfoUI.Instance != null)
        {
            CharacterInfoUI.Instance.UpdateEquippedWeaponUI(null); // 무기 UI 아이콘 제거
            
            // 무기 숙련도 업데이트 (가장 낮은 숙련도로 TotalAttackTMP 설정)
            if (CharacterManager.Instance != null && CharacterManager.Instance.playerState != null)
            {
                CharacterInfoUI.Instance.UpdateWeaponPowers(CharacterManager.Instance.playerState.WeaponSkills);
            }
        }

        //추가 - 버리기
        if (PlaceItemManager.Instance != null && PlaceItemManager.Instance.CurrentRegion != null)
        {
            PlaceItemManager.Instance.CurrentRegion.IncreaseItemCount(currentWeaponData); 
        }
        else
        {
            return;
            //Debug.LogWarning("[ItemPopupUI][버리기] PlaceItemManager.Instance.CurrentRegion이 null입니다.");
        }

        // 인벤토리 UI에서 아이템 제거
        // 이 로직은 InventoryUI 또는 BagManager로 옮겨야 합니다.

        if (CharacterInfoUI.Instance.AttackIcon.childCount >= 3)
        {
             Destroy(CharacterInfoUI.Instance.AttackIcon.GetChild(2).gameObject); 
        }
        
        // 3D 모델 제거
        ClearEquippedWeaponModel();


    }
}



