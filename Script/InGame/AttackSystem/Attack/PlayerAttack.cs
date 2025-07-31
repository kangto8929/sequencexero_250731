using UnityEngine;
using UnityEngine.UI;

public class PlayerAttack : MonoBehaviour
{

    public Button AttackButton;

   public void OnAttackButtonClicked()
{
    

    Enemy enemyToAttack = ItemSearchManager.Instance.SelectedEnemy;

    if (enemyToAttack == null)
    {
        Debug.LogWarning("선택된 적이 없습니다!");
        return;
    }

    if(ItemSearchManager.Instance.FoundEnemy.activeSelf == true)
    {
         ItemSearchManager.Instance.FoundEnemy.SetActive(false);
    }

    PlayerAttack playerAttack = GetComponent<PlayerAttack>();
    playerAttack.AttackEnemy(enemyToAttack);

    AttackButton.interactable = false;
}

public void AttackEnemy(Enemy enemy)
{
    if (!ItemSearchManager.Instance.SelectedEnemy.EnemyCharacter.activeSelf)
        return;

    string attackText = CharacterInfoUI.Instance.TotalAttackTMP.text.Trim();

    if (!int.TryParse(attackText, out int totalAttack))
    {
        Debug.LogWarning("TotalAttackTMP에 유효한 숫자가 없습니다.");
        return;
    }

    Debug.Log($"[공격] 플레이어 총 공격력: {totalAttack}");

    float rand = Random.value;
    bool isMiss = rand < 0.1f;
    bool isCritical = !isMiss && rand < 0.3f;//0.3

    int damageBeforeDefense = 0;

    if (!isMiss)
    {
        damageBeforeDefense = isCritical
            ? Mathf.RoundToInt(totalAttack * 1.2f)
            : totalAttack;
    }

    Debug.Log($"[전투] 데미지 (방어 전): {damageBeforeDefense} / 크리티컬: {isCritical} / 미스: {isMiss}");

    int finalDamage = 0;
    if (!isMiss)
    {
        finalDamage = DamageCalculator.CalculateFinalDamage(damageBeforeDefense, enemy.CurrentDefense);
    }

    Debug.Log($"[전투] 최종 데미지 (방어 후): {finalDamage}");

    // ✅ 이 부분에서 직접 ShowDigits 또는 ShowCritical 호출
    DamageDisplay damageDisplay = enemy.DamageDisplay;


    if (isMiss)
    {
        damageDisplay.ShowMiss(enemy);
        Debug.Log("미스 떠야 함");
    }
    else if (isCritical)
    {
        damageDisplay.ShowCritical(finalDamage, enemy);
    }
    else
    {
        damageDisplay.ShowDigits(finalDamage, enemy); // 일반 공격 시
    }

    // 최종 데미지 적용
    //enemy.TakeDamage(finalDamage, isCritical, isMiss);
    enemy.TakeDamage(finalDamage, this, isCritical, isMiss);


    PlayWeaponSFX();
}



    private void PlayWeaponSFX()
    {
        switch (CharacterManager.Instance.playerState.CurrentWeaponType)
        {
            case WeaponType.Gun:
                //SFX_Manager.Instance.GunShotSFX();
                break;
            case WeaponType.Sword:
                //SFX_Manager.Instance.SwordSlashSFX();
                break;
            case WeaponType.Bow:
                //SFX_Manager.Instance.BowShotSFX();
                break;
            case WeaponType.Magic:
                //SFX_Manager.Instance.MagicCastSFX();
                break;
            case WeaponType.Blunt:
                //SFX_Manager.Instance.SwordSlashSFX();
                break;
            case WeaponType.Fist:
                //SFX_Manager.Instance.BowShotSFX();
                break;
            case WeaponType.Throwing:
                //SFX_Manager.Instance.MagicCastSFX();
                break;
            default:
                Debug.LogWarning("무기 타입에 맞는 사운드가 없습니다.");
                break;
        }
    }
}


/*using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

public void AttackEnemy(Enemy enemy)
{
    if (!ItemSearchManager.Instance.EnemyCharacter.activeSelf)
        return;

    if (!int.TryParse(CharacterInfoUI.Instance.TotalAttackTMP.text.Trim(), out int totalAttack))
    {
        Debug.LogWarning("TotalAttackTMP에 유효한 숫자가 없습니다.");
        return;
    }

    Debug.Log($"[공격] 플레이어 총 공격력: {totalAttack}");

    float rand = Random.value;
    bool isMiss = rand < 0.1f;
    bool isCritical = !isMiss && rand < 0.3f;

    int damage = isMiss ? 0 : (isCritical ? totalAttack * 2 : totalAttack);

    Debug.Log($"[전투] 피해량: {damage} / 크리티컬: {isCritical} / 미스: {isMiss}");

    enemy.TakeDamage(damage, isCritical, isMiss);
    PlayWeaponSFX();
}


    private void PlayWeaponSFX()
    {
        switch (CharacterManager.Instance.playerState.CurrentWeaponType)
        {
            case WeaponType.Gun:
                //SFX_Manager.Instance.GunShotSFX();
                break;
            case WeaponType.Sword:
                //SFX_Manager.Instance.SwordSlashSFX();
                break;
            case WeaponType.Bow:
               // SFX_Manager.Instance.BowShotSFX();
                break;
            case WeaponType.Magic:
               // SFX_Manager.Instance.MagicCastSFX();
                break;
            case WeaponType.Blunt:
               // SFX_Manager.Instance.SwordSlashSFX();
                break;
            case WeaponType.Fist:
               // SFX_Manager.Instance.BowShotSFX();
                break;
            case WeaponType.Throwing:
                //SFX_Manager.Instance.MagicCastSFX();
                break;
            default:
                Debug.LogWarning("무기 타입에 맞는 사운드가 없습니다.");
                break;
        }
    }
}*/

/*
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public void AttackEnemy(Enemy enemy)
    {
        //int skillPower = CharacterInfoUI.Instance.CurrentValue; // 숙련도
        //int bonusPower = 0;

        // TMP 텍스트에서 보너스 공격력 파싱
        //if (int.TryParse(CharacterInfoUI.Instance.WeaponPowerTMP.text.Replace("+", "").Trim(), out int val))
        //{
        //    bonusPower = val;
        //}
        if(ItemSearchManager.Instance.EnemyCharacter.activeSelf == true)
        {
 int totalAttack = CharacterManager.Instance.GetWeaponPower(CharacterManager.Instance.playerState.CurrentWeaponType);

        //int totalAttack = skillPower + bonusPower;

        Debug.Log($"플레이어가 적을 공격! 총 공격력: {totalAttack}");

        enemy.TakeDamage(totalAttack);


        // 무기 타입에 따라 다른 소리 출력
        switch (CharacterManager.Instance.playerState.CurrentWeaponType)
        {
            case WeaponType.Gun:
                SFX_Manager.Instance.GunShotSFX();
                break;
            case WeaponType.Sword:
                SFX_Manager.Instance.SwordSlashSFX();
                break;
            
            case WeaponType.Bow:
                SFX_Manager.Instance.BowShotSFX();
                break;
            
            case WeaponType.Magic:
                SFX_Manager.Instance.MagicCastSFX();
                break;
                
            default:
                Debug.LogWarning("무기 타입에 맞는 사운드가 없습니다.");
                break;
        }
        }

       

        
    }
}
*/
