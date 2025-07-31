using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;
using System;
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("스탯 설정")]
    public CharacterStatSO EnemyStat;

    [Header("데미지 표시")]
    public DamageDisplay DamageDisplay;

    [SerializeField] private int _MaxHealth;
    [SerializeField] private int _CurrentHealth;

    public int CurrentDefense;
    [SerializeField] private int _currentAttackPower;

    [Header("화면에 보일 적 캐릭터")]
    public GameObject EnemyCharacter;

    private EnemyMoveManager _enemyMoveManager;
    private bool _isCounterAttackScheduled = false;
    public bool WillCounterAttack;
    public bool IsDead => _enemyMoveManager != null && _enemyMoveManager.IsDead;

    private const float CounterAttackDelay = 1.2f;
private const float CounterChance = 1.0f;
private const float HitChance = 0.85f;
private const float CounterDamageMultiplier = 0.5f;


[Header("기습 반격 관련련")]


    private bool canBackAttack = true;
    //private bool isCounterAttacking = false;


    private void Start()
    {
        if (EnemyStat == null)
        {
            Debug.LogError("CharacterStatSO가 할당되지 않았습니다!");
            return;
        }

        _enemyMoveManager = GetComponent<EnemyMoveManager>();
        if (_enemyMoveManager == null)
            Debug.LogWarning($"{gameObject.name}에 EnemyMoveManager가 없습니다!");

        _MaxHealth = EnemyStat.MaxHealth;
        _CurrentHealth = _MaxHealth;
        CurrentDefense = EnemyStat.Defense;
        _currentAttackPower = GetBaseAttackPower();
    }

    public void TakeDamage(int baseDamage, bool isCritical = false, bool isMiss = false)
    {
        TakeDamage(baseDamage, null, isCritical, isMiss);
    }

    public void TakeDamage(int baseDamage, PlayerAttack attacker, bool isCritical = false, bool isMiss = false)
    {
        if (IsDead) return;

        // 반격 가능 상태로 설정 (필요에 따라 조건 조정)
     WillCounterAttack = true;

        int damage = Mathf.RoundToInt(baseDamage * 100f / (100f + CurrentDefense));
        damage = Mathf.Max(damage, 1);
        _CurrentHealth -= damage;

        /*if (DamageDisplay != null)
        {
            if (isCritical)
                DamageDisplay.ShowCritical(damage, this);
            else
                DamageDisplay.ShowDigits(damage, this);
        }*/

        // 기습 금지 플래그 세팅
        canBackAttack = false;

        // DamageDisplay 표시 끝날 때까지 기다리는 코루틴 실행
        StartCoroutine(WaitUntilDamageDisplayOff());

        if (_CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            TryCounterAttack();
        }
    }

    private IEnumerator WaitUntilDamageDisplayOff()
    {
        yield return new WaitUntil(() =>
            DamageDisplay.DigitParent != null && DamageDisplay.CriticalParent != null && DamageDisplay.MissParent != null &&
            !DamageDisplay.DigitParent.activeSelf &&
            !DamageDisplay.CriticalParent.activeSelf &&
            !DamageDisplay.MissParent.activeSelf);

        // Damage 표시 오브젝트 모두 꺼졌으면 기습 허용
        canBackAttack = true;
    }


 /*public void ExecuteAmbushAttack()
    {
        if (IsDead || CharacterInfoUI.Instance == null || CharacterInfoUI.Instance.CurrentHealth <= 0)
            return;

        // 기습이 허용되지 않으면 실행 안 함
        //if (!canBackAttack || isCounterAttacking)
          //  return;

        // 기습 중 적 캐릭터가 보이지 않게 설정
    if (EnemyCharacter.activeSelf)
        EnemyCharacter.SetActive(false);

    if (!canBackAttack || WillCounterAttack)
        return;

        int rawDamage = Mathf.FloorToInt(_currentAttackPower * 0.3f);
        int finalDamage = CalculateFinalDamage(rawDamage, CharacterInfoUI.Instance.CurrentDefense);

        ExecuteAttackAnimation(() =>
        {
            if (finalDamage <= 0)
            {
                DamageDisplay?.ShowEnemyPlayerMiss(this);
            }
            else
            {
                CharacterManager.Instance.DecreaseHealth(finalDamage, this);
                DamageDisplay?.ShowPlayerDamage(finalDamage, this, true); // 기습이므로 true 전달
                Debug.Log("기습 시작");

                //DamageDisplay?.ShowPlayerDamage(finalDamage, this);
            }

            Debug.Log($"[기습] {gameObject.name} | {GetWeaponName_KR()} | 데미지: {finalDamage}");
        });
    }*/
    public void ExecuteAmbushAttack()
{
    if (IsDead || CharacterInfoUI.Instance == null || CharacterInfoUI.Instance.CurrentHealth <= 0)
        return;

    // 기습 중 적 캐릭터가 보이지 않게 설정
    if (EnemyCharacter.activeSelf)
        EnemyCharacter.SetActive(false);

    // 기습이 허용되지 않거나 반격 예정이면 실행 안 함
    if (!canBackAttack || WillCounterAttack)
        return;

    int rawDamage = Mathf.FloorToInt(_currentAttackPower * 0.3f);
    int finalDamage = CalculateFinalDamage(rawDamage, CharacterInfoUI.Instance.CurrentDefense);

    ExecuteAttackAnimation(() =>
    {
        /*if (finalDamage <= 0)
        {
            DamageDisplay?.ShowEnemyPlayerMiss(this);
        }*/
        if (finalDamage <= 0)
        {
            DamageDisplay?.ShowEnemyPlayerMiss(this, true); // 기습 실패
        }
        else
        {
            CharacterManager.Instance.DecreaseHealth(finalDamage, this);
            DamageDisplay?.ShowPlayerDamage(finalDamage, this, true); // 기습이므로 true 전달
            Debug.Log("기습 시작");
        }

        Debug.Log($"[기습] {gameObject.name} | {GetWeaponName_KR()} | 데미지: {finalDamage}");
    });
}





/* public void TryCounterAttack()
    {
        if (!CanCounterAttack()) return;

        _isCounterAttackScheduled = true;
        WillCounterAttack = true; // 반격 중임 표시

        DOVirtual.DelayedCall(CounterAttackDelay, () =>
        {
            _isCounterAttackScheduled = false;

            if (IsDead || CharacterInfoUI.Instance.CurrentHealth <= 0)
            {
                WillCounterAttack = false;
                return;
            }

            EnemyCharacter?.SetActive(true);
            ItemSearchManager.Instance?.SearchButton.gameObject.SetActive(false);

            ExecuteAttackAnimation(() =>
            {
                bool isHit = UnityEngine.Random.value < HitChance;
                if (isHit)
                    ApplyCounterAttackDamage();
                else
                    DamageDisplay?.ShowEnemyPlayerMiss(this);

                WillCounterAttack = false; // 반격 종료
            });
        });
    }*/
    public void TryCounterAttack()
{
    if (!CanCounterAttack()) return;

    _isCounterAttackScheduled = true;
    WillCounterAttack = true; // 반격 중임 표시

    DOVirtual.DelayedCall(CounterAttackDelay, () =>
    {
        _isCounterAttackScheduled = false;

        if (IsDead || CharacterInfoUI.Instance.CurrentHealth <= 0)
        {
            WillCounterAttack = false;
            return;
        }

        EnemyCharacter?.SetActive(true);
        ItemSearchManager.Instance?.SearchButton.gameObject.SetActive(false);

        ExecuteAttackAnimation(() =>
        {
            bool isHit = UnityEngine.Random.value < HitChance;
            if (isHit)
                ApplyCounterAttackDamage();
            else
                DamageDisplay?.ShowEnemyPlayerMiss(this, false); // 반격 실패

            WillCounterAttack = false; // 반격 종료
        });
    });
}


private bool CanCounterAttack()
{
    bool result = WillCounterAttack &&
                  !_isCounterAttackScheduled &&
                  !IsDead &&
                  CharacterInfoUI.Instance != null &&
                  CharacterInfoUI.Instance.CurrentHealth > 0 &&
                  UnityEngine.Random.value < CounterChance;

    Debug.Log($"CanCounterAttack: {result}, WillCounterAttack: {WillCounterAttack}, IsDead: {IsDead}, CharacterInfoUI.Instance: {CharacterInfoUI.Instance != null}, PlayerHealth: {(CharacterInfoUI.Instance != null ? CharacterInfoUI.Instance.CurrentHealth : -1)}");

    return result;
}

private void ApplyCounterAttackDamage()
{
    int rawDamage = Mathf.FloorToInt(_currentAttackPower * CounterDamageMultiplier);
    int finalDamage = CalculateFinalDamage(rawDamage, CharacterInfoUI.Instance.CurrentDefense);

    /*if (finalDamage <= 0)
        DamageDisplay?.ShowEnemyPlayerMiss(this);*/
    if (finalDamage <= 0)
    {
         DamageDisplay?.ShowEnemyPlayerMiss(this, false); // 반격 실패
    }
   
    else
    {
        CharacterManager.Instance.DecreaseHealth(finalDamage, this);
        DamageDisplay?.ShowPlayerDamage(finalDamage, this);
    }

    Debug.Log($"[반격] {gameObject.name} | {GetWeaponName_KR()} | 데미지: {finalDamage}");
 
}




    private void ExecuteAttackAnimation(System.Action onAnimationPeak)
    {
        if (EnemyCharacter == null)
        {
            onAnimationPeak?.Invoke();
            return;
        }

        Transform tf = EnemyCharacter.transform;
        Vector3 original = tf.localScale;

        tf.DOScale(original * 1.15f, 0.1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                onAnimationPeak?.Invoke();
                tf.DOScale(original, 0.2f).SetEase(Ease.InBack);
            });
    }

    private void Die()
    {
        Debug.Log($"[적 사망] {gameObject.name}");

        if (_enemyMoveManager != null)
            _enemyMoveManager.HandleDeath();
        else
            Debug.LogWarning($"[사망 실패] {gameObject.name}에 EnemyMoveManager 없음!");
    }

    public int GetBaseAttackPower()
    {
        if (EnemyStat == null) return 0;

        return EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => EnemyStat.Magic,
            WeaponType.Sword => EnemyStat.Sword,
            WeaponType.Blunt => EnemyStat.Blunt,
            WeaponType.Fist => EnemyStat.Fist,
            WeaponType.Bow => EnemyStat.Bow,
            WeaponType.Throwing => EnemyStat.Throwing,
            WeaponType.Gun => EnemyStat.Gun,
            _ => 0
        };
    }

    private int CalculateFinalDamage(int rawDamage, int defense)
    {
        int final = Mathf.RoundToInt(rawDamage * 100f / (100f + defense));
        return Mathf.Max(final, 1);
    }

    private string GetWeaponName_KR()
    {
        return EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => "마법",
            WeaponType.Sword => "검",
            WeaponType.Blunt => "둔기",
            WeaponType.Fist => "주먹",
            WeaponType.Bow => "활",
            WeaponType.Throwing => "던지기",
            WeaponType.Gun => "총",
            _ => "알 수 없음"
        };
    }
}

/*using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("스탯 설정")]
    public CharacterStatSO EnemyStat;

    [Header("데미지 표시")]
    public DamageDisplay DamageDisplay;

    [SerializeField] private int _MaxHealth;
    [SerializeField] private int _CurrentHealth; // 현재 체력 (인스펙터에서 보기용)

    public int CurrentDefense;
    [SerializeField] private int _currentAttackPower;

    [Header("화면에 보일 적 캐릭터")]
    public GameObject EnemyCharacter;

    // ✅ EnemyMoveManager 참조 추가
    private EnemyMoveManager _enemyMoveManager;

    private bool _isCounterAttackScheduled = false;

    public bool WillCounterAttack; // 이걸로 반격 여부 판단

    private bool _hasBeenAttackedByPlayer = false;  // 플레이어 공격받았는지 체크




    // ✅ 죽음 상태를 확인하는 프로퍼티 추가
    public bool IsDead 
    { 
        get 
        { 
            if (_enemyMoveManager == null)
                _enemyMoveManager = GetComponent<EnemyMoveManager>();
            
            return _enemyMoveManager != null && _enemyMoveManager.IsDead;
        } 
    }

    private void Start()
    {
        if (EnemyStat == null)
        {
            Debug.LogError("CharacterStatSO가 할당되지 않았습니다!");
            return;
        }

        // ✅ EnemyMoveManager 참조 가져오기
        _enemyMoveManager = GetComponent<EnemyMoveManager>();
        if (_enemyMoveManager == null)
        {
            Debug.LogWarning($"{gameObject.name}에 EnemyMoveManager가 없습니다!");
        }

        _MaxHealth = EnemyStat.MaxHealth;
        _CurrentHealth = _MaxHealth;
        CurrentDefense = EnemyStat.Defense; // 방어력 설정
        _currentAttackPower = GetBaseAttackPower();
        // Debug.Log($"[적 초기화] 체력: {_MaxHealth} | 방어력: {CurrentDefense} | 공격력: {_currentAttackPower} | {GetCurrentWeaponSkillInfo_KR()}");
    }

    public void TakeDamage(int baseDamage, bool isCritical = false, bool isMiss = false)
    {
        // 인터페이스에서 요구한 메서드 → 기존처럼 작동하게
        TakeDamage(baseDamage, null, isCritical, isMiss);
    }

    public void TakeDamage(int baseDamage, PlayerAttack attacker, bool isCritical = false, bool isMiss = false)
    {
        // ✅ 이미 죽은 적에게는 데미지를 주지 않음
        if (IsDead)
        {
            Debug.Log($"[Enemy] {gameObject.name}은 이미 죽어있어서 데미지를 받지 않습니다.");
            return;
        }

        // 기존 데미지 처리 코드
        int damage = Mathf.RoundToInt(baseDamage * 100f / (100f + CurrentDefense));
        damage = Mathf.Max(damage, 1);
        _CurrentHealth -= damage;

        if (DamageDisplay != null)
        {
            if (isCritical)
                DamageDisplay.ShowCritical(damage, this);
            else
                DamageDisplay.ShowDigits(damage, this);
        }

        if (_CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            // ✅ 공격받았을 때 플레이어가 살아 있으면 반격 시도
            TryCounterAttack();
        }
    }

    // 기습 공격 메서드 추가
    public void ExecuteAmbushAttack()
    {
        // ✅ 죽은 적은 기습 공격 불가
        if (IsDead)
        {
            Debug.Log($"[기습 공격] {gameObject.name}은 죽어있어서 기습 공격할 수 없습니다.");
            return;
        }

        Debug.Log($"[기습 공격] {gameObject.name}이 플레이어를 기습 공격!");

        // 플레이어 생존 확인
        if (CharacterInfoUI.Instance == null || CharacterInfoUI.Instance.CurrentHealth <= 0)
        {
            Debug.Log("[기습 공격] 플레이어가 사망 상태 → 기습 취소");
            return;
        }


        if (ItemSearchManager.Instance != null)
        {
            ItemSearchManager.Instance.SearchButton.gameObject.SetActive(false);
        }

        // 현재 적용된 무기 타입 정보 가져오기
        string weaponKR = EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => "마법",
            WeaponType.Sword => "검",
            WeaponType.Blunt => "둔기",
            WeaponType.Fist => "주먹",
            WeaponType.Bow => "활",
            WeaponType.Throwing => "던지기",
            WeaponType.Gun => "총",
            _ => "알 수 없음"
        };

        // 기습 공격은 반격보다 강함 (반격 0.5배 → 기습 0.8배)
        int rawAmbushDamage = Mathf.FloorToInt(_currentAttackPower * 0.3f);
        int playerDefense = CharacterInfoUI.Instance.CurrentDefense;

        int finalDamage = Mathf.RoundToInt(rawAmbushDamage * 100f / (100f + playerDefense));
        finalDamage = Mathf.Max(finalDamage, 1);

        Debug.Log($"[기습 공격] {gameObject.name} | 무기 종류: {weaponKR} | 기본 공격력: {_currentAttackPower} | 기습 데미지: {rawAmbushDamage} | 플레이어 방어력: {playerDefense} | 최종 데미지: {finalDamage}");

        // ✅ 기습 공격 애니메이션과 함께 데미지 표시
        ExecuteAttackAnimation(() => {
            if (finalDamage <= 0)
            {
                Debug.Log("[기습 공격] 최종 데미지 0 → 미스 처리");
                if (DamageDisplay != null)
                {
                    DamageDisplay.ShowEnemyPlayerMiss(this);
                }
            }
            else
            {
                // 플레이어에게 데미지 적용
                CharacterManager.Instance.DecreaseHealth(finalDamage, this);

                // 데미지 표시
                if (DamageDisplay != null)
                {
                    DamageDisplay.ShowPlayerDamage(finalDamage, this);
                }

                Debug.Log($"[기습 성공] {gameObject.name}가 {weaponKR}으로 {finalDamage} 데미지 기습 공격 완료");
            }
        });
    }

    public void TryCounterAttack()
{
    if (!WillCounterAttack)
        return;

    
    if (IsDead)
    {
        Debug.Log($"[Enemy] {gameObject.name}은 죽어있어서 반격할 수 없습니다.");
        return;
    }

    if (_isCounterAttackScheduled)
    {
        Debug.Log($"[Enemy 반격] 이미 반격이 예약되어 있음 → 중복 방지");
        return;
    }

    float counterChance = 1.0f;//0.75
    float hitChance = 0.85f;

    if (CharacterInfoUI.Instance.CurrentHealth <= 0)
        return;

    if (Random.value < counterChance)
    {
        Debug.Log("[Enemy 반격] 1.2초 후 반격 예정!");
        _isCounterAttackScheduled = true; // ✅ 예약 상태로 설정

        DOVirtual.DelayedCall(1.2f, () =>
        {
            _isCounterAttackScheduled = false; // ✅ 실행 이후 상태 초기화

            if (IsDead || CharacterInfoUI.Instance.CurrentHealth <= 0)
            {
                Debug.Log("[Enemy 반격] 반격 실행 시점에 상태 변화로 반격 취소");
                return;
            }

            if (EnemyCharacter != null)
            {
                EnemyCharacter.SetActive(true);
            }
            if (ItemSearchManager.Instance != null)
            {
                ItemSearchManager.Instance.SearchButton.gameObject.SetActive(false);
            }

            string weaponKR = EnemyStat.DefaultWeaponType switch
            {
                WeaponType.Magic => "마법",
                WeaponType.Sword => "검",
                WeaponType.Blunt => "둔기",
                WeaponType.Fist => "주먹",
                WeaponType.Bow => "활",
                WeaponType.Throwing => "던지기",
                WeaponType.Gun => "총",
                _ => "알 수 없음"
            };

            if (Random.value < hitChance)
            {
                int rawDamage = Mathf.FloorToInt(_currentAttackPower * 0.5f);
                int playerDefense = CharacterInfoUI.Instance.CurrentDefense;
                int finalDamage = Mathf.RoundToInt(rawDamage * 100f / (100f + playerDefense));
                finalDamage = Mathf.Max(finalDamage, 1);

                ExecuteAttackAnimation(() =>
                {
                    if (finalDamage <= 0)
                    {
                        Debug.LogWarning("[Enemy 반격] 데미지 0 → 미스 처리");
                        DamageDisplay?.ShowEnemyPlayerMiss(this);
                    }
                    else
                    {
                        CharacterManager.Instance.DecreaseHealth(finalDamage, this);
                        DamageDisplay?.ShowPlayerDamage(finalDamage, this);
                    }
                });
            }
            else
            {
                Debug.Log("[Enemy 반격] 반격 Miss!");
                ExecuteAttackAnimation(() =>
                {
                    DamageDisplay?.ShowEnemyPlayerMiss(this);
                });
            }
        });
    }
    else
    {
        Debug.Log("[Enemy 반격] 반격 시도하지 않음 (25% 확률로 반격 안함)");
    }
}


    // ✅ 새로운 공격 애니메이션 메서드 추가
    private void ExecuteAttackAnimation(System.Action onAnimationPeak)
    {
        if (EnemyCharacter == null)
        {
            onAnimationPeak?.Invoke();
            return;
        }

        Transform enemyTransform = EnemyCharacter.transform;
        Vector3 originalScale = enemyTransform.localScale;

        // 캐릭터가 커지면서 공격 애니메이션
        enemyTransform.DOScale(originalScale * 1.3f, 0.1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                // ✅ 캐릭터가 최대 크기가 된 순간에 데미지/미스 표시
                onAnimationPeak?.Invoke();
                
                // 원래 크기로 돌아오기
                enemyTransform.DOScale(originalScale, 0.2f)
                    .SetEase(Ease.InBack);
            });
    }

    private void Die()
    {
        Debug.Log("[적 사망] 적이 쓰러졌습니다!");
        
        // ✅ EnemyMoveManager에게 사망 알림
        if (_enemyMoveManager != null)
        {
            _enemyMoveManager.HandleDeath();
        }
        else
        {
            Debug.LogWarning($"[적 사망] {gameObject.name}에 EnemyMoveManager가 없어서 사망 처리를 완전히 할 수 없습니다!");
        }

        //TimeFlowManager.Instance.PlayerCount.text = "1";
        // Destroy(gameObject);
    }

    public int GetBaseAttackPower()
    {
        if (EnemyStat == null) return 0;

        return EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => EnemyStat.Magic,
            WeaponType.Sword => EnemyStat.Sword,
            WeaponType.Blunt => EnemyStat.Blunt,
            WeaponType.Fist => EnemyStat.Fist,
            WeaponType.Bow => EnemyStat.Bow,
            WeaponType.Throwing => EnemyStat.Throwing,
            WeaponType.Gun => EnemyStat.Gun,
            _ => 0
        };
    }
}

/*using UnityEngine;
using DG.Tweening;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("스탯 설정")]
    public CharacterStatSO EnemyStat;

    [Header("데미지 표시")]
    public DamageDisplay DamageDisplay;

    [SerializeField] private int _MaxHealth;
    [SerializeField] private int _CurrentHealth; // 현재 체력 (인스펙터에서 보기용)

    public int CurrentDefense;
    [SerializeField] private int _currentAttackPower;

    [Header("화면에 보일 적 캐릭터")]
    public GameObject EnemyCharacter;

    // ✅ EnemyMoveManager 참조 추가
    private EnemyMoveManager _enemyMoveManager;

    // ✅ 죽음 상태를 확인하는 프로퍼티 추가
    public bool IsDead 
    { 
        get 
        { 
            if (_enemyMoveManager == null)
                _enemyMoveManager = GetComponent<EnemyMoveManager>();
            
            return _enemyMoveManager != null && _enemyMoveManager.IsDead;
        } 
    }

    private void Start()
    {
        if (EnemyStat == null)
        {
            Debug.LogError("CharacterStatSO가 할당되지 않았습니다!");
            return;
        }

        // ✅ EnemyMoveManager 참조 가져오기
        _enemyMoveManager = GetComponent<EnemyMoveManager>();
        if (_enemyMoveManager == null)
        {
            Debug.LogWarning($"{gameObject.name}에 EnemyMoveManager가 없습니다!");
        }

        _MaxHealth = EnemyStat.MaxHealth;
        _CurrentHealth = _MaxHealth;
        CurrentDefense = EnemyStat.Defense; // 방어력 설정
        _currentAttackPower = GetBaseAttackPower();
        // Debug.Log($"[적 초기화] 체력: {_MaxHealth} | 방어력: {CurrentDefense} | 공격력: {_currentAttackPower} | {GetCurrentWeaponSkillInfo_KR()}");
    }

    public void TakeDamage(int baseDamage, bool isCritical = false, bool isMiss = false)
    {
        // 인터페이스에서 요구한 메서드 → 기존처럼 작동하게
        TakeDamage(baseDamage, null, isCritical, isMiss);
    }

    public void TakeDamage(int baseDamage, PlayerAttack attacker, bool isCritical = false, bool isMiss = false)
    {
        // ✅ 이미 죽은 적에게는 데미지를 주지 않음
        if (IsDead)
        {
            Debug.Log($"[Enemy] {gameObject.name}은 이미 죽어있어서 데미지를 받지 않습니다.");
            return;
        }

        // 기존 데미지 처리 코드
        int damage = Mathf.RoundToInt(baseDamage * 100f / (100f + CurrentDefense));
        damage = Mathf.Max(damage, 1);
        _CurrentHealth -= damage;

        if (DamageDisplay != null)
        {
            if (isCritical)
                DamageDisplay.ShowCritical(damage, this);
            else
                DamageDisplay.ShowDigits(damage, this);
        }

        if (_CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            // ✅ 공격받았을 때 플레이어가 살아 있으면 반격 시도
            TryCounterAttack();
        }
    }

    // 기습 공격 메서드 추가
    public void ExecuteAmbushAttack()
    {
        // ✅ 죽은 적은 기습 공격 불가
        if (IsDead)
        {
            Debug.Log($"[기습 공격] {gameObject.name}은 죽어있어서 기습 공격할 수 없습니다.");
            return;
        }

        Debug.Log($"[기습 공격] {gameObject.name}이 플레이어를 기습 공격!");

        // 플레이어 생존 확인
        if (CharacterInfoUI.Instance == null || CharacterInfoUI.Instance.CurrentHealth <= 0)
        {
            Debug.Log("[기습 공격] 플레이어가 사망 상태 → 기습 취소");
            return;
        }

        // ✅ 기습 공격 시에도 캐릭터 상태 설정
        if (EnemyCharacter != null)
        {
            EnemyCharacter.SetActive(true);
        }
        if (ItemSearchManager.Instance != null)
        {
            ItemSearchManager.Instance.SearchButton.gameObject.SetActive(false);
        }

        // 현재 적용된 무기 타입 정보 가져오기
        string weaponKR = EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => "마법",
            WeaponType.Sword => "검",
            WeaponType.Blunt => "둔기",
            WeaponType.Fist => "주먹",
            WeaponType.Bow => "활",
            WeaponType.Throwing => "던지기",
            WeaponType.Gun => "총",
            _ => "알 수 없음"
        };

        // 기습 공격은 반격보다 강함 (반격 0.5배 → 기습 0.8배)
        int rawAmbushDamage = Mathf.FloorToInt(_currentAttackPower * 0.8f);
        int playerDefense = CharacterInfoUI.Instance.CurrentDefense;

        int finalDamage = Mathf.RoundToInt(rawAmbushDamage * 100f / (100f + playerDefense));
        finalDamage = Mathf.Max(finalDamage, 1);

        Debug.Log($"[기습 공격] {gameObject.name} | 무기 종류: {weaponKR} | 기본 공격력: {_currentAttackPower} | 기습 데미지: {rawAmbushDamage} | 플레이어 방어력: {playerDefense} | 최종 데미지: {finalDamage}");

        // ✅ 기습 공격 애니메이션과 함께 데미지 표시
        ExecuteAttackAnimation(() => {
            if (finalDamage <= 0)
            {
                Debug.Log("[기습 공격] 최종 데미지 0 → 미스 처리");
                if (DamageDisplay != null)
                {
                    DamageDisplay.ShowEnemyPlayerMiss(this);
                }
            }
            else
            {
                // 플레이어에게 데미지 적용
                CharacterManager.Instance.DecreaseHealth(finalDamage, this);

                // 데미지 표시
                if (DamageDisplay != null)
                {
                    DamageDisplay.ShowPlayerDamage(finalDamage, this);
                }

                Debug.Log($"[기습 성공] {gameObject.name}가 {weaponKR}으로 {finalDamage} 데미지 기습 공격 완료");
            }
        });
    }

    //반격
    public void TryCounterAttack()
    {
        // ✅ 죽은 적은 반격 불가
        if (IsDead)
        {
            Debug.Log($"[Enemy] {gameObject.name}은 죽어있어서 반격할 수 없습니다.");
            return;
        }

        float counterChance = 0.75f;
        float hitChance = 0.85f;

        if (CharacterInfoUI.Instance.CurrentHealth <= 0)
            return;

        if (Random.value < counterChance)
        {
            Debug.Log("[Enemy 반격] 반격 시도!");

            // ✅ 반격 시 캐릭터 상태 설정
            if (EnemyCharacter != null)
            {
                EnemyCharacter.SetActive(true);
            }
            if (ItemSearchManager.Instance != null)
            {
                ItemSearchManager.Instance.SearchButton.gameObject.SetActive(false);
            }

            // 반격 시도 무기 정보
            string weaponKR = EnemyStat.DefaultWeaponType switch
            {
                WeaponType.Magic => "마법",
                WeaponType.Sword => "검",
                WeaponType.Blunt => "둔기",
                WeaponType.Fist => "주먹",
                WeaponType.Bow => "활",
                WeaponType.Throwing => "던지기",
                WeaponType.Gun => "총",
                _ => "알 수 없음"
            };

            if (Random.value < hitChance) // 명중
            {
                int rawDamage = Mathf.FloorToInt(_currentAttackPower * 0.5f);
                int playerDefense = CharacterInfoUI.Instance.CurrentDefense;

                int finalDamage = Mathf.RoundToInt(rawDamage * 100f / (100f + playerDefense));
                finalDamage = Mathf.Max(finalDamage, 1);

                Debug.Log($"[Enemy 반격] 무기 종류: {weaponKR} | 기본 공격력: {_currentAttackPower} | 반격 데미지: {rawDamage} | 플레이어 방어력: {playerDefense} | 최종 데미지: {finalDamage}");

                // ✅ 반격 애니메이션과 함께 데미지 표시
                ExecuteAttackAnimation(() => {
                    if (finalDamage <= 0)
                    {
                        Debug.Log("[Enemy 반격] 데미지 0 → 미스 처리");
                        if (DamageDisplay != null)
                        {
                            DamageDisplay.ShowEnemyPlayerMiss(this);
                        }
                    }
                    else
                    {
                        CharacterManager.Instance.DecreaseHealth(finalDamage, this);
                        if (DamageDisplay != null)
                        {
                            DamageDisplay.ShowPlayerDamage(finalDamage, this);
                        }
                    }
                });
            }
            else
            {
                Debug.Log("[Enemy 반격] 반격 Miss!");
                
                // ✅ 미스 애니메이션과 함께 미스 표시
                ExecuteAttackAnimation(() => {
                    if (DamageDisplay != null)
                    {
                        DamageDisplay.ShowEnemyPlayerMiss(this);
                    }
                });
            }
        }
        else
        {
            Debug.Log("[Enemy 반격] 반격 시도하지 않음 (25% 확률로 반격 안함)");
            // ✅ 반격하지 않을 때는 기존 DamageDisplay 로직이 캐릭터를 숨김
        }
    }

    // ✅ 새로운 공격 애니메이션 메서드 추가
    private void ExecuteAttackAnimation(System.Action onAnimationPeak)
    {
        if (EnemyCharacter == null)
        {
            onAnimationPeak?.Invoke();
            return;
        }

        Transform enemyTransform = EnemyCharacter.transform;
        Vector3 originalScale = enemyTransform.localScale;

        // 캐릭터가 커지면서 공격 애니메이션
        enemyTransform.DOScale(originalScale * 1.3f, 0.1f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                // ✅ 캐릭터가 최대 크기가 된 순간에 데미지/미스 표시
                onAnimationPeak?.Invoke();
                
                // 원래 크기로 돌아오기
                enemyTransform.DOScale(originalScale, 0.2f)
                    .SetEase(Ease.InBack);
            });
    }

    private void Die()
    {
        Debug.Log("[적 사망] 적이 쓰러졌습니다!");
        
        // ✅ EnemyMoveManager에게 사망 알림
        if (_enemyMoveManager != null)
        {
            _enemyMoveManager.HandleDeath();
        }
        else
        {
            Debug.LogWarning($"[적 사망] {gameObject.name}에 EnemyMoveManager가 없어서 사망 처리를 완전히 할 수 없습니다!");
        }

        //TimeFlowManager.Instance.PlayerCount.text = "1";
        // Destroy(gameObject);
    }

    public int GetBaseAttackPower()
    {
        if (EnemyStat == null) return 0;

        return EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => EnemyStat.Magic,
            WeaponType.Sword => EnemyStat.Sword,
            WeaponType.Blunt => EnemyStat.Blunt,
            WeaponType.Fist => EnemyStat.Fist,
            WeaponType.Bow => EnemyStat.Bow,
            WeaponType.Throwing => EnemyStat.Throwing,
            WeaponType.Gun => EnemyStat.Gun,
            _ => 0
        };
    }
}*/
/*using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("스탯 설정")]
    public CharacterStatSO EnemyStat;

    [Header("데미지 표시")]
    public DamageDisplay DamageDisplay;

    [SerializeField] private int _MaxHealth;
    [SerializeField] private int _CurrentHealth; // 현재 체력 (인스펙터에서 보기용)

    public int CurrentDefense;
    [SerializeField] private int _currentAttackPower;

    [Header("화면에 보일 적 캐릭터")]
    public GameObject EnemyCharacter;

    // ✅ EnemyMoveManager 참조 추가
    private EnemyMoveManager _enemyMoveManager;

    // ✅ 죽음 상태를 확인하는 프로퍼티 추가
    public bool IsDead 
    { 
        get 
        { 
            if (_enemyMoveManager == null)
                _enemyMoveManager = GetComponent<EnemyMoveManager>();
            
            return _enemyMoveManager != null && _enemyMoveManager.IsDead;
        } 
    }

    private void Start()
    {
        if (EnemyStat == null)
        {
            Debug.LogError("CharacterStatSO가 할당되지 않았습니다!");
            return;
        }

        // ✅ EnemyMoveManager 참조 가져오기
        _enemyMoveManager = GetComponent<EnemyMoveManager>();
        if (_enemyMoveManager == null)
        {
            Debug.LogWarning($"{gameObject.name}에 EnemyMoveManager가 없습니다!");
        }

        _MaxHealth = EnemyStat.MaxHealth;
        _CurrentHealth = _MaxHealth;
        CurrentDefense = EnemyStat.Defense; // 방어력 설정
        _currentAttackPower = GetBaseAttackPower();
        // Debug.Log($"[적 초기화] 체력: {_MaxHealth} | 방어력: {CurrentDefense} | 공격력: {_currentAttackPower} | {GetCurrentWeaponSkillInfo_KR()}");
    }

    public void TakeDamage(int baseDamage, bool isCritical = false, bool isMiss = false)
    {
        // 인터페이스에서 요구한 메서드 → 기존처럼 작동하게
        TakeDamage(baseDamage, null, isCritical, isMiss);
    }

    public void TakeDamage(int baseDamage, PlayerAttack attacker, bool isCritical = false, bool isMiss = false)
    {
        // ✅ 이미 죽은 적에게는 데미지를 주지 않음
        if (IsDead)
        {
            Debug.Log($"[Enemy] {gameObject.name}은 이미 죽어있어서 데미지를 받지 않습니다.");
            return;
        }

        // 기존 데미지 처리 코드
        int damage = Mathf.RoundToInt(baseDamage * 100f / (100f + CurrentDefense));
        damage = Mathf.Max(damage, 1);
        _CurrentHealth -= damage;

        if (DamageDisplay != null)
        {
            if (isCritical)
                DamageDisplay.ShowCritical(damage, this);
            else
                DamageDisplay.ShowDigits(damage, this);
        }

        if (_CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 공격받았을 때 플레이어가 살아 있으면 반격 시도
            TryCounterAttack();
        }
    }

    // 기습 공격 메서드 추가
    public void ExecuteAmbushAttack()
    {
        // ✅ 죽은 적은 기습 공격 불가
        if (IsDead)
        {
            Debug.Log($"[기습 공격] {gameObject.name}은 죽어있어서 기습 공격할 수 없습니다.");
            return;
        }

        Debug.Log($"[기습 공격] {gameObject.name}이 플레이어를 기습 공격!");

        // 플레이어 생존 확인
        if (CharacterInfoUI.Instance == null || CharacterInfoUI.Instance.CurrentHealth <= 0)
        {
            Debug.Log("[기습 공격] 플레이어가 사망 상태 → 기습 취소");
            return;
        }

        // 현재 적용된 무기 타입 정보 가져오기
        string weaponKR = EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => "마법",
            WeaponType.Sword => "검",
            WeaponType.Blunt => "둔기",
            WeaponType.Fist => "주먹",
            WeaponType.Bow => "활",
            WeaponType.Throwing => "던지기",
            WeaponType.Gun => "총",
            _ => "알 수 없음"
        };

        // 기습 공격은 반격보다 강함 (반격 0.5배 → 기습 0.8배)
        int rawAmbushDamage = Mathf.FloorToInt(_currentAttackPower * 0.8f);
        int playerDefense = CharacterInfoUI.Instance.CurrentDefense;

        int finalDamage = Mathf.RoundToInt(rawAmbushDamage * 100f / (100f + playerDefense));
        finalDamage = Mathf.Max(finalDamage, 1);

        Debug.Log($"[기습 공격] {gameObject.name} | 무기 종류: {weaponKR} | 기본 공격력: {_currentAttackPower} | 기습 데미지: {rawAmbushDamage} | 플레이어 방어력: {playerDefense} | 최종 데미지: {finalDamage}");

        // 기습은 확정 명중 (기습의 특성상)
        if (finalDamage <= 0)
        {
            Debug.Log("[기습 공격] 최종 데미지 0 → 미스 처리");
            DamageDisplay.ShowEnemyPlayerMiss(this);
            return;
        }

        // 플레이어에게 데미지 적용
        CharacterManager.Instance.DecreaseHealth(finalDamage, this);

        // 데미지 표시 (기습은 특별한 표시가 있다면 추가 가능)
        if (DamageDisplay != null)
        {
            DamageDisplay.ShowPlayerDamage(finalDamage, this);
        }

        Debug.Log($"[기습 성공] {gameObject.name}가 {weaponKR}으로 {finalDamage} 데미지 기습 공격 완료");
    }

    //반격
    public void TryCounterAttack()
    {
        // ✅ 죽은 적은 반격 불가
        if (IsDead)
        {
            Debug.Log($"[Enemy] {gameObject.name}은 죽어있어서 반격할 수 없습니다.");
            return;
        }

        float counterChance = 0.75f;
        float hitChance = 0.85f;

        if (CharacterInfoUI.Instance.CurrentHealth <= 0)
            return;

        if (Random.value < counterChance)
        {
            // 반격 시도 무기 정보
            string weaponKR = EnemyStat.DefaultWeaponType switch
            {
                WeaponType.Magic => "마법",
                WeaponType.Sword => "검",
                WeaponType.Blunt => "둔기",
                WeaponType.Fist => "주먹",
                WeaponType.Bow => "활",
                WeaponType.Throwing => "던지기",
                WeaponType.Gun => "총",
                _ => "알 수 없음"
            };

            if (Random.value < hitChance) // 명중
            {
                int rawDamage = Mathf.FloorToInt(_currentAttackPower * 0.5f);
                int playerDefense = CharacterInfoUI.Instance.CurrentDefense;

                int finalDamage = Mathf.RoundToInt(rawDamage * 100f / (100f + playerDefense));
                finalDamage = Mathf.Max(finalDamage, 1);

                // 디버그 로그 추가
                Debug.Log($"[Enemy 반격] 무기 종류: {weaponKR} | 기본 공격력: {_currentAttackPower} | 반격 데미지: {rawDamage} | 플레이어 방어력: {playerDefense} | 최종 데미지: {finalDamage}");

                if (finalDamage <= 0)
                {
                    Debug.Log("[Enemy 반격] 데미지 0 → 미스 처리");
                    DamageDisplay.ShowEnemyPlayerMiss(this);
                    return;
                }

                CharacterManager.Instance.DecreaseHealth(finalDamage, this);
                DamageDisplay.ShowPlayerDamage(finalDamage, this);
            }
            else
            {
                Debug.Log("[Enemy 반격] 반격 Miss!");
                DamageDisplay.ShowEnemyPlayerMiss(this);
            }
        }
    }

    private void Die()
    {
        Debug.Log("[적 사망] 적이 쓰러졌습니다!");
        
        // ✅ EnemyMoveManager에게 사망 알림
        if (_enemyMoveManager != null)
        {
            _enemyMoveManager.HandleDeath();
        }
        else
        {
            Debug.LogWarning($"[적 사망] {gameObject.name}에 EnemyMoveManager가 없어서 사망 처리를 완전히 할 수 없습니다!");
        }

        //TimeFlowManager.Instance.PlayerCount.text = "1";
        // Destroy(gameObject);
    }

    public int GetBaseAttackPower()
    {
        if (EnemyStat == null) return 0;

        return EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => EnemyStat.Magic,
            WeaponType.Sword => EnemyStat.Sword,
            WeaponType.Blunt => EnemyStat.Blunt,
            WeaponType.Fist => EnemyStat.Fist,
            WeaponType.Bow => EnemyStat.Bow,
            WeaponType.Throwing => EnemyStat.Throwing,
            WeaponType.Gun => EnemyStat.Gun,
            _ => 0
        };
    }
}*/

/*using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("스탯 설정")]
    public CharacterStatSO EnemyStat;

    [Header("데미지 표시")]
    public DamageDisplay DamageDisplay;

    [SerializeField] private int _MaxHealth;
    [SerializeField] private int _CurrentHealth; // 현재 체력 (인스펙터에서 보기용)

    public int CurrentDefense;
    [SerializeField] private int _currentAttackPower;

    [Header("화면에 보일 적 캐릭터")]
    public GameObject EnemyCharacter;

    private void Start()
    {
        if (EnemyStat == null)
        {
            Debug.LogError("CharacterStatSO가 할당되지 않았습니다!");
            return;
        }

        _MaxHealth = EnemyStat.MaxHealth;
        _CurrentHealth = _MaxHealth;
        CurrentDefense = EnemyStat.Defense; // 방어력 설정
        _currentAttackPower = GetBaseAttackPower();
        // Debug.Log($"[적 초기화] 체력: {_MaxHealth} | 방어력: {CurrentDefense} | 공격력: {_currentAttackPower} | {GetCurrentWeaponSkillInfo_KR()}");
    }

    public void TakeDamage(int baseDamage, bool isCritical = false, bool isMiss = false)
    {
        // 인터페이스에서 요구한 메서드 → 기존처럼 작동하게
        TakeDamage(baseDamage, null, isCritical, isMiss);
    }

    public void TakeDamage(int baseDamage, PlayerAttack attacker, bool isCritical = false, bool isMiss = false)
    {
        // 기존 데미지 처리 코드
        int damage = Mathf.RoundToInt(baseDamage * 100f / (100f + CurrentDefense));
        damage = Mathf.Max(damage, 1);
        _CurrentHealth -= damage;

        if (DamageDisplay != null)
        {
            if (isCritical)
                DamageDisplay.ShowCritical(damage, this);
            else
                DamageDisplay.ShowDigits(damage, this);
        }

        if (_CurrentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 공격받았을 때 플레이어가 살아 있으면 반격 시도
            TryCounterAttack();
        }
    }

    // 기습 공격 메서드 추가
    public void ExecuteAmbushAttack()
    {
        Debug.Log($"[기습 공격] {gameObject.name}이 플레이어를 기습 공격!");

        // 플레이어 생존 확인
        if (CharacterInfoUI.Instance == null || CharacterInfoUI.Instance.CurrentHealth <= 0)
        {
            Debug.Log("[기습 공격] 플레이어가 사망 상태 → 기습 취소");
            return;
        }

        // 현재 적용된 무기 타입 정보 가져오기
        string weaponKR = EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => "마법",
            WeaponType.Sword => "검",
            WeaponType.Blunt => "둔기",
            WeaponType.Fist => "주먹",
            WeaponType.Bow => "활",
            WeaponType.Throwing => "던지기",
            WeaponType.Gun => "총",
            _ => "알 수 없음"
        };

        // 기습 공격은 반격보다 강함 (반격 0.5배 → 기습 0.8배)
        int rawAmbushDamage = Mathf.FloorToInt(_currentAttackPower * 0.3f);
        int playerDefense = CharacterInfoUI.Instance.CurrentDefense;

        int finalDamage = Mathf.RoundToInt(rawAmbushDamage * 100f / (100f + playerDefense));
        finalDamage = Mathf.Max(finalDamage, 1);

        Debug.Log($"[기습 공격] {gameObject.name} | 무기 종류: {weaponKR} | 기본 공격력: {_currentAttackPower} | 기습 데미지: {rawAmbushDamage} | 플레이어 방어력: {playerDefense} | 최종 데미지: {finalDamage}");

        // 기습은 확정 명중 (기습의 특성상)
        if (finalDamage <= 0)
        {
            Debug.Log("[기습 공격] 최종 데미지 0 → 미스 처리");
            DamageDisplay.ShowEnemyPlayerMiss(this);
            return;
        }

        // 플레이어에게 데미지 적용
        CharacterManager.Instance.DecreaseHealth(finalDamage, this);

        // 데미지 표시 (기습은 특별한 표시가 있다면 추가 가능)
        if (DamageDisplay != null)
        {
            DamageDisplay.ShowPlayerDamage(finalDamage, this);
        }

        Debug.Log($"[기습 성공] {gameObject.name}가 {weaponKR}으로 {finalDamage} 데미지 기습 공격 완료");
    }

    //반격
    public void TryCounterAttack()
    {
        float counterChance = 0.75f;
        float hitChance = 0.85f;

        if (CharacterInfoUI.Instance.CurrentHealth <= 0)
            return;

        if (Random.value < counterChance)
        {
            // 반격 시도 무기 정보
            string weaponKR = EnemyStat.DefaultWeaponType switch
            {
                WeaponType.Magic => "마법",
                WeaponType.Sword => "검",
                WeaponType.Blunt => "둔기",
                WeaponType.Fist => "주먹",
                WeaponType.Bow => "활",
                WeaponType.Throwing => "던지기",
                WeaponType.Gun => "총",
                _ => "알 수 없음"
            };

            if (Random.value < hitChance) // 명중
            {
                int rawDamage = Mathf.FloorToInt(_currentAttackPower * 0.5f);
                int playerDefense = CharacterInfoUI.Instance.CurrentDefense;

                int finalDamage = Mathf.RoundToInt(rawDamage * 100f / (100f + playerDefense));
                finalDamage = Mathf.Max(finalDamage, 1);

                // 디버그 로그 추가
                Debug.Log($"[Enemy 반격] 무기 종류: {weaponKR} | 기본 공격력: {_currentAttackPower} | 반격 데미지: {rawDamage} | 플레이어 방어력: {playerDefense} | 최종 데미지: {finalDamage}");

                if (finalDamage <= 0)
                {
                    Debug.Log("[Enemy 반격] 데미지 0 → 미스 처리");
                    DamageDisplay.ShowEnemyPlayerMiss(this);
                    return;
                }

                CharacterManager.Instance.DecreaseHealth(finalDamage, this);
                DamageDisplay.ShowPlayerDamage(finalDamage, this);
            }
            else
            {
                Debug.Log("[Enemy 반격] 반격 Miss!");
                DamageDisplay.ShowEnemyPlayerMiss(this);
            }
        }
    }

    private void Die()
    {
        Debug.Log("[적 사망] 적이 쓰러졌습니다!");
        //TimeFlowManager.Instance.PlayerCount.text = "1";
       // Destroy(gameObject);
    }

    public int GetBaseAttackPower()
    {
        if (EnemyStat == null) return 0;

        return EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => EnemyStat.Magic,
            WeaponType.Sword => EnemyStat.Sword,
            WeaponType.Blunt => EnemyStat.Blunt,
            WeaponType.Fist => EnemyStat.Fist,
            WeaponType.Bow => EnemyStat.Bow,
            WeaponType.Throwing => EnemyStat.Throwing,
            WeaponType.Gun => EnemyStat.Gun,
            _ => 0
        };
    }

    // 기존 메서드들 (GetBaseAttackPower, GetCurrentWeaponSkillInfo_KR, Die 등)
    // 이 부분은 기존 코드에 있는 메서드들을 그대로 유지하세요
}*/
/*using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("스탯 설정")]
    public CharacterStatSO EnemyStat;

    [Header("데미지 표시")]
    public DamageDisplay DamageDisplay;

    [SerializeField] private int _MaxHealth;
    [SerializeField] private int _CurrentHealth; // 현재 체력 (인스펙터에서 보기용)

    public int CurrentDefense;
    [SerializeField] private int _currentAttackPower;

    [Header("화면에 보일 적 캐릭터")]
    public GameObject EnemyCharacter;

    private void Start()
    {
        if (EnemyStat == null)
        {
            Debug.LogError("CharacterStatSO가 할당되지 않았습니다!");
            return;
        }

        _MaxHealth = EnemyStat.MaxHealth;
        _CurrentHealth = _MaxHealth;
        CurrentDefense = EnemyStat.Defense; // 방어력 설정
        _currentAttackPower = GetBaseAttackPower();

       // Debug.Log($"[적 초기화] 체력: {_MaxHealth} | 방어력: {CurrentDefense} | 공격력: {_currentAttackPower} | {GetCurrentWeaponSkillInfo_KR()}");
    }

    public void TakeDamage(int baseDamage, bool isCritical = false, bool isMiss = false)
{
    // 인터페이스에서 요구한 메서드 → 기존처럼 작동하게
    TakeDamage(baseDamage, null, isCritical, isMiss);
}

public void TakeDamage(int baseDamage, PlayerAttack attacker, bool isCritical = false, bool isMiss = false)
{
    // 기존 데미지 처리 코드
    int damage = Mathf.RoundToInt(baseDamage * 100f / (100f + CurrentDefense));
    damage = Mathf.Max(damage, 1);
    _CurrentHealth -= damage;

    if (DamageDisplay != null)
    {
        if (isCritical)
            DamageDisplay.ShowCritical(damage, this);
        else
            DamageDisplay.ShowDigits(damage, this);
    }

    if (_CurrentHealth <= 0)
    {
        Die();
    }
    else
    {
        // 공격받았을 때 플레이어가 살아 있으면 반격 시도
        TryCounterAttack();
    }
}


//반격
public void TryCounterAttack()
{
    float counterChance = 0.75f;
    float hitChance = 0.85f;

    if (CharacterInfoUI.Instance.CurrentHealth <= 0)
        return;

    if (Random.value < counterChance)
    {
        // 반격 시도 무기 정보
        string weaponKR = EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => "마법",
            WeaponType.Sword => "검",
            WeaponType.Blunt => "둔기",
            WeaponType.Fist => "주먹",
            WeaponType.Bow => "활",
            WeaponType.Throwing => "던지기",
            WeaponType.Gun => "총",
            _ => "알 수 없음"
        };

        if (Random.value < hitChance) // 명중
        {
            int rawDamage = Mathf.FloorToInt(_currentAttackPower * 0.5f);
            int playerDefense = CharacterInfoUI.Instance.CurrentDefense;

            int finalDamage = Mathf.RoundToInt(rawDamage * 100f / (100f + playerDefense));
            finalDamage = Mathf.Max(finalDamage, 1);

            // 디버그 로그 추가
            Debug.Log($"[Enemy 반격] 무기 종류: {weaponKR} | 기본 공격력: {_currentAttackPower} | 반격 데미지: {rawDamage} | 플레이어 방어력: {playerDefense} | 최종 데미지: {finalDamage}");

            if (finalDamage <= 0)
            {
                Debug.Log("[Enemy 반격] 데미지 0 → 미스 처리");
                DamageDisplay.ShowEnemyPlayerMiss(this);
                return;
            }

            CharacterManager.Instance.DecreaseHealth(finalDamage, this);
            DamageDisplay.ShowPlayerDamage(finalDamage, this);
        }
        else
        {
            Debug.Log("[Enemy 반격] 반격 Miss!");
            DamageDisplay.ShowEnemyPlayerMiss(this);
        }
    }
}*/

/*public void TryCounterAttack()
{
    float counterChance = 0.75f;
    float hitChance = 0.85f;

    if (CharacterInfoUI.Instance.CurrentHealth <= 0)
        return;

    if (Random.value < counterChance)
    {
        if (Random.value < hitChance) // 명중
        {
            int rawDamage = Mathf.FloorToInt(_currentAttackPower * 0.5f);

            // 플레이어의 방어력 고려한 최종 데미지 계산
            int playerDefense = CharacterInfoUI.Instance.CurrentDefense; // 플레이어 방어력
            int finalDamage = Mathf.RoundToInt(rawDamage * 100f / (100f + playerDefense));
            finalDamage = Mathf.Max(finalDamage, 1); // 최소 1 데미지

            if (finalDamage <= 0)
            {
                Debug.Log("[Enemy 반격] 데미지 0 → 미스 처리");
                DamageDisplay.ShowEnemyPlayerMiss(this);
                return;
            }

            Debug.Log($"[Enemy 반격] 플레이어에게 반격 성공! {finalDamage} 피해");

            CharacterManager.Instance.DecreaseHealth(finalDamage, this); // 체력 감소
            DamageDisplay.ShowPlayerDamage(finalDamage, this); // 데미지 표시
        }
        else // Miss
        {
            Debug.Log("[Enemy 반격] 반격 Miss!");
            DamageDisplay.ShowEnemyPlayerMiss(this);
        }
    }
}


    private void Die()
    {
        Debug.Log("[적 사망] 적이 쓰러졌습니다!");
        TimeFlowManager.Instance.PlayerCount.text = "1";
        Destroy(gameObject);
    }

    public int GetBaseAttackPower()
    {
        if (EnemyStat == null) return 0;

        return EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => EnemyStat.Magic,
            WeaponType.Sword => EnemyStat.Sword,
            WeaponType.Blunt => EnemyStat.Blunt,
            WeaponType.Fist => EnemyStat.Fist,
            WeaponType.Bow => EnemyStat.Bow,
            WeaponType.Throwing => EnemyStat.Throwing,
            WeaponType.Gun => EnemyStat.Gun,
            _ => 0
        };
    }

    public string GetCurrentWeaponSkillInfo()
    {
        if (EnemyStat == null) return "정보 없음";

        string weaponType = EnemyStat.DefaultWeaponType.ToString();
        int skillValue = GetBaseAttackPower();

        return $"{weaponType} 숙련도: {skillValue}";
    }*/

    /*public string GetCurrentWeaponSkillInfo_KR()
    {
        if (EnemyStat == null) return "정보 없음";

        string typeKR = EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => "마법",
            WeaponType.Sword => "검",
            WeaponType.Blunt => "둔기",
            WeaponType.Fist => "주먹",
            WeaponType.Bow => "활",
            WeaponType.Throwing => "던지기",
            WeaponType.Gun => "총",
            _ => "알 수 없음"
        };

        int skillValue = GetBaseAttackPower();
        return $"적의 {typeKR} 무기 숙련도: {skillValue}";
    }*/
//}


/*using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("스탯 설정")]
    public CharacterStatSO EnemyStat;

    [Header("데미지 표시")]
    public DamageDisplay DamageDisplay;
    
    
    
    [SerializeField]
    private int _currentHealth;
    [SerializeField]
    private int _currentDefense;

    [SerializeField]
    private int _currentAttackPower;

    private void Start()
{
    if (EnemyStat == null)
    {
        Debug.LogError("CharacterStatSO가 할당되지 않았습니다!");
        return;
    }

    _currentHealth = EnemyStat.MaxHealth;

    // 무기 타입에 따라 공격력 세팅
    _currentAttackPower = GetBaseAttackPower();

    Debug.Log($"[적 초기화] 체력: {_currentHealth} | 공격력: {_currentAttackPower} | {GetCurrentWeaponSkillInfo_KR()}");
}

    public void TakeDamage(int damage, bool isCritical = false, bool isMiss = false)
    {
        if (isMiss)
        {
            DamageDisplay?.ShowMiss();
            return;
        }

        _currentHealth -= damage;
        Debug.Log($"[적 피해] {damage}의 데미지를 입었습니다. 남은 체력: {_currentHealth}");

        // 데미지 시각 효과
        if (DamageDisplay != null)
        {
            if (isCritical)
                DamageDisplay.ShowCritical(damage);
            else
                DamageDisplay.ShowDamage(damage);
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("[적 사망] 적이 쓰러졌습니다!");
        TimeFlowManager.Instance.PlayerCount.text = "1"; // 적절한 방식으로 수정 가능
        Destroy(gameObject);
    }

    // 현재 무기 종류에 따른 공격력 반환
    public int GetBaseAttackPower()
    {
        if (EnemyStat == null) return 0;

        return EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => EnemyStat.Magic,
            WeaponType.Sword => EnemyStat.Sword,
            WeaponType.Blunt => EnemyStat.Blunt,
            WeaponType.Fist => EnemyStat.Fist,
            WeaponType.Bow => EnemyStat.Bow,
            WeaponType.Throwing => EnemyStat.Throwing,
            WeaponType.Gun => EnemyStat.Gun,
            _ => 0
        };
    }

    // 무기 숙련도 정보 출력 (영문)
    public string GetCurrentWeaponSkillInfo()
    {
        if (EnemyStat == null) return "정보 없음";

        string weaponType = EnemyStat.DefaultWeaponType.ToString();
        int skillValue = GetBaseAttackPower();

        return $"{weaponType} 숙련도: {skillValue}";
    }

    // 무기 숙련도 정보 출력 (한글)
    public string GetCurrentWeaponSkillInfo_KR()
    {
        if (EnemyStat == null) return "정보 없음";

        string typeKR = EnemyStat.DefaultWeaponType switch
        {
            WeaponType.Magic => "마법",
            WeaponType.Sword => "검",
            WeaponType.Blunt => "둔기",
            WeaponType.Fist => "주먹",
            WeaponType.Bow => "활",
            WeaponType.Throwing => "던지기",
            WeaponType.Gun => "총",
            _ => "알 수 없음"
        };

        int skillValue = GetBaseAttackPower();
        return $"적의 {typeKR} 무기 숙련도: {skillValue}";


    }
}
*/