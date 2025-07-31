// AttackHandler.cs
using UnityEngine;

public class AttackHandler : MonoBehaviour
{
    // 공격할 대상 지정 (예시: 충돌 시 등)
    public void AttackTarget(GameObject target)
    {
        // 대상이 데미지를 받을 수 있는지 확인
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable == null)
        {
            Debug.LogWarning("대상이 데미지를 받을 수 없습니다. IDamageable 컴포넌트가 없습니다.");
            return;
        }

        // --- 공격력 정보 가져오기 ---
        // 1. CharacterManager 인스턴스 확인
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("[AttackHandler] CharacterManager.Instance가 null입니다. 공격력 계산 불가.");
            return;
        }

        // 2. WeaponEquipmentManager 인스턴스 확인
        if (WeaponEquipmentManager.Instance == null)
        {
            Debug.LogError("[AttackHandler] WeaponEquipmentManager.Instance가 null입니다. 장착 무기 보너스 적용 불가.");
            return;
        }

        // 현재 장착된 무기 타입 가져오기 (WeaponEquipmentManager에서)
        // CharacterManager의 CurrentWeaponType은 '기본' 무기 타입이므로, 실제 장착된 무기 타입을 가져와야 합니다.
        WeaponType equippedWeaponType = WeaponEquipmentManager.Instance.EquippedWeaponType;
        
        // 장착된 무기 보너스 공격력 가져오기 (WeaponEquipmentManager에서)
        int equippedWeaponBonus = WeaponEquipmentManager.Instance.EquippedWeaponPowerBonus;

        // 현재 캐릭터의 해당 무기 타입에 대한 기본 숙련도 가져오기 (CharacterManager에서)
        // 만약 장착된 무기가 없으면, CharacterManager의 CurrentWeaponType(캐릭터의 기본 무기 숙련도)을 사용
        /*int baseWeaponSkill = CharacterManager.Instance.GetWeaponPower(
            equippedWeaponType != WeaponType.None ? equippedWeaponType : CharacterManager.Instance.CurrentWeaponType);

        // 총 공격력 계산
        int totalAttackPower = baseWeaponSkill + equippedWeaponBonus;

        Debug.Log($"[AttackHandler] 공격력: {totalAttackPower} (기본 숙련도: {baseWeaponSkill}, 장착 무기 보너스: {equippedWeaponBonus}, 무기 타입: {equippedWeaponType})");

        // 데미지 적용
        damageable.TakeDamage(totalAttackPower);
        */
    }
}

