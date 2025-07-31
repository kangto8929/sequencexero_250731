using UnityEngine;
using System.Collections.Generic;
using System;

public class EnemyMoveManager : MonoBehaviour
{
    public static EnemyMoveManager Instance;
    public AIMovePlaceManager AIMoveManager;
    public AIEscapePathfinder AIEscapePathFinder;

    [Header("이동 간격 (초)")]
    [SerializeField] private float _moveInterval = 5f;

    [Header("기습 설정")]
    [SerializeField] private float _ambushChance = 0.4f; // 20% 확률
    [SerializeField] private float _ambushCooldown = 5f; // 기습 후 쿨다운
    private float _lastAmbushTime = -999f;

    private float timer;
    private int moveCount = 0;

    public bool IsDead = false; // ✅ 사망 플래그 추가


//private Dictionary<string, bool> ambushDisabledPlaces = new Dictionary<string, bool>();
    private PlaceNameType lastPlace;

    // 현재 위치(장소)를 나타내는 변수 (예: 외부에서 세팅)
    //public PlaceNameType CurrentPlace;
    public PlaceNameType CurrentPlace => AIMoveManager.CurrentPlaceNameType;


    // 각 장소별 기습(disabled 여부) 상태 딕셔너리
    private Dictionary<PlaceNameType, bool> ambushDisabledPlaces = new Dictionary<PlaceNameType, bool>();


    private void Awake()
{

    Instance = this;

    // 기존 코드 유지
    if (AIMoveManager == null)
    {
       AIMoveManager = GetComponent<AIMovePlaceManager>();
        if (AIMoveManager == null)
            Debug.LogError("AIMovePlaceManager 컴포넌트를 찾을 수 없습니다!");
    }

    InitializeAmbushStatus();
}


    private void InitializeAmbushStatus()
    {
        foreach (PlaceNameType place in Enum.GetValues(typeof(PlaceNameType)))
        {
            if (place == PlaceNameType.None) continue;
            ambushDisabledPlaces[place] = false;  // 처음엔 모두 기습 가능 상태
        }
    }

    private void Update()
    {
        if (IsDead) return; // ✅ 사망 시 이동 중단

        timer += Time.deltaTime;

        if (timer >= _moveInterval)
        {
            timer = 0f;
            ProcessAIAction();
        }
    }

    private void ProcessAIAction()
{

    // 1.5. 안전 장소로 탈출 시도 (TryEscapeSmart)
    if (AIMoveManager.TryEscapeSmart())  // aiMovePlaceManager는 AIMovePlaceManager 인스턴스 참조 변수
    {
        Debug.LogWarning("[AI Action] 안전 장소로 스마트 탈출 시도 성공");
        return;
    }

    // 1. 시스템 붕괴 지역에서 즉시 탈출 체크
    if (CheckAndEscapeFromCollapseZone())
    {
        Debug.Log("[AI Action] 시스템 붕괴 지역에서 탈출함");
        return;
    }

    

    // 2. 플레이어와 같은 장소에 있는지 확인하고 기습 시도
    if (CheckAndAttemptAmbush())
    {
        Debug.Log("[AI Action] 플레이어 기습 시도함");
        return;
    }

    // 3. 일반 이동 처리
    HandleNormalMovement();
}


    /*private void ProcessAIAction()
    {
        // 1. 시스템 붕괴 지역에서 즉시 탈출 체크
        if (CheckAndEscapeFromCollapseZone())
        {
            Debug.Log("[AI Action] 시스템 붕괴 지역에서 탈출함");
            return;
        }

        // 2. 플레이어와 같은 장소에 있는지 확인하고 기습 시도
        if (CheckAndAttemptAmbush())
        {
            Debug.Log("[AI Action] 플레이어 기습 시도함");
            return;
        }

        // 3. 일반 이동 처리
        HandleNormalMovement();
    }*/

    /// <summary>
    /// 시스템 붕괴 지역인지 확인하고 즉시 탈출
    /// </summary>
    /*private bool CheckAndEscapeFromCollapseZone()
    {
        if (AIMoveManager.CurrentPlaceName == null) return false;

        bool isCollapseZone = (AIMoveManager.CurrentPlaceName.BewareSystemCollapseIcon != null && 
                              AIMoveManager.CurrentPlaceName.BewareSystemCollapseIcon.activeSelf) ||
                             (AIMoveManager.CurrentPlaceName.AlreadySystemCollapseIcon != null && 
                              AIMoveManager.CurrentPlaceName.AlreadySystemCollapseIcon.activeSelf);

        if (isCollapseZone)
        {
            Debug.LogWarning($"[시스템 붕괴 탈출] {gameObject.name}현재 위치 {AIMoveManager.CurrentPlaceNameType}에서 즉시 탈출 시도");
            
            var availableMoves = AIMoveManager.GetAvailableMoves();
            
            // 현재 위치를 제외하고 안전한 장소만 필터링
            var safePlaces = new List<PlaceConnector>();
            foreach (var place in availableMoves)
            {
                if (place !=AIMoveManager.CurrentPlace)
                {
                    var placeState = place.GetComponentInChildren<PlaceState>();
                    if (placeState != null)
                    {
                        bool isSafe = (placeState.BewareSystemCollapseIcon == null || !placeState.BewareSystemCollapseIcon.activeSelf) &&
                                     (placeState.AlreadySystemCollapseIcon == null || !placeState.AlreadySystemCollapseIcon.activeSelf);
                        
                        if (isSafe)
                        {
                            safePlaces.Add(place);
                        }
                    }
                }
            }

            if (safePlaces.Count > 0)
            {
                var escapePlace = safePlaces[UnityEngine.Random.Range(0, safePlaces.Count)];
                bool moved =AIMoveManager.MoveToPlace(escapePlace);
                
                if (moved)
                {
                    moveCount++;
                    Debug.Log($"[시스템 붕괴 탈출 성공] ({moveCount}회) → {gameObject.name}새 위치: {AIMoveManager.CurrentPlaceNameType}");
                    return true;
                }
                else
                {
                    Debug.LogError("[시스템 붕괴 탈출 실패] 이동 불가능");
                }
            }
            else
            {
                Debug.LogError($"[시스템 붕괴 탈출 실패] {gameObject.name}안전한 장소 없음 → 사망 처리");
                HandleDeath();
                return true;
            }
        }

        return false;
    }*/

    private bool CheckAndEscapeFromCollapseZone()
{
    if (AIMoveManager.CurrentPlaceName == null) return false;

    bool isCollapseZone = (AIMoveManager.CurrentPlaceName.BewareSystemCollapseIcon != null &&
                          AIMoveManager.CurrentPlaceName.BewareSystemCollapseIcon.activeSelf) ||
                         (AIMoveManager.CurrentPlaceName.AlreadySystemCollapseIcon != null &&
                          AIMoveManager.CurrentPlaceName.AlreadySystemCollapseIcon.activeSelf);

    if (!isCollapseZone)
        return false;

    Debug.LogWarning($"[시스템 붕괴 탈출] {gameObject.name} 현재 위치 {AIMoveManager.CurrentPlaceNameType}에서 즉시 탈출 시도");



    PlaceConnector currentPlace = PlaceConnectorManager.Instance.GetPlaceConnectorByPlaceName(AIMoveManager.CurrentPlaceNameType);

    if (currentPlace == null)
    {
        Debug.LogError("현재 위치 PlaceConnector를 찾지 못함");
        return false;
    }

    List<PlaceConnector> escapePath = AIEscapePathfinder.FindEscapeRoute(currentPlace);

    if (escapePath == null || escapePath.Count == 0)
    {
        Debug.LogError($"[시스템 붕괴 탈출 실패] {gameObject.name} 안전한 경로 없음 → 사망 처리");
        HandleDeath();
        return true;
    }

    // 🔁 탈출 경로 순회: 현재 위치 제외하고 안전한 장소 찾기
    PlaceConnector nextPlace = null;
    PlaceConnector fallbackPlace = null;

    foreach (var connector in escapePath)
    {
        if (connector == AIMoveManager.CurrentPlace)
            continue;

        var placeState = connector.GetComponentInChildren<PlaceState>();
        bool isConnectorCollapsed = (placeState != null &&
            ((placeState.BewareSystemCollapseIcon != null && placeState.BewareSystemCollapseIcon.activeSelf) ||
             (placeState.AlreadySystemCollapseIcon != null && placeState.AlreadySystemCollapseIcon.activeSelf)));

        if (!isConnectorCollapsed)
        {
            nextPlace = connector;
            break; // 안전한 곳 발견
        }

        // 붕괴 지역도 일단 fallback으로 저장
        if (fallbackPlace == null)
            fallbackPlace = connector;
    }

    if (nextPlace == null && fallbackPlace != null)
    {
        Debug.LogWarning("[탈출 경고] 안전한 장소는 없지만, 붕괴 장소라도 이동 시도합니다.");
        nextPlace = fallbackPlace;
    }

    if (nextPlace == null)
    {
        Debug.LogError($"[시스템 붕괴 탈출 실패] {gameObject.name} 이동할 수 있는 장소 없음 → 사망 처리");
        HandleDeath();
        return true;
    }

    // PlaceConnectorManager에서 이름으로 재확인
    var placeFromManager = PlaceConnectorManager.Instance.GetPlaceConnectorByPlaceName(
        nextPlace.GetComponentInChildren<PlaceState>().PlaceNameSetting);

    if (placeFromManager != null)
    {
        nextPlace = placeFromManager;
    }
    else
    {
        Debug.LogWarning("PlaceConnectorManager에서 장소를 찾지 못해, 원래 PlaceConnector 사용합니다.");
    }

    bool moved = AIMoveManager.MoveToPlace(nextPlace);

    if (moved)
    {
        moveCount++;
        Debug.Log($"[시스템 붕괴 탈출 성공] ({moveCount}회) → {gameObject.name} 새 위치: {AIMoveManager.CurrentPlaceNameType}");
        return true;
    }
    else
    {
        Debug.LogError($"[시스템 붕괴 탈출 실패] {gameObject.name}  이동 불가능");
        return false;
    }
}


    /* private bool CheckAndEscapeFromCollapseZone()
{
    if (AIMoveManager.CurrentPlaceName == null) return false;

    bool isCollapseZone = (AIMoveManager.CurrentPlaceName.BewareSystemCollapseIcon != null &&
                          AIMoveManager.CurrentPlaceName.BewareSystemCollapseIcon.activeSelf) ||
                         (AIMoveManager.CurrentPlaceName.AlreadySystemCollapseIcon != null &&
                          AIMoveManager.CurrentPlaceName.AlreadySystemCollapseIcon.activeSelf);

    if (isCollapseZone)
    {
        Debug.LogWarning($"[시스템 붕괴 탈출] {gameObject.name} 현재 위치 {AIMoveManager.CurrentPlaceNameType}에서 즉시 탈출 시도");

        if (AIEscapePathFinder == null)
        {
            Debug.LogError("AIEscapePathfinder가 할당되지 않았습니다.");
            return false;
        }

        // 1. 현재 위치 PlaceConnector 얻기
        PlaceConnector currentPlace = PlaceConnectorManager.Instance.GetPlaceConnectorByPlaceName(AIMoveManager.CurrentPlaceNameType);

        if (currentPlace == null)
        {
            Debug.LogError("현재 위치 PlaceConnector를 찾지 못함");
            return false;
        }

        // 2. 탈출 경로 전체 받기 (List<PlaceConnector>)
        List<PlaceConnector> escapePath = AIEscapePathfinder.FindEscapeRoute(currentPlace);

        if (escapePath != null && escapePath.Count > 0)
        {
            // 다음 이동할 장소 결정
            PlaceConnector nextPlace = escapePath[0];

            if (nextPlace == AIMoveManager.CurrentPlace)
            {
                if (escapePath.Count > 1)
                    nextPlace = escapePath[1];
                else
                    return false; // 이동할 장소 없음
            }

            // PlaceConnectorManager에서 다시 확인 (선택사항)
            var placeFromManager = PlaceConnectorManager.Instance.GetPlaceConnectorByPlaceName(
                nextPlace.GetComponentInChildren<PlaceState>().PlaceNameSetting);

            if (placeFromManager != null)
            {
                nextPlace = placeFromManager;
            }
            else
            {
                Debug.LogWarning("PlaceConnectorManager에서 장소를 찾지 못해, 원래 장소를 사용합니다.");
            }

            bool moved = AIMoveManager.MoveToPlace(nextPlace);

            if (moved)
            {
                moveCount++;
                Debug.Log($"[시스템 붕괴 탈출 성공] ({moveCount}회) → {gameObject.name} 새 위치: {AIMoveManager.CurrentPlaceNameType}");
                return true;
            }
            else
            {
                Debug.LogError("[시스템 붕괴 탈출 실패] 이동 불가능");
            }
        }
        else
        {
            Debug.LogError($"[시스템 붕괴 탈출 실패] {gameObject.name} 안전한 경로 없음 → 사망 처리");
            HandleDeath();
            return true;
        }
    }

    return false;
}*/


private bool CheckAndAttemptAmbush()
{
    if (MovePlaceManager.Instance == null || MovePlaceManager.Instance.CurrentPlaceName == null)
        return false;

    PlaceNameType currentPlace = AIMoveManager.CurrentPlaceNameType;

    if (ambushDisabledPlaces.ContainsKey(currentPlace) && ambushDisabledPlaces[currentPlace])
    {
        Debug.Log($"[기습 체크]{gameObject.name} 현재 장소({currentPlace})는 기습 불가 상태");
        return false;
    }

    bool isPlayerInSamePlace = MovePlaceManager.Instance.CurrentPlaceName.PlaceNameSetting == currentPlace;

    if (isPlayerInSamePlace)
    {
        if (Time.time - _lastAmbushTime < _ambushCooldown)
        {
            Debug.Log($"[기습 체크] 쿨다운 중");
            return false;
        }

        if (CharacterInfoUI.Instance == null || CharacterInfoUI.Instance.CurrentHealth <= 0)
        {
            Debug.Log("[기습 체크] 플레이어 사망 상태 → 기습 취소");
            return false;
        }

        if (UnityEngine.Random.value < _ambushChance)
        {
            ExecuteAmbush();
            _lastAmbushTime = Time.time;
            ambushDisabledPlaces[currentPlace] = true;
            return true;
        }
        else
        {
            Debug.Log("[기습 체크] 기습 확률 실패");
            return false;
        }
    }

    return false;
}




  /* private bool CheckAndAttemptAmbush()
{
    if (MovePlaceManager.Instance == null || MovePlaceManager.Instance.CurrentPlaceName == null)
        return false;

    PlaceNameType currentPlace = AIMoveManager.CurrentPlaceNameType;

    // 현재 장소가 기습 불가 상태라면 기습 불가
    if (ambushDisabledPlaces.ContainsKey(currentPlace) && ambushDisabledPlaces[currentPlace])
    {
        Debug.Log($"[기습 체크]{gameObject.name}는 현재 장소({currentPlace})는 기습 불가 상태");
        return false;
    }

    bool isPlayerInSamePlace = MovePlaceManager.Instance.CurrentPlaceName.PlaceNameSetting == currentPlace;

    if (isPlayerInSamePlace)
    {
        // 쿨다운 체크
        if (Time.time - _lastAmbushTime < _ambushCooldown)
        {
            Debug.Log($"[기습 체크] 쿨다운 중 (남은 시간: {_ambushCooldown - (Time.time - _lastAmbushTime):F1}초)");
            return false;
        }

        // 플레이어가 살아있는지 확인
        if (CharacterInfoUI.Instance == null || CharacterInfoUI.Instance.CurrentHealth <= 0)
        {
            Debug.Log("[기습 체크] 플레이어가 사망 상태 → 기습 취소");
            return false;
        }

        // 기습 확률 체크
        if (UnityEngine.Random.value < _ambushChance)
        {
            ExecuteAmbush();
            _lastAmbushTime = Time.time;

            // 기습 성공 시 해당 장소를 기습 불가 상태로 변경
            ambushDisabledPlaces[currentPlace] = true;

            return true;
        }
        else
        {
            Debug.Log("[기습 체크] 기습 확률 실패 (실패 확률 80%)");
            return false;
        }
    }

    return false;
}*/


public void TryAmbush()
    {
        if (!ambushDisabledPlaces[CurrentPlace])
        {
            Debug.Log($"기습 성공! 장소: {CurrentPlace}");
            ambushDisabledPlaces[CurrentPlace] = true;  // 한번 기습하면 다시 기습 불가능
            // 기습 처리 로직 추가
        }
        else
        {
            Debug.Log($"기습 불가능한 장소입니다: {CurrentPlace}");
        }
    }

    // 필요하면 기습 상태 초기화 함수
    public void ResetAmbushStatus()
    {
        foreach (var key in new List<PlaceNameType>(ambushDisabledPlaces.Keys))
        {
            ambushDisabledPlaces[key] = false;
        }
    }


    /// <summary>
    /// 기습 공격 실행
    /// </summary>
    private void ExecuteAmbush()
    {
        // Enemy 컴포넌트 가져오기
        Enemy enemyComponent = GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError("[기습 공격] Enemy 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // Enemy 클래스의 기습 공격 메서드 호출
        enemyComponent.ExecuteAmbushAttack();
    }

    /// <summary>
    /// 일반적인 이동 처리
    /// </summary>
    /// 
    private void HandleNormalMovement()
{
    var availableMoves =AIMoveManager.GetAvailableMoves();

    if ((availableMoves == null || availableMoves.Count == 0)
        && AIMoveManager.CurrentPlaceName.CanNotEnter.activeSelf == false)
    {
        Debug.LogWarning($"[UPDATE] 이동 가능한 장소 없음 → 대기 (현재 위치: {AIMoveManager.CurrentPlaceNameType})");
        return;
    }

    if ((availableMoves == null || availableMoves.Count == 0)
        && AIMoveManager.CurrentPlaceName.CanNotEnter.activeSelf == true)
    {
        Debug.LogWarning($"[UPDATE] 이동 불가 & 진입금지 → 사망 (현재 위치: {AIMoveManager.CurrentPlaceNameType})");
        HandleDeath();
        return;
    }

    var nextPlace = availableMoves[UnityEngine.Random.Range(0, availableMoves.Count)];
    bool moved = AIMoveManager.MoveToPlace(nextPlace);

    if (moved)
    {
        moveCount++;
        Debug.Log($"[UPDATE] 이동 성공 ({moveCount}회) → 현재 위치: {AIMoveManager.CurrentPlaceNameType}");
        
        // 이동 성공 시 호출
        OnMoveCompleted();
    }
    else
    {
        Debug.LogWarning("[UPDATE] 이동 실패 (MoveToPlace 반환 false)");
    }
}



    public void CheckAIStatusOnTimeChange()
    {
        if (IsDead) return; // ✅ 사망 시 이동 중단

        // 시간 변경 시에도 시스템 붕괴 체크
        if (CheckAndEscapeFromCollapseZone())
        {
            Debug.Log("[시간 변경] 시스템 붕괴 지역에서 탈출함");
            return;
        }

        var currentPlaceState = AIMoveManager.CurrentPlaceName;

        if (currentPlaceState == null)
        {
            Debug.LogWarning("[CHECK] 현재 장소 상태 정보 없음 → 무시");
            return;
        }

        if (currentPlaceState.CanNotEnter != null && currentPlaceState.CanNotEnter.activeSelf)
        {
            Debug.LogWarning("[CHECK] 현재 장소가 진입 금지 상태임 → 즉시 사망");
            HandleDeath();
            return;
        }

        // 시간 변경 시에도 기습 체크
        if (CheckAndAttemptAmbush())
        {
            Debug.Log("[시간 변경] 플레이어 기습 시도함");
            return;
        }

        var availableMoves = AIMoveManager.GetAvailableMoves();

        if (availableMoves == null || availableMoves.Count == 0)
        {
            Debug.Log("[CHECK] 이동할 수 있는 장소 없음 → 대기 상태 유지");
            return;
        }

        var nextPlace = availableMoves[UnityEngine.Random.Range(0, availableMoves.Count)];
        bool moved =AIMoveManager.MoveToPlace(nextPlace);

        if (moved)
        {
            moveCount++;
            Debug.Log($"[CHECK] 낮/밤 전환으로 이동 성공 ({moveCount}회) → 현재 위치: {AIMoveManager.CurrentPlaceNameType}");
        }
        else
        {
            Debug.LogWarning("[CHECK] 이동 실패 (MoveToPlace 반환 false)");
        }
    }

    public void HandleDeath()
{
    if (IsDead)
    {
        Debug.LogWarning($"[HandleDeath] 이미 죽은 상태임: {gameObject.name}");
        return;
    }

    IsDead = true;
    Debug.LogError($"[EnemyMoveManager] {gameObject.name} 사망 처리됨. 위치: {AIMoveManager.CurrentPlaceNameType}");
    UI_InGameManager.Instance.UpdatePlayerCount();
}


    /*public void HandleDeath()
    {
        if (IsDead) return; // ✅ 중복 사망 방지

        IsDead= true; // ✅ 이동 정지 처리
        Debug.LogError($"[EnemyMoveManager] {gameObject.name} 사망 처리됨. 위치: {AIMoveManager.CurrentPlaceNameType}");
         UI_InGameManager.Instance.UpdatePlayerCount( UI_InGameManager.Instance.PlayerCount-1);
         Debug.Log($"생존자 수 { UI_InGameManager.Instance.PlayerCount}");
        

        // 사망 시 비활성화 또는 시각적 처리
        //gameObject.SetActive(false);
    }*/

    private void OnMoveCompleted()
{
    PlaceNameType  currentPlace = AIMoveManager.CurrentPlaceNameType;

    // 3. 이전에 기습 불가였던 장소라도 다시 돌아오면 초기화(기습 가능)
    if (ambushDisabledPlaces.ContainsKey(currentPlace))
    {
        ambushDisabledPlaces[currentPlace] = false; // 기습 가능 상태로 리셋
    }
    else
    {
        ambushDisabledPlaces.Add(currentPlace, false); // 신규 장소는 기본 기습 가능 상태
    }

    lastPlace = currentPlace;
}

public void OnAmbushedOrAttacked()
{
     PlaceNameType  currentPlace = AIMoveManager.CurrentPlaceNameType;

   if (ambushDisabledPlaces.ContainsKey(currentPlace))
            ambushDisabledPlaces[currentPlace] = true;
        else
            ambushDisabledPlaces.Add(currentPlace, true);
}


    
}

/*using UnityEngine;
using System.Collections.Generic;

public class EnemyMoveManager : MonoBehaviour
{
    public static EnemyMoveManager Instance;
    public AIMovePlaceManager moveManager;

    [Header("이동 간격 (초)")]
    [SerializeField] private float _moveInterval = 3f;

    [Header("기습 설정")]
    [SerializeField] private float _ambushChance = 0.2f; // 20% 확률
    [SerializeField] private float _ambushCooldown = 5f; // 기습 후 쿨다운
    private float _lastAmbushTime = -999f;

    private float timer;
    private int moveCount = 0;

    private bool isDead = false; // ✅ 사망 플래그 추가

    private void Awake()
    {
        if (moveManager == null)
        {
            moveManager = GetComponent<AIMovePlaceManager>();
            if (moveManager == null)
                Debug.LogError("AIMovePlaceManager 컴포넌트를 찾을 수 없습니다!");
        }
    }

    private void Update()
    {
        if (isDead) return; // ✅ 사망 시 이동 중단

        timer += Time.deltaTime;

        if (timer >= _moveInterval)
        {
            timer = 0f;
            ProcessAIAction();
        }
    }

    private void ProcessAIAction()
    {
        // 1. 시스템 붕괴 지역에서 즉시 탈출 체크
        if (CheckAndEscapeFromCollapseZone())
        {
            Debug.Log("[AI Action] 시스템 붕괴 지역에서 탈출함");
            return;
        }

        // 2. 플레이어와 같은 장소에 있는지 확인하고 기습 시도
        if (CheckAndAttemptAmbush())
        {
            Debug.Log("[AI Action] 플레이어 기습 시도함");
            return;
        }

        // 3. 일반 이동 처리
        HandleNormalMovement();
    }

    /// <summary>
    /// 시스템 붕괴 지역인지 확인하고 즉시 탈출
    /// </summary>
    private bool CheckAndEscapeFromCollapseZone()
    {
        if (moveManager.CurrentPlaceName == null) return false;

        bool isCollapseZone = (moveManager.CurrentPlaceName.BewareSystemCollapseIcon != null && 
                              moveManager.CurrentPlaceName.BewareSystemCollapseIcon.activeSelf) ||
                             (moveManager.CurrentPlaceName.AlreadySystemCollapseIcon != null && 
                              moveManager.CurrentPlaceName.AlreadySystemCollapseIcon.activeSelf);

        if (isCollapseZone)
        {
            Debug.LogWarning($"[시스템 붕괴 탈출] 현재 위치 {moveManager.CurrentPlaceNameType}에서 즉시 탈출 시도");
            
            var availableMoves = moveManager.GetAvailableMoves();
            
            // 현재 위치를 제외하고 안전한 장소만 필터링
            var safePlaces = new List<PlaceConnector>();
            foreach (var place in availableMoves)
            {
                if (place != moveManager.CurrentPlace)
                {
                    var placeState = place.GetComponentInChildren<PlaceState>();
                    if (placeState != null)
                    {
                        bool isSafe = (placeState.BewareSystemCollapseIcon == null || !placeState.BewareSystemCollapseIcon.activeSelf) &&
                                     (placeState.AlreadySystemCollapseIcon == null || !placeState.AlreadySystemCollapseIcon.activeSelf);
                        
                        if (isSafe)
                        {
                            safePlaces.Add(place);
                        }
                    }
                }
            }

            if (safePlaces.Count > 0)
            {
                var escapePlace = safePlaces[Random.Range(0, safePlaces.Count)];
                bool moved = moveManager.MoveToPlace(escapePlace);
                
                if (moved)
                {
                    moveCount++;
                    Debug.Log($"[시스템 붕괴 탈출 성공] ({moveCount}회) → 새 위치: {moveManager.CurrentPlaceNameType}");
                    return true;
                }
                else
                {
                    Debug.LogError("[시스템 붕괴 탈출 실패] 이동 불가능");
                }
            }
            else
            {
                Debug.LogError("[시스템 붕괴 탈출 실패] 안전한 장소 없음 → 사망 처리");
                HandleDeath();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 플레이어와 같은 장소에 있는지 확인하고 기습 시도
    /// </summary>
    private bool CheckAndAttemptAmbush()
    {
        // MovePlaceManager가 없으면 기습 불가
        if (MovePlaceManager.Instance == null || MovePlaceManager.Instance.CurrentPlaceName == null)
            return false;

        // 플레이어와 같은 장소에 있는지 확인
        bool isPlayerInSamePlace = MovePlaceManager.Instance.CurrentPlaceName.PlaceNameSetting == moveManager.CurrentPlaceNameType;
        
        if (isPlayerInSamePlace)
        {
            Debug.Log($"[기습 체크] 플레이어와 같은 장소 감지: {moveManager.CurrentPlaceNameType}");
            
            // 쿨다운 체크
            if (Time.time - _lastAmbushTime < _ambushCooldown)
            {
                Debug.Log($"[기습 체크] 쿨다운 중 (남은 시간: {_ambushCooldown - (Time.time - _lastAmbushTime):F1}초)");
                return false;
            }

            // 플레이어가 살아있는지 확인
            if (CharacterInfoUI.Instance == null || CharacterInfoUI.Instance.CurrentHealth <= 0)
            {
                Debug.Log("[기습 체크] 플레이어가 사망 상태 → 기습 취소");
                return false;
            }

            // 20% 확률로 기습 시도
            if (Random.value < _ambushChance)
            {
                ExecuteAmbush();
                _lastAmbushTime = Time.time;
                return true;
            }
            else
            {
                Debug.Log("[기습 체크] 기습 확률 실패 (80% 확률로 실패)");
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// 기습 공격 실행
    /// </summary>
    private void ExecuteAmbush()
    {
        Debug.Log($"[기습 공격] {gameObject.name}이 플레이어를 기습 공격!");
        
        // Enemy 컴포넌트 가져오기
        Enemy enemyComponent = GetComponent<Enemy>();
        if (enemyComponent == null)
        {
            Debug.LogError("[기습 공격] Enemy 컴포넌트를 찾을 수 없습니다!");
            return;
        }

        // 기습 공격은 일반 반격보다 강함 (반격은 0.5배, 기습은 0.8배)
        int ambushDamage = Mathf.FloorToInt(enemyComponent.EnemyStat.AttackPower * 0.8f);
        int playerDefense = CharacterInfoUI.Instance.CurrentDefense;
        
        int finalDamage = Mathf.RoundToInt(ambushDamage * 100f / (100f + playerDefense));
        finalDamage = Mathf.Max(finalDamage, 1);

        Debug.Log($"[기습 공격]  {gameObject.name} 기본 데미지: {ambushDamage} | 플레이어 방어력: {playerDefense} | 최종 데미지: {finalDamage}");

        // 플레이어에게 데미지 적용
        CharacterManager.Instance.DecreaseHealth(finalDamage, enemyComponent);
        
        // 데미지 표시
        if (enemyComponent.DamageDisplay != null)
        {
            enemyComponent.DamageDisplay.ShowPlayerDamage(finalDamage, enemyComponent);
        }

        // 기습 성공 로그
        Debug.Log($"[기습 성공]  {gameObject.name}가 {finalDamage} 데미지로 플레이어 공격 완료");
    }

    /// <summary>
    /// 일반적인 이동 처리
    /// </summary>
    private void HandleNormalMovement()
    {
        var availableMoves = moveManager.GetAvailableMoves();

        if ((availableMoves == null || availableMoves.Count == 0)
            && moveManager.CurrentPlaceName.CanNotEnter.activeSelf == false)
        {
            Debug.LogWarning($"[UPDATE] 이동 가능한 장소 없음 → 대기 (현재 위치: {moveManager.CurrentPlaceNameType})");
            return;
        }

        if ((availableMoves == null || availableMoves.Count == 0)
            && moveManager.CurrentPlaceName.CanNotEnter.activeSelf == true)
        {
            Debug.LogWarning($"[UPDATE] 이동 불가 & 진입금지 →  {gameObject.name} 사망 (현재 위치: {moveManager.CurrentPlaceNameType})");
            HandleDeath();
            return;
        }

        var nextPlace = availableMoves[Random.Range(0, availableMoves.Count)];
        bool moved = moveManager.MoveToPlace(nextPlace);

        if (moved)
        {
            moveCount++;
            Debug.Log($"[UPDATE] 이동 성공 ({moveCount}회) → 현재 위치: {moveManager.CurrentPlaceNameType}");
        }
        else
        {
            Debug.LogWarning("[UPDATE] 이동 실패 (MoveToPlace 반환 false)");
        }
    }

    public void CheckAIStatusOnTimeChange()
    {
        if (isDead) return; // ✅ 사망 시 이동 중단

        // 시간 변경 시에도 시스템 붕괴 체크
        if (CheckAndEscapeFromCollapseZone())
        {
            Debug.LogWarning($"[시간 변경]  {gameObject.name} 시스템 붕괴 지역에서 탈출함");
            return;
        }

        var currentPlaceState = moveManager.CurrentPlaceName;

        if (currentPlaceState == null)
        {
            Debug.LogWarning("[CHECK] 현재 장소 상태 정보 없음 → 무시");
            return;
        }

        if (currentPlaceState.CanNotEnter != null && currentPlaceState.CanNotEnter.activeSelf)
        {
            Debug.LogWarning($"[CHECK] 현재 장소가 진입 금지 상태임 →  {gameObject.name} 즉시 사망");
            HandleDeath();
            return;
        }

        // 시간 변경 시에도 기습 체크
        if (CheckAndAttemptAmbush())
        {
            Debug.Log($"[시간 변경]  {gameObject.name} 플레이어 기습 시도함");
            return;
        }

        var availableMoves = moveManager.GetAvailableMoves();

        if (availableMoves == null || availableMoves.Count == 0)
        {
            Debug.Log("[CHECK] 이동할 수 있는 장소 없음 → 대기 상태 유지");
            return;
        }

        var nextPlace = availableMoves[Random.Range(0, availableMoves.Count)];
        bool moved = moveManager.MoveToPlace(nextPlace);

        if (moved)
        {
            moveCount++;
            Debug.Log($"[CHECK] 낮/밤 전환으로 이동 성공 ({moveCount}회) → 현재 위치: {moveManager.CurrentPlaceNameType}");
        }
        else
        {
            Debug.LogWarning("[CHECK] 이동 실패 (MoveToPlace 반환 false)");
        }
    }

    public void HandleDeath()
    {
        if (isDead) return; // ✅ 중복 사망 방지

        isDead = true; // ✅ 이동 정지 처리
        Debug.LogError($"[EnemyMoveManager] {gameObject.name} 사망 처리됨. 위치: {moveManager.CurrentPlaceNameType}");

        // 사망 시 비활성화 또는 시각적 처리
        //gameObject.SetActive(false);
    }
}*/

/*using UnityEngine;
using System.Collections.Generic;
public class EnemyMoveManager : MonoBehaviour
{
    public static EnemyMoveManager Instance;
    public AIMovePlaceManager moveManager;

    [Header("이동 간격 (초)")]
    [SerializeField] private float _moveInterval = 3f;

    private float timer;
    private int moveCount = 0;

    private bool isDead = false; // ✅ 사망 플래그 추가

    private void Awake()
    {
        if (moveManager == null)
        {
            moveManager = GetComponent<AIMovePlaceManager>();
            if (moveManager == null)
                Debug.LogError("AIMovePlaceManager 컴포넌트를 찾을 수 없습니다!");
        }
    }

    private void Update()
    {
        if (isDead) return; // ✅ 사망 시 이동 중단

        timer += Time.deltaTime;

        if (timer >= _moveInterval)
        {
            timer = 0f;

            var availableMoves = moveManager.GetAvailableMoves();

            if ((availableMoves == null || availableMoves.Count == 0) 
                && moveManager.CurrentPlaceName.CanNotEnter.activeSelf == false)
            {
                Debug.LogWarning($"[UPDATE] 이동 가능한 장소 없음 → 대기 (현재 위치: {moveManager.CurrentPlaceNameType})");
                return;
            }

            if ((availableMoves == null || availableMoves.Count == 0)
                && moveManager.CurrentPlaceName.CanNotEnter.activeSelf == true)
            {
                Debug.LogWarning($"[UPDATE] 이동 불가 & 진입금지 → 사망 (현재 위치: {moveManager.CurrentPlaceNameType})");
                HandleDeath();
                return;
            }

            var nextPlace = availableMoves[Random.Range(0, availableMoves.Count)];
            bool moved = moveManager.MoveToPlace(nextPlace);

            if (moved)
            {
                moveCount++;
                Debug.Log($"[UPDATE] 이동 성공 ({moveCount}회) → 현재 위치: {moveManager.CurrentPlaceNameType}");
            }
            else
            {
                Debug.LogWarning("[UPDATE] 이동 실패 (MoveToPlace 반환 false)");
            }
        }
    }

    public void CheckAIStatusOnTimeChange()
    {
        if (isDead) return; // ✅ 사망 시 이동 중단

        var currentPlaceState = moveManager.CurrentPlaceName;

        if (currentPlaceState == null)
        {
            Debug.LogWarning("[CHECK] 현재 장소 상태 정보 없음 → 무시");
            return;
        }

        if (currentPlaceState.CanNotEnter != null && currentPlaceState.CanNotEnter.activeSelf)
        {
            Debug.LogWarning("[CHECK] 현재 장소가 진입 금지 상태임 → 즉시 사망");
            HandleDeath();
            return;
        }

        var availableMoves = moveManager.GetAvailableMoves();

        if (availableMoves == null || availableMoves.Count == 0)
        {
            Debug.Log("[CHECK] 이동할 수 있는 장소 없음 → 대기 상태 유지");
            return;
        }

        var nextPlace = availableMoves[Random.Range(0, availableMoves.Count)];
        bool moved = moveManager.MoveToPlace(nextPlace);

        if (moved)
        {
            moveCount++;
            Debug.Log($"[CHECK] 낮/밤 전환으로 이동 성공 ({moveCount}회) → 현재 위치: {moveManager.CurrentPlaceNameType}");
        }
        else
        {
            Debug.LogWarning("[CHECK] 이동 실패 (MoveToPlace 반환 false)");
        }
    }

    public void HandleDeath()
    {
        if (isDead) return; // ✅ 중복 사망 방지

        isDead = true; // ✅ 이동 정지 처리
        Debug.LogError($"[EnemyMoveManager] {gameObject.name} 사망 처리됨. 위치: {moveManager.CurrentPlaceNameType}");

        // 사망 시 비활성화 또는 시각적 처리
        //gameObject.SetActive(false);
    }
}
*/
/*using UnityEngine;
using System.Collections.Generic;

public class EnemyMoveManager : MonoBehaviour
{
    public static EnemyMoveManager Instance;
    public AIMovePlaceManager moveManager;

    [Header("시작 장소")]
    //[SerializeField] private PlaceConnector _startPlace;

    [Header("이동 간격 (초)")]
    [SerializeField] private float _moveInterval = 3f;

    private float timer;
    private int moveCount = 0;

    //public PlaceState _currentPlaceState;

    private void Awake()
    {
        if (moveManager == null)
        {
            moveManager = GetComponent<AIMovePlaceManager>();
            if (moveManager == null)
                Debug.LogError("AIMovePlaceManager 컴포넌트를 찾을 수 없습니다!");
        }
    }


private void Update()
{
    timer += Time.deltaTime;

    if (timer >= _moveInterval)
    {
        timer = 0f;

        var availableMoves = moveManager.GetAvailableMoves();

        if (availableMoves == null || availableMoves.Count == 0
        && moveManager.CurrentPlaceName.CanNotEnter.activeSelf ==false)
        {
            Debug.LogWarning($"[UPDATE] 이동 가능한 장소 없음 → 대기 (현재 위치: {moveManager.CurrentPlaceNameType})");
            // 사망하지 않음, 다음 주기까지 대기
            return;
        }

        if (availableMoves == null || availableMoves.Count == 0
        && moveManager.CurrentPlaceName.CanNotEnter.activeSelf ==true)
        {
            Debug.LogWarning($"[UPDATE] 이동 가능한 장소 없음 → 사망망 (현재 위치: {moveManager.CurrentPlaceNameType})");
            HandleDeath();
            // 사망하지 않음, 다음 주기까지 대기
            return;
        }

        var nextPlace = availableMoves[Random.Range(0, availableMoves.Count)];
        bool moved = moveManager.MoveToPlace(nextPlace);

        if (moved)
        {
            moveCount++;
            Debug.Log($"[UPDATE] 이동 성공 ({moveCount}회) → 현재 위치: {moveManager.CurrentPlaceNameType}");
        }
        else
        {
            Debug.LogWarning("[UPDATE] 이동 실패 (MoveToPlace 반환 false)");
        }
    }
}


public void CheckAIStatusOnTimeChange()
{
    var currentPlaceState = moveManager.CurrentPlaceName;

    if (currentPlaceState == null)
    {
        Debug.LogWarning("[CHECK] 현재 장소 상태 정보 없음 → 무시");
        return;
    }

    // CanNotEnter가 활성화된 장소에 있을 경우 즉시 사망
    if (currentPlaceState.CanNotEnter != null && currentPlaceState.CanNotEnter.activeSelf)
    {
        Debug.LogWarning("[CHECK] 현재 장소가 진입 금지 상태임 → 즉시 사망");
        HandleDeath();
        return;
    }

    var availableMoves = moveManager.GetAvailableMoves();

    if (availableMoves == null || availableMoves.Count == 0)
    {
        // 이동 불가능 → 다음 낮/밤까지 대기
        Debug.Log("[CHECK] 이동할 수 있는 장소 없음 → 대기 상태 유지");
        return;
    }

    // 이동 가능하므로 무작위 장소로 이동
    var nextPlace = availableMoves[Random.Range(0, availableMoves.Count)];
    bool moved = moveManager.MoveToPlace(nextPlace);

    if (moved)
    {
        moveCount++;
        Debug.Log($"[CHECK] 낮/밤 전환으로 이동 성공 ({moveCount}회) → 현재 위치: {moveManager.CurrentPlaceNameType}");
    }
    else
    {
        Debug.LogWarning("[CHECK] 이동 실패 (MoveToPlace 반환 false)");
    }
}



    public void HandleDeath()
    {
        Debug.LogError($"[EnemyMoveManager] {gameObject.name}사망 처리됨. AI의 사망한 위치 {moveManager.CurrentPlaceNameType}");


        // 사망 처리 예시: 오브젝트 비활성화
        //gameObject.SetActive(false);

        // 추가적인 사망 처리 로직이 있다면 여기에 추가
        // 예: UI 표시, 이펙트, 게임 오버 등
    }
}
*/