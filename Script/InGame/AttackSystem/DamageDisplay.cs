using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class DamageDisplay : MonoBehaviour
{
     [Header("화면 가리는 핏자국")]
    public GameObject BloodImage;

    [Header("숫자 이미지 0~9")]
    public Sprite[] DigitSprites = new Sprite[10];

    [Header("공격 표시 오브젝트")]
    public GameObject DigitParent;

    [Header("공격격 이미지 오브젝트 (세 자리용)")]
    public List<Image> DigitImages = new List<Image>();

    [Header("크리티컬 표시 (피격 효과)")]
    public GameObject CriticalParent;

    [Header("크리티컬 이미지 오브젝트 (세 자리용)")]
    public List<Image> CriticalImages = new List<Image>();

    [Header("데미지 표시 (피격 효과)")]
    public GameObject DamagedParent;

    // 여기에 새로 추가
    [Header("데미지 이미지 오브젝트 (세 자리용)")]
    public List<Image> DamagedImages = new List<Image>();

    [Header("Miss 표시 (피격 효과)")]
    public GameObject MissParent;
    public GameObject EnemyAttackPlayerMissParent;//적이 플레이어 공격했을 때 미스스

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = DigitParent.transform.localScale;
    }

    public void ShowDamage(int damage, Enemy enemy)
    {
        if (damage <= 0)
        {
            DigitParent.SetActive(false);
            return;
        }

        ShowPlayerDamage(damage, enemy);
    }

    public void ShowCritical(int damage, Enemy enemy, float scaleMultiplier = 1.5f)
    {
        if (damage <= 0)
        {
            CriticalParent.SetActive(false);
            return;
        }

        CriticalParent.SetActive(true);

        string damageStr = damage.ToString();

        for (int i = 0; i < CriticalImages.Count; i++)
        {
            int index = damageStr.Length - (CriticalImages.Count - i);
            if (index >= 0)
            {
                int digit = damageStr[index] - '0';
                CriticalImages[i].sprite = DigitSprites[digit];
                CriticalImages[i].gameObject.SetActive(true);
                CriticalImages[i].color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                CriticalImages[i].gameObject.SetActive(false);
            }
        }

         ItemSearchManager.Instance.FoundEnemy.SetActive(false);
        CriticalParent.transform.localScale = _originalScale * scaleMultiplier;
        CriticalParent.transform.DOScale(_originalScale * 1.2f, 0.05f)
            .SetEase(Ease.InBack)//.SetEase(Ease.OutBack)
            .OnComplete(() => { HideCritical(enemy); });
    }

    public void ShowMiss(Enemy enemy)
{
    Debug.Log("미스 떠야 함");
    DigitParent.SetActive(false);     // 숫자 표시 끄기
    MissParent.SetActive(true);       // 미스 표시 켜기
    MissParent.transform.localScale = _originalScale;  // 스케일 초기화
     ItemSearchManager.Instance.FoundEnemy.SetActive(false);

    MissParent.transform.DOScale(_originalScale * 1f, 0.05f)
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
            HideMiss(enemy);
        });
}

    public void ShowDigits(int damage, Enemy enemy, float scaleMultiplier = 1.4f)
    {
        DigitParent.SetActive(true);

        string damageStr = damage.ToString();
         ItemSearchManager.Instance.FoundEnemy.SetActive(false);

        for (int i = 0; i < DigitImages.Count; i++)
        {
            int index = damageStr.Length - (DigitImages.Count - i);
            if (index >= 0)
            {
                int digit = damageStr[index] - '0';
                DigitImages[i].sprite = DigitSprites[digit];
                DigitImages[i].gameObject.SetActive(true);
                DigitImages[i].color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                DigitImages[i].gameObject.SetActive(false);
            }
        }

        // 처음에 커진 상태
        DigitParent.transform.localScale = _originalScale * scaleMultiplier;

        // 팍! 하고 줄어듦 → 이후 함수 호출
        DigitParent.transform.DOScale(_originalScale * 0.9f, 0.05f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                HideDigit(enemy);
            });
    }


    private void HideDigit(Enemy enemy)
{
    bool willCounter = enemy != null && enemy.WillCounterAttack;

    DOVirtual.DelayedCall(1.3f, () =>
    {
        if (enemy != null && !willCounter)
            enemy.EnemyCharacter.SetActive(false);
    });

    DOVirtual.DelayedCall(1.3f, () =>
    {
        DigitParent.SetActive(false);
        MissParent.transform.localScale = Vector3.one;
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
        ItemSearchManager.Instance.FoundEnemy.SetActive(false);
    });
}



   /* private void HideDigit(Enemy enemy)
{
    DOVirtual.DelayedCall(1.3f, () =>
    {
        if (enemy != null && !enemy.WillCounterAttack)
            enemy.EnemyCharacter.SetActive(false);
    });

    DOVirtual.DelayedCall(1.3f, () =>
    {
        DigitParent.SetActive(false);
        MissParent.transform.localScale = Vector3.one;
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
         ItemSearchManager.Instance.FoundEnemy.SetActive(false);
    });
}*/



    private void HideMiss(Enemy enemy)
{
    bool willCounter = enemy != null && enemy.WillCounterAttack;


    DOVirtual.DelayedCall(1.3f, () =>
    {
        if (enemy != null && ! willCounter)
            enemy.EnemyCharacter.SetActive(false);
    });

    DOVirtual.DelayedCall(1.3f, () =>
    {
        MissParent.SetActive(false);
        MissParent.transform.localScale = _originalScale;
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
        ItemSearchManager.Instance.FoundEnemy.SetActive(false);
    });
}


    private void HideCritical(Enemy enemy)
{
    bool willCounter = enemy != null && enemy.WillCounterAttack;

    DOVirtual.DelayedCall(1.3f, () =>
    {
        if (enemy != null && !willCounter)
            enemy.EnemyCharacter.SetActive(false);
    });

    DOVirtual.DelayedCall(1.3f, () =>
    {
        CriticalParent.SetActive(false);
        MissParent.transform.localScale = Vector3.one;
        CriticalParent.transform.localScale = _originalScale;
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
         ItemSearchManager.Instance.FoundEnemy.SetActive(false);
    });
}




    //********** 적 반격격
    //플레이어 데미지 받음
    public void ShowPlayerDamage(int damage, Enemy enemy, bool isSurpriseAttack = false)
{
    if (isSurpriseAttack)
    {
        Debug.Log("기습 공격으로 플레이어가 데미지를 받음");

    }
    else
    {
        Debug.Log("일반 공격으로 플레이어가 데미지를 받음");

    }

    DamagedParent.SetActive(true);

    string damageStr = damage.ToString();

    for (int i = 0; i < DamagedImages.Count; i++)
    {
        int index = damageStr.Length - (DamagedImages.Count - i);
        if (index >= 0)
        {
            int digit = damageStr[index] - '0';
            DamagedImages[i].sprite = DigitSprites[digit];
            DamagedImages[i].gameObject.SetActive(true);
            DamagedImages[i].color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            DamagedImages[i].gameObject.SetActive(false);
        }
    }

    // 항상 0.9로 고정
    float scaleMultiplier = 0.9f;

    DamagedParent.transform.localScale = _originalScale * scaleMultiplier;

    DamagedParent.transform.DOScale(_originalScale * 0.75f, 0.05f)
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
            HideDamaged(enemy); // 플레이어 데미지는 바로 숨김
        });
}

    /*public void ShowPlayerDamage(int damage, Enemy enemy, float scaleMultiplier = 0.9f)
    {
        Debug.Log("반격 혹은 기습");
        Debug.Log("플레이어 데미지 받음");
        DamagedParent.SetActive(true);

        string damageStr = damage.ToString();

        for (int i = 0; i < DamagedImages.Count; i++)
        {
            int index = damageStr.Length - (DamagedImages.Count - i);
            if (index >= 0)
            {
                int digit = damageStr[index] - '0';
                DamagedImages[i].sprite = DigitSprites[digit];
                DamagedImages[i].gameObject.SetActive(true);
                DamagedImages[i].color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                DamagedImages[i].gameObject.SetActive(false);
            }
        }

        // 처음에 커진 상태
        DamagedParent.transform.localScale = _originalScale * scaleMultiplier;

        // 팍! 하고 줄어듦 → 이후 함수 호출
        DamagedParent.transform.DOScale(_originalScale * 0.75f, 0.05f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                HideDamaged(enemy); // 플레이어 데미지는 바로 숨김
            });
    }*/

    // 적이이 플레이어에게 반격격 공격을 미스했을 때 호출
    public void ShowEnemyPlayerMiss(Enemy enemy, bool isSurpriseAttack = false)
{
    /*if (isSurpriseAttack)
    {
        Debug.Log("기습 실패 - 플레이어는 피해를 받지 않음");

    }
    else
    {
        Debug.Log("반격 실패 - 플레이어는 피해를 받지 않음");

    }*/

    EnemyAttackPlayerMissParent.SetActive(true);
    EnemyAttackPlayerMissParent.transform.localScale = _originalScale * 1f;

    EnemyAttackPlayerMissParent.transform.DOScale(_originalScale * 0.75f, 0.05f)
        .SetEase(Ease.InBack)
        .OnComplete(() => { HideEnemyMiss(enemy); });
}

    /*public void ShowEnemyPlayerMiss(Enemy enemy)
    {

        Debug.Log("반격 혹은 기습");
        EnemyAttackPlayerMissParent.SetActive(true);
        EnemyAttackPlayerMissParent.transform.localScale = _originalScale * 1f;

        EnemyAttackPlayerMissParent.transform.DOScale(_originalScale * 0.75f, 0.05f)
            .SetEase(Ease.InBack)//.SetEase(Ease.OutBack)
            .OnComplete(() => { HideEnemyMiss(enemy); });
    }*/

    //플레이어 데미지 받음
    // 반격일 때는 false, 기습일 때는 true
private void HideDamaged(Enemy enemy, bool isSurpriseAttack = false)
{
    if(isSurpriseAttack == false)//기습
    {
        DOVirtual.DelayedCall(1f, () =>
    {
      DamagedParent.SetActive(false);
         DamagedParent.transform.localScale = _originalScale;
          ItemSearchManager.Instance.FoundEnemy.SetActive(false);
          enemy.EnemyCharacter.SetActive(false);
    });

    }

    else//기습이 아닌 경우
    {
        DOVirtual.DelayedCall(1f, () =>
    {
        DamagedParent.SetActive(false);
         enemy.EnemyCharacter.SetActive(false);
        DamagedParent.transform.localScale = _originalScale;
         ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
            ItemSearchManager.Instance.FoundEnemy.SetActive(false);
    });
    }

    
}

private void HideEnemyMiss(Enemy enemy, bool isSurpriseAttack = false)
{
    if(isSurpriseAttack == false)//기습
    {
        DOVirtual.DelayedCall(1f, () =>
    {
        EnemyAttackPlayerMissParent.SetActive(false);
         EnemyAttackPlayerMissParent.transform.localScale = _originalScale;
          ItemSearchManager.Instance.FoundEnemy.SetActive(false);
          enemy.EnemyCharacter.SetActive(false);

    });

    }

    else//기습이 아닌 경우
    {
        DOVirtual.DelayedCall(1f, () =>
    {
        EnemyAttackPlayerMissParent.SetActive(false);
         enemy.EnemyCharacter.SetActive(false);
        EnemyAttackPlayerMissParent.transform.localScale = _originalScale;
         ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
            ItemSearchManager.Instance.FoundEnemy.SetActive(false);
    });
    }


}

    /*private void HideDamaged(Enemy enemy)
    {
        DOVirtual.DelayedCall(2.2f, () =>
        {
            if (enemy != null && enemy.EnemyCharacter != null)
                enemy.EnemyCharacter.SetActive(false);
                enemy.WillCounterAttack = false;
        });

        // UI 정리
        DOVirtual.DelayedCall(2.2f, () =>
        {
            DamagedParent.SetActive(false);
            DamagedParent.transform.localScale = _originalScale;
            if (ItemSearchManager.Instance != null)
                ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
                 ItemSearchManager.Instance.FoundEnemy.SetActive(false);
        });
    }

    private void HideEnemyMiss(Enemy enemy)
    {
        // 반격 미스 후 캐릭터 숨기기
        DOVirtual.DelayedCall(2.2f, () =>
        {
            if (enemy != null && enemy.EnemyCharacter != null)
                enemy.EnemyCharacter.SetActive(false);
                enemy.WillCounterAttack = false;
        });

        // UI 정리
        DOVirtual.DelayedCall(2.2f, () =>
        {
            EnemyAttackPlayerMissParent.SetActive(false);
            EnemyAttackPlayerMissParent.transform.localScale = _originalScale;
            if (ItemSearchManager.Instance != null)
                ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
                 ItemSearchManager.Instance.FoundEnemy.SetActive(false);
        });
    }*/

    //기습
    //surprise


    
}

/*using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class DamageDisplay : MonoBehaviour
{
     [Header("화면 가리는 핏자국")]
    public GameObject BloodImage;

    [Header("숫자 이미지 0~9")]
    public Sprite[] DigitSprites = new Sprite[10];

    [Header("공격 표시 오브젝트")]
    public GameObject DigitParent;

    [Header("공격격 이미지 오브젝트 (세 자리용)")]
    public List<Image> DigitImages = new List<Image>();

    [Header("크리티컬 표시 (피격 효과)")]
    public GameObject CriticalParent;

    [Header("크리티컬 이미지 오브젝트 (세 자리용)")]
    public List<Image> CriticalImages = new List<Image>();

    [Header("데미지 표시 (피격 효과)")]
    public GameObject DamagedParent;

    // 여기에 새로 추가
    [Header("데미지 이미지 오브젝트 (세 자리용)")]
    public List<Image> DamagedImages = new List<Image>();

    [Header("Miss 표시 (피격 효과)")]
    public GameObject MissParent;
    public GameObject EnemyAttackPlayerMissParent;//적이 플레이어 공격했을 때 미스스

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = DigitParent.transform.localScale;
    }

    public void ShowDamage(int damage, Enemy enemy)
    {
        if (damage <= 0)
        {
            DigitParent.SetActive(false);
            return;
        }

        ShowPlayerDamage(damage, enemy);
    }

    public void ShowCritical(int damage, Enemy enemy, float scaleMultiplier = 1.5f)
    {
        if (damage <= 0)
        {
            CriticalParent.SetActive(false);
            return;
        }

        CriticalParent.SetActive(true);

        string damageStr = damage.ToString();

        for (int i = 0; i < CriticalImages.Count; i++)
        {
            int index = damageStr.Length - (CriticalImages.Count - i);
            if (index >= 0)
            {
                int digit = damageStr[index] - '0';
                CriticalImages[i].sprite = DigitSprites[digit];
                CriticalImages[i].gameObject.SetActive(true);
                CriticalImages[i].color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                CriticalImages[i].gameObject.SetActive(false);
            }
        }

        CriticalParent.transform.localScale = _originalScale * scaleMultiplier;
        CriticalParent.transform.DOScale(_originalScale * 1.2f, 0.05f)
            .SetEase(Ease.InBack)//.SetEase(Ease.OutBack)
            .OnComplete(() => { 
                // ✅ 반격 가능성을 체크
                CheckForCounterAttack(enemy, () => HideCritical(enemy));
            });
    }

    public void ShowMiss(Enemy enemy)
    {
         MissParent.transform.DOScale(_originalScale * 1.4f, 0.05f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // ✅ 반격 가능성을 체크
                CheckForCounterAttack(enemy, () => HideMiss(enemy));
            });
    }

    public void ShowDigits(int damage, Enemy enemy, float scaleMultiplier = 1.4f)
    {
        DigitParent.SetActive(true);

        string damageStr = damage.ToString();

        for (int i = 0; i < DigitImages.Count; i++)
        {
            int index = damageStr.Length - (DigitImages.Count - i);
            if (index >= 0)
            {
                int digit = damageStr[index] - '0';
                DigitImages[i].sprite = DigitSprites[digit];
                DigitImages[i].gameObject.SetActive(true);
                DigitImages[i].color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                DigitImages[i].gameObject.SetActive(false);
            }
        }

        // 처음에 커진 상태
        DigitParent.transform.localScale = _originalScale * scaleMultiplier;

        // 팍! 하고 줄어듦 → 이후 함수 호출
        DigitParent.transform.DOScale(_originalScale * 0.9f, 0.05f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                // ✅ 반격 가능성을 체크
                CheckForCounterAttack(enemy, () => HideDigit(enemy));
            });
    }

    // ✅ 새로운 메서드: 반격 가능성 체크
    private void CheckForCounterAttack(Enemy enemy, System.Action hideAction)
    {
        if (enemy == null || enemy.IsDead || CharacterInfoUI.Instance.CurrentHealth <= 0)
        {
            // 적이 죽었거나 플레이어가 죽었으면 바로 숨기기
            hideAction?.Invoke();
            return;
        }

        // 반격 확률 체크 (Enemy.cs와 동일한 로직)
        float counterChance = 0.75f;
        if (Random.value < counterChance)
        {
            Debug.Log("[DamageDisplay] 반격 예정 → 캐릭터 유지");
            // 반격이 예정되어 있으면 일정 시간 후에 숨기기 (반격 애니메이션 시간 고려)
            DOVirtual.DelayedCall(1.5f, () => hideAction?.Invoke());
        }
        else
        {
            Debug.Log("[DamageDisplay] 반격 없음 → 즉시 숨기기");
            // 반격이 없으면 바로 숨기기
            hideAction?.Invoke();
        }
    }

    //********** 적 반격격
    //플레이어 데미지 받음
    public void ShowPlayerDamage(int damage, Enemy enemy, float scaleMultiplier = 0.9f)
    {
        Debug.Log("플레이어 데미지 받음");
        DamagedParent.SetActive(true);

        string damageStr = damage.ToString();

        for (int i = 0; i < DamagedImages.Count; i++)
        {
            int index = damageStr.Length - (DamagedImages.Count - i);
            if (index >= 0)
            {
                int digit = damageStr[index] - '0';
                DamagedImages[i].sprite = DigitSprites[digit];
                DamagedImages[i].gameObject.SetActive(true);
                DamagedImages[i].color = new Color(1f, 1f, 1f, 1f);
            }
            else
            {
                DamagedImages[i].gameObject.SetActive(false);
            }
        }

        // 처음에 커진 상태
        DamagedParent.transform.localScale = _originalScale * scaleMultiplier;

        // 팍! 하고 줄어듦 → 이후 함수 호출
        DamagedParent.transform.DOScale(_originalScale * 0.75f, 0.05f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                HideDamaged(enemy); // 플레이어 데미지는 바로 숨김
            });
    }

    // 적이이 플레이어에게 반격격 공격을 미스했을 때 호출
    public void ShowEnemyPlayerMiss(Enemy enemy)
    {
        EnemyAttackPlayerMissParent.SetActive(true);
        EnemyAttackPlayerMissParent.transform.localScale = _originalScale * 1f;

        EnemyAttackPlayerMissParent.transform.DOScale(_originalScale * 0.75f, 0.05f)
            .SetEase(Ease.InBack)//.SetEase(Ease.OutBack)
            .OnComplete(() => { HideEnemyMiss(enemy); });
    }

    // ✅ 반격이 없을 때 바로 숨기는 버전
    private void HideDigit(Enemy enemy)
    {
        // 즉시 캐릭터 숨기기
        DOVirtual.DelayedCall(0.2f, () =>
        {
            if (enemy != null && enemy.EnemyCharacter != null)
                enemy.EnemyCharacter.SetActive(false);
        });

        // UI 정리
        DOVirtual.DelayedCall(0.2f, () =>
        {
            DigitParent.SetActive(false);
            MissParent.transform.localScale = Vector3.one; 
            if (ItemSearchManager.Instance != null)
                ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
        });
    }

    private void HideMiss(Enemy enemy)
    {
        // 즉시 캐릭터 숨기기
        DOVirtual.DelayedCall(0.2f, () =>
        {
            if (enemy != null && enemy.EnemyCharacter != null)
                enemy.EnemyCharacter.SetActive(false);
        });

        // UI 정리
        DOVirtual.DelayedCall(0.2f, () =>
        {
            MissParent.SetActive(false);
            MissParent.transform.localScale = _originalScale;
            if (ItemSearchManager.Instance != null)
                ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
        });
    }

    private void HideEnemyMiss(Enemy enemy)
    {
        // 반격 미스 후 캐릭터 숨기기
        DOVirtual.DelayedCall(1.2f, () =>
        {
            if (enemy != null && enemy.EnemyCharacter != null)
                enemy.EnemyCharacter.SetActive(false);
        });

        // UI 정리
        DOVirtual.DelayedCall(1.2f, () =>
        {
            EnemyAttackPlayerMissParent.SetActive(false);
            EnemyAttackPlayerMissParent.transform.localScale = _originalScale;
            if (ItemSearchManager.Instance != null)
                ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
        });
    }

    //플레이어 데미지 받음
    private void HideDamaged(Enemy enemy)
    {
        DOVirtual.DelayedCall(1.2f, () =>
        {
            if (enemy != null && enemy.EnemyCharacter != null)
                enemy.EnemyCharacter.SetActive(false);
        });

        // UI 정리
        DOVirtual.DelayedCall(1.2f, () =>
        {
            DamagedParent.SetActive(false);
            DamagedParent.transform.localScale = _originalScale;
            if (ItemSearchManager.Instance != null)
                ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
        });
    }
    
    private void HideCritical(Enemy enemy)
    {
        // 즉시 캐릭터 숨기기
        DOVirtual.DelayedCall(0.2f, () =>
        {
            if (enemy != null && enemy.EnemyCharacter != null)
                enemy.EnemyCharacter.SetActive(false);
        });

        // UI 정리
        DOVirtual.DelayedCall(0.2f, () =>
        {
            CriticalParent.SetActive(false);
            MissParent.transform.localScale = Vector3.one; 
            CriticalParent.transform.localScale = _originalScale;
            if (ItemSearchManager.Instance != null)
                ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
        });
    }
}*/

/*using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class DamageDisplay : MonoBehaviour
{
     [Header("화면 가리는 핏자국")]
    public GameObject BloodImage;

    [Header("숫자 이미지 0~9")]
    public Sprite[] DigitSprites = new Sprite[10];

    [Header("공격 표시 오브젝트")]
    public GameObject DigitParent;

    [Header("공격격 이미지 오브젝트 (세 자리용)")]
    public List<Image> DigitImages = new List<Image>();

    [Header("크리티컬 표시 (피격 효과)")]
    public GameObject CriticalParent;

    [Header("크리티컬 이미지 오브젝트 (세 자리용)")]
    public List<Image> CriticalImages = new List<Image>();

    [Header("데미지 표시 (피격 효과)")]
    public GameObject DamagedParent;

    // 여기에 새로 추가
    [Header("데미지 이미지 오브젝트 (세 자리용)")]
    public List<Image> DamagedImages = new List<Image>();

    [Header("Miss 표시 (피격 효과)")]
    public GameObject MissParent;
    public GameObject EnemyAttackPlayerMissParent;//적이 플레이어 공격했을 때 미스스

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = DigitParent.transform.localScale;
    }

    public void ShowDamage(int damage, Enemy enemy)
{
    if (damage <= 0)
    {
        DigitParent.SetActive(false);
        return;
    }

    ShowPlayerDamage(damage, enemy);
}


public void ShowCritical(int damage, Enemy enemy, float scaleMultiplier = 1.5f)
{
    if (damage <= 0)
    {
        CriticalParent.SetActive(false);
        return;
    }

    CriticalParent.SetActive(true);

    string damageStr = damage.ToString();

    for (int i = 0; i < CriticalImages.Count; i++)
    {
        int index = damageStr.Length - (CriticalImages.Count - i);
        if (index >= 0)
        {
            int digit = damageStr[index] - '0';
            CriticalImages[i].sprite = DigitSprites[digit];
            CriticalImages[i].gameObject.SetActive(true);
            CriticalImages[i].color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            CriticalImages[i].gameObject.SetActive(false);
        }
    }

    CriticalParent.transform.localScale = _originalScale * scaleMultiplier;
    CriticalParent.transform.DOScale(_originalScale * 1.2f, 0.05f)
        .SetEase(Ease.InBack)//.SetEase(Ease.OutBack)
        .OnComplete(() => { HideCritical(enemy); });
}



public void ShowMiss(Enemy enemy)
{

     MissParent.transform.DOScale(_originalScale * 1.4f, 0.05f)
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
            HideMiss(enemy); // 크기 회복은 이 함수 밖에서!
        });
}



public void ShowDigits(int damage, Enemy enemy, float scaleMultiplier = 1.4f)
{
    DigitParent.SetActive(true);

    string damageStr = damage.ToString();

    for (int i = 0; i < DigitImages.Count; i++)
    {
        int index = damageStr.Length - (DigitImages.Count - i);
        if (index >= 0)
        {
            int digit = damageStr[index] - '0';
            DigitImages[i].sprite = DigitSprites[digit];
            DigitImages[i].gameObject.SetActive(true);
            DigitImages[i].color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            DigitImages[i].gameObject.SetActive(false);
        }
    }

    // 처음에 커진 상태
    DigitParent.transform.localScale = _originalScale * scaleMultiplier;

    // 팍! 하고 줄어듦 → 이후 함수 호출
    DigitParent.transform.DOScale(_originalScale * 0.9f, 0.05f)
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
            HideDigit(enemy); // 크기 회복은 이 함수 밖에서!
        });
}


//********** 적 반격격
//플레이어 데미지 받음
public void ShowPlayerDamage(int damage, Enemy enemy, float scaleMultiplier = 0.9f)
{

    Debug.Log("플레이어 데미지 받음");
   DamagedParent.SetActive(true);

    string damageStr = damage.ToString();

    for (int i = 0; i < DamagedImages.Count; i++)
    {
        int index = damageStr.Length - (DamagedImages.Count - i);
        if (index >= 0)
        {
            int digit = damageStr[index] - '0';
            DamagedImages[i].sprite = DigitSprites[digit];
            DamagedImages[i].gameObject.SetActive(true);
            DamagedImages[i].color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            DamagedImages[i].gameObject.SetActive(false);
        }
    }

    // 처음에 커진 상태
    DamagedParent.transform.localScale = _originalScale * scaleMultiplier;

    // 팍! 하고 줄어듦 → 이후 함수 호출
    DamagedParent.transform.DOScale(_originalScale * 0.75f, 0.05f)
        .SetEase(Ease.InBack)
        .OnComplete(() =>
        {
            HideDamaged(enemy); // 크기 회복은 이 함수 밖에서!
        });
}

// 적이이 플레이어에게 반격격 공격을 미스했을 때 호출
    public void ShowEnemyPlayerMiss(Enemy enemy)
    {
        EnemyAttackPlayerMissParent.SetActive(true);
        EnemyAttackPlayerMissParent.transform.localScale = _originalScale * 1f;

        EnemyAttackPlayerMissParent.transform.DOScale(_originalScale * 0.75f, 0.05f)
            .SetEase(Ease.InBack)//.SetEase(Ease.OutBack)
            .OnComplete(() => { HideEnemyMiss(enemy); });
    }




private void HideDigit(Enemy enemy)
{

    DOVirtual.DelayedCall(1.2f, () =>
    {
        if (enemy != null)
            enemy.EnemyCharacter.SetActive(false);
    });

    // 1.5초 후에 DigitParent 비활성화 + 원래 스케일 복구 + 버튼 활성화
    DOVirtual.DelayedCall(1.2f, () =>
    {
        DigitParent.SetActive(false);
        MissParent.transform.localScale = Vector3.one; 
        //DigitParent.transform.localScale = _originalScale;  // 혹시 이후 재사용 대비
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
    });
}

private void HideMiss(Enemy enemy)
{

    // 1초 후에 EnemyCharacter 비활성화
    DOVirtual.DelayedCall(1.2f, () =>
    {
        if (enemy != null)
            enemy.EnemyCharacter.SetActive(false);
    });

    // 1.5초 후에 DigitParent 비활성화 + 원래 스케일 복구 + 버튼 활성화
    DOVirtual.DelayedCall(1.2f, () =>
    {
        MissParent.SetActive(false);
        MissParent.transform.localScale = _originalScale;  // 혹시 이후 재사용 대비
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
    });
}

private void HideEnemyMiss(Enemy enemy)
{

    // 1초 후에 EnemyCharacter 비활성화
    DOVirtual.DelayedCall(1.2f, () =>
    {
        if (enemy != null)
            enemy.EnemyCharacter.SetActive(false);
    });

    // 1.5초 후에 DigitParent 비활성화 + 원래 스케일 복구 + 버튼 활성화
    DOVirtual.DelayedCall(1.2f, () =>
    {
        EnemyAttackPlayerMissParent.SetActive(false);
        EnemyAttackPlayerMissParent.transform.localScale = _originalScale;  // 혹시 이후 재사용 대비
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
    });
}

//플레이어 데미지 받음
private void HideDamaged(Enemy enemy)
{

    DOVirtual.DelayedCall(1.2f, () =>
    {
        if (enemy != null)
            enemy.EnemyCharacter.SetActive(false);
    });

    // 1.5초 후에 DigitParent 비활성화 + 원래 스케일 복구 + 버튼 활성화
    DOVirtual.DelayedCall(1.2f, () =>
    {
        DamagedParent.SetActive(false);
        DamagedParent.transform.localScale = _originalScale;  // 혹시 이후 재사용 대비
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
    });
}
private void HideCritical(Enemy enemy)
{

     DOVirtual.DelayedCall(1.2f, () =>
    {
        if (enemy != null)
            enemy.EnemyCharacter.SetActive(false);
    });

    // 1.5초 후에 DigitParent 비활성화 + 원래 스케일 복구 + 버튼 활성화
    DOVirtual.DelayedCall(1.2f, () =>
    {
        CriticalParent.SetActive(false);
        MissParent.transform.localScale = Vector3.one; 
        CriticalParent.transform.localScale = _originalScale;  // 혹시 이후 재사용 대비
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
    });
}



}*/






    /*public void ShowDamage(int damage)
    {
        if (damage <= 0)
        {
            DigitParent.SetActive(false);
            return;
        }

        //ShowDigits(damage, 1.0f);
        UpdateDamagedImage(damage); // 여기서 데미지에 맞게 이미지 갱신 호출
    }

    public void ShowCritical(int damage)
    {
        if (damage <= 0)
        {
            DigitParent.SetActive(false);
            return;
        }

        ShowDigits(damage, 1.5f);
        //UpdateDamagedImage(damage);
    }

    private void ShowDigits(int damage, float scaleMultiplier)
    {
        DigitParent.SetActive(true);

        string damageStr = damage.ToString();

        for (int i = 0; i < DigitImages.Count; i++)
        {
            int index = damageStr.Length - (DigitImages.Count - i);
            if (index >= 0)
            {
                int digit = damageStr[index] - '0';
                DigitImages[i].sprite = DigitSprites[digit];
                DigitImages[i].gameObject.SetActive(true);
            }
            else
            {
                DigitImages[i].gameObject.SetActive(false);
            }
        }

        DigitParent.transform.localScale = _originalScale * scaleMultiplier;
        DigitParent.transform.DOScale(_originalScale * 1.0f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => { Invoke(nameof(HideDamage), 1f); });
    }

    public void UpdateDamagedImage(int damage)
    {
       DamagedParent.SetActive(true);

        string damageStr = damage.ToString();

        for (int i = 0; i < DamagedImages.Count; i++)
        {
            int index = damageStr.Length - (DamagedImages.Count - i);
            if (index >= 0)
            {
                int digit = damageStr[index] - '0';
               DamagedImages[i].sprite = DigitSprites[digit];
                DamagedImages[i].gameObject.SetActive(true);
            }
            else
            {
               DamagedImages[i].gameObject.SetActive(false);
            }
        }

        DamagedParent.transform.localScale = Vector3.one * 1.5f;
        DamagedParent.transform.DOScale(Vector3.one, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => Invoke(nameof(HideDamaged), 1f));
    }

    public void ShowMiss()
    {
        MissParent.SetActive(true);
        MissParent.transform.localScale = Vector3.one * 1.5f;
        MissParent.transform.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => { Invoke(nameof(HideMiss), 1f); });
    }

    private void HideDamage()
    {
        DigitParent.SetActive(false);
        ItemSearchManager.Instance.EnemyCharacter.SetActive(false);
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
    }

    private void HideMiss()
    {
        MissParent.SetActive(false);
        ItemSearchManager.Instance.EnemyCharacter.SetActive(false);
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
    }

    private void HideDamaged()
    {
        DamagedParent.SetActive(false);
        ItemSearchManager.Instance.EnemyCharacter.SetActive(false);
        ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);
    }
}*/


/*using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class DamageDisplay : MonoBehaviour
{
    [Header("숫자 이미지 0~9")]
    public Sprite[] DigitSprites = new Sprite[10];

    [Header("데미지 주는 이미지 오브젝트 (세 자리용)")]
    public List<Image> DigitImages = new List<Image>();

    [Header("부모 오브젝트")]
    public GameObject DigitParent;

    [Header("Miss 표시 오브젝트")]
    public GameObject MissParent;

    [Header("맞았을 때 표시 (피격 효과)")]
    public GameObject DamagedParent;

    [Header("데미지 받는 이미지지 오브젝트 (세 자리용)")]
    public List<Image> DamagedImages = new List<Image>();

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = DigitParent.transform.localScale;
    }

    // 일반 데미지
    public void ShowDamage(int damage)
    {
        if (damage <= 0)
        {
            DigitParent.SetActive(false);
            return;
        }

        ShowDigits(damage, 1.0f); // 기본 크기
    }

    // 크리티컬 데미지
    public void ShowCritical(int damage)
    {
        if (damage <= 0)
        {
            DigitParent.SetActive(false);
            return;
        }

        ShowDigits(damage, 1.5f); // 더 크게 보여줌
    }

    // 숫자 이미지 띄우는 내부 함수
    private void ShowDigits(int damage, float scaleMultiplier)
    {
        DigitParent.SetActive(true);

        string damageStr = damage.ToString();

        for (int i = 0; i < DigitImages.Count; i++)
        {
            int index = damageStr.Length - (DigitImages.Count - i);
            if (index >= 0)
            {
                int digit = damageStr[index] - '0';
                DigitImages[i].sprite = DigitSprites[digit];
                DigitImages[i].gameObject.SetActive(true);
            }
            else
            {
                DigitImages[i].gameObject.SetActive(false);
            }
        }

        // 애니메이션
        DigitParent.transform.localScale = _originalScale * scaleMultiplier;
        DigitParent.transform.DOScale(_originalScale * 1.0f, 0.15f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                Invoke(nameof(HideDamage), 1f);
            });
    }

    public void ShowMiss()
    {
        MissParent.SetActive(true);
        MissParent.transform.localScale = Vector3.one * 1.5f;
        MissParent.transform.DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                Invoke(nameof(HideMiss), 1f);
            });
    }

    public void ShowDamaged()
    {
        DamagedParent.SetActive(true);
        // 만약 애니메이션이 있다면 DOTween으로 연출 추가 가능
        Invoke(nameof(HideDamaged), 1f);
    }

    private void HideDamage()
    {
        DigitParent.SetActive(false);
    }

    private void HideMiss()
    {
        MissParent.SetActive(false);
    }

    private void HideDamaged()
    {
        DamagedParent.SetActive(false);
    }
}*/


/*using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening; // ← DOTween 네임스페이스 추가

public class DamageDisplay : MonoBehaviour
{
    [Header("숫자 이미지 0~9")]
    public Sprite[] DigitSprites = new Sprite[10];

    [Header("데미지 이미지 오브젝트 (세 자리용)")]
    public List<Image> DigitImages = new List<Image>();

    [Header("부모 오브젝트")]
    public GameObject DigitParent;

    private Vector3 _originalScale = Vector3.one;

    private void Awake()
    {
        _originalScale = DigitParent.transform.localScale;
    }

    public void ShowDamage(int damage)
    {
        if (damage <= 0)
        {
            DigitParent.SetActive(false);
            return;
        }

        DigitParent.SetActive(true);

        string damageStr = damage.ToString();

        for (int i = 0; i < DigitImages.Count; i++)
        {
            int digitIndexFromRight = damageStr.Length - (DigitImages.Count - i);
            if (digitIndexFromRight >= 0)
            {
                int digit = damageStr[digitIndexFromRight] - '0';
                DigitImages[i].gameObject.SetActive(true);
                DigitImages[i].sprite = DigitSprites[digit];
            }
            else
            {
                DigitImages[i].gameObject.SetActive(false);
            }
        }

        // "팍!" 스케일 애니메이션
        DigitParent.transform.localScale = _originalScale * 1.7f; // 먼저 줄여놓고
        DigitParent.transform.DOScale(_originalScale * 1f, 0.1f)//1.3, 0.1f
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                DigitParent.transform.DOScale(_originalScale, 0.1f).SetEase(Ease.InQuad);
                 // 1초 뒤 HideDamage 호출
            Invoke(nameof(HideDamage), 1f);
            });
    }

    public void HideDamage()
    {
        DigitParent.SetActive(false);
        ItemSearchManager.Instance.EnemyCharacter.SetActive(false);
    }
}
*/