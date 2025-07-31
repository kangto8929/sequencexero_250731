using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class PlaceConnector : MonoBehaviour
{
    public List<PlaceConnector> ConnectPlaces = new List<PlaceConnector>();

    [SerializeField]
    private string _placeName;

    public bool IsDisabled 
    { 
        get => _isDisabled;
        set 
        {
            /*if (_isDisabled != value)
            {
                Debug.Log($"[{Time.time:F1}초] {name}.IsDisabled 변경: {_isDisabled} → {value}");
                Debug.Log($"호출 스택: {System.Environment.StackTrace}");
            }*/
            _isDisabled = value;
        }
    }
    [SerializeField] private bool _isDisabled = false;
    
    // 추가된 부분
    public bool IsDisabledForPlayer = false;  // 플레이어만 접근 불가
    public bool IsDisabledForAI = false;      // AI만 접근 불가
    
    // 플레이어용 접근 가능 여부
    public bool IsAccessibleForPlayer => !IsDisabled && !IsDisabledForPlayer;
    
    // AI용 접근 가능 여부 (IsDisabled 무시)
    public bool IsAccessibleForAI 
    { 
        get 
        {
            // AI 전용 차단만 체크 (IsDisabled는 플레이어용이므로 무시)
            if (IsDisabledForAI) 
                return false;
            
            // GameObject 활성화 상태 체크
            if (!gameObject.activeInHierarchy) 
                return false;
            
            // PlaceState의 실제 진입 가능 여부 체크 (게임 로직)
            var placeState = GetComponentInChildren<PlaceState>();
            if (placeState != null)
            {
                // CanEnter가 비활성화되어 있으면 게임 로직상 진입 불가능
                if (placeState.CanEnter != null && !placeState.CanEnter.activeInHierarchy)
                    return false;
                
                // CanNotEnter가 활성화되어 있으면 진입 불가능
                if (placeState.CanNotEnter != null && placeState.CanNotEnter.activeInHierarchy)
                    return false;
            }
            
            // IsDisabled는 플레이어 위치 기반 시스템용이므로 AI는 무시
            // AI는 실제 게임 로직(CanEnter/CanNotEnter)만 따름
            
            return true;
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeBlockedPlaces());
    }

    private IEnumerator InitializeBlockedPlaces()
    {
        yield return null;

        _placeName = GetComponentInChildren<PlaceState>().PlaceNameSetting.ToString();
        // PrintConnections();
    }

    public int GetActiveConnectCount()
    {
        int count = 0;
        foreach(var place in ConnectPlaces)
        {
            if(!place.IsDisabled)
            {
                count++;
            }
        }

        return count;
    }

    public void CheckIfBlocked()
    {
        var currentPlace = MovePlaceManager.Instance.CurrentPlace;

        // 현재 장소 또는 연결 정보가 없으면 리턴
        if (currentPlace == null || currentPlace.ConnectPlaces == null)
        {
         //   Debug.Log($"[CheckIfBlocked] {placeState.PlaceNameSetting}: 버튼 또는 현재 장소가 없음");
        return;
        }

        var thisPlaceState = GetComponentInChildren<PlaceState>();
        string thisPlaceName = thisPlaceState != null ? thisPlaceState.PlaceNameSetting.ToString() : name;
        
        // 연결된 장소에 포함되어 있지 않으면 무조건 비활성화
        bool isConnected = currentPlace.ConnectPlaces.Contains(this);
        if (!isConnected)
        {
            // 플레이어용 비활성화 설정 (AI에는 영향 없음)
            IsDisabledForPlayer = true;
            
            foreach (var btn in GetComponentsInChildren<Button>(true))
                btn.interactable = false;

            //Debug.Log($"[CheckIfBlocked] 연결되지 않음 → {thisPlaceName}: 버튼 비활성화됨");
        return;
        }

        // 연결되어 있으므로, CanEnter가 true일 경우만 버튼 활성화
        bool canEnter = thisPlaceState != null && thisPlaceState.CanEnter.activeSelf;
        
        // 플레이어 접근 가능 여부 설정 (AI에는 영향 없음)
        IsDisabledForPlayer = !canEnter;

        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            btn.interactable = canEnter;
        }

        //Debug.Log($"[CheckIfBlocked] 연결됨 → {thisPlaceName}: CanEnter={canEnter} → 버튼 {(canEnter ? "활성화" : "비활성화")}");
    }

    // AI 전용 접근 가능 여부 체크 (플레이어 UI 제약 무시) - 중복이므로 제거해도 됨
    // public bool IsAccessibleForAI()
    // {
    //     // AI는 기본적인 IsDisabled와 GameObject 활성화 상태만 체크
    //     return !IsDisabled && gameObject.activeInHierarchy;
    // }

    // 디버깅용 메서드
    public void LogConnectionStatus()
    {
        Debug.Log($"[{name}] 연결 상태:");
        Debug.Log($"  - IsDisabled: {IsDisabled} (플레이어 위치 기반, AI는 무시)");
        Debug.Log($"  - IsDisabledForPlayer: {IsDisabledForPlayer}");
        Debug.Log($"  - IsDisabledForAI: {IsDisabledForAI}");
        Debug.Log($"  - IsAccessibleForPlayer: {IsAccessibleForPlayer}");
        Debug.Log($"  - IsAccessibleForAI: {IsAccessibleForAI}");
        Debug.Log($"  - GameObject.active: {gameObject.activeInHierarchy}");
        
        var placeState = GetComponentInChildren<PlaceState>();
        if (placeState != null)
        {
            if (placeState.CanEnter != null)
                Debug.Log($"  - CanEnter.active: {placeState.CanEnter.activeInHierarchy}");
            if (placeState.CanNotEnter != null)
                Debug.Log($"  - CanNotEnter.active: {placeState.CanNotEnter.activeInHierarchy}");
        }
        
        Debug.Log($"  - 연결된 장소 수: {ConnectPlaces.Count}");
        
        foreach (var place in ConnectPlaces)
        {
            if (place != null)
            {
                Debug.Log($"    -> {place.GetComponentInChildren<PlaceState>().name}: IsAccessibleForAI={place.IsAccessibleForAI}");
            }
        }
    }
}

/*using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class PlaceConnector : MonoBehaviour
{
    public List<PlaceConnector> ConnectPlaces = new List<PlaceConnector>();

    [SerializeField]
    private string _placeName;

    public bool IsDisabled 
    { 
        get => _isDisabled;
        set 
        {
            if (_isDisabled != value)
            {
                Debug.Log($"[{Time.time:F1}초] {name}.IsDisabled 변경: {_isDisabled} → {value}");
                Debug.Log($"호출 스택: {System.Environment.StackTrace}");
            }
            _isDisabled = value;
        }
    }
    [SerializeField] private bool _isDisabled = false;
    
    // 추가된 부분
    public bool IsDisabledForPlayer = false;  // 플레이어만 접근 불가
    public bool IsDisabledForAI = false;      // AI만 접근 불가
    
    // 플레이어용 접근 가능 여부
    public bool IsAccessibleForPlayer => !IsDisabled && !IsDisabledForPlayer;
    
    // AI용 접근 가능 여부  
    public bool IsAccessibleForAI 
    { 
        get 
        {
            // AI 전용 차단 체크
            if (IsDisabledForAI) 
                return false;
            
            // GameObject 활성화 상태 체크
            if (!gameObject.activeInHierarchy) 
                return false;
            
            // PlaceState의 실제 진입 가능 여부 체크 (게임 로직)
            var placeState = GetComponentInChildren<PlaceState>();
            if (placeState != null)
            {
                // CanEnter가 비활성화되어 있으면 게임 로직상 진입 불가능
                if (placeState.CanEnter != null && !placeState.CanEnter.activeInHierarchy)
                    return false;
                
                // CanNotEnter가 활성화되어 있으면 진입 불가능
                if (placeState.CanNotEnter != null && placeState.CanNotEnter.activeInHierarchy)
                    return false;
            }
            
            // IsDisabled는 마지막에 체크 (다른 조건들이 더 중요)
            if (IsDisabled)
                return false;
            
            return true;
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeBlockedPlaces());
    }

    private IEnumerator InitializeBlockedPlaces()
    {
        yield return null;

        _placeName = GetComponentInChildren<PlaceState>().PlaceNameSetting.ToString();
        // PrintConnections();
    }

    public int GetActiveConnectCount()
    {
        int count = 0;
        foreach(var place in ConnectPlaces)
        {
            if(!place.IsDisabled)
            {
                count++;
            }
        }

        return count;
    }

    public void CheckIfBlocked()
    {
        var currentPlace = MovePlaceManager.Instance.CurrentPlace;

        // 현재 장소 또는 연결 정보가 없으면 리턴
        if (currentPlace == null || currentPlace.ConnectPlaces == null)
        {
         //   Debug.Log($"[CheckIfBlocked] {placeState.PlaceNameSetting}: 버튼 또는 현재 장소가 없음");
        return;
        }

        var thisPlaceState = GetComponentInChildren<PlaceState>();
        string thisPlaceName = thisPlaceState != null ? thisPlaceState.PlaceNameSetting.ToString() : name;
        
        // 연결된 장소에 포함되어 있지 않으면 무조건 비활성화
        bool isConnected = currentPlace.ConnectPlaces.Contains(this);
        if (!isConnected)
        {
            // 플레이어용 비활성화 설정 (AI에는 영향 없음)
            IsDisabledForPlayer = true;
            
            foreach (var btn in GetComponentsInChildren<Button>(true))
                btn.interactable = false;

            //Debug.Log($"[CheckIfBlocked] 연결되지 않음 → {thisPlaceName}: 버튼 비활성화됨");
        return;
        }

        // 연결되어 있으므로, CanEnter가 true일 경우만 버튼 활성화
        bool canEnter = thisPlaceState != null && thisPlaceState.CanEnter.activeSelf;
        
        // 플레이어 접근 가능 여부 설정 (AI에는 영향 없음)
        IsDisabledForPlayer = !canEnter;

        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            btn.interactable = canEnter;
        }

        //Debug.Log($"[CheckIfBlocked] 연결됨 → {thisPlaceName}: CanEnter={canEnter} → 버튼 {(canEnter ? "활성화" : "비활성화")}");
    }

    // AI 전용 접근 가능 여부 체크 (플레이어 UI 제약 무시) - 중복이므로 제거해도 됨
    // public bool IsAccessibleForAI()
    // {
    //     // AI는 기본적인 IsDisabled와 GameObject 활성화 상태만 체크
    //     return !IsDisabled && gameObject.activeInHierarchy;
    // }

    // 디버깅용 메서드
    public void LogConnectionStatus()
    {
        Debug.Log($"[{name}] 연결 상태:");
        Debug.Log($"  - IsDisabled: {IsDisabled}");
        Debug.Log($"  - IsDisabledForPlayer: {IsDisabledForPlayer}");
        Debug.Log($"  - IsDisabledForAI: {IsDisabledForAI}");
        Debug.Log($"  - IsAccessibleForPlayer: {IsAccessibleForPlayer}");
        Debug.Log($"  - IsAccessibleForAI: {IsAccessibleForAI}");
        Debug.Log($"  - GameObject.active: {gameObject.activeInHierarchy}");
        
        var placeState = GetComponentInChildren<PlaceState>();
        if (placeState != null)
        {
            if (placeState.CanEnter != null)
                Debug.Log($"  - CanEnter.active: {placeState.CanEnter.activeInHierarchy}");
            if (placeState.CanNotEnter != null)
                Debug.Log($"  - CanNotEnter.active: {placeState.CanNotEnter.activeInHierarchy}");
        }
        
        Debug.Log($"  - 연결된 장소 수: {ConnectPlaces.Count}");
        
        foreach (var place in ConnectPlaces)
        {
            if (place != null)
            {
                Debug.Log($"    -> {place.name}: IsAccessibleForAI={place.IsAccessibleForAI}");
            }
        }
    }
}*/
/*using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class PlaceConnector : MonoBehaviour
{
    public List<PlaceConnector> ConnectPlaces = new List<PlaceConnector>();

    [SerializeField]
    private string _placeName;

    public bool IsDisabled 
    { 
        get => _isDisabled;
        set 
        {
            if (_isDisabled != value)
            {
                Debug.Log($"[{Time.time:F1}초] {name}.IsDisabled 변경: {_isDisabled} → {value}");
                Debug.Log($"호출 스택: {System.Environment.StackTrace}");
            }
            _isDisabled = value;
        }
    }
    [SerializeField] private bool _isDisabled = false;
    
    // 추가된 부분
    public bool IsDisabledForPlayer = false;  // 플레이어만 접근 불가
    public bool IsDisabledForAI = false;      // AI만 접근 불가
    
    // 플레이어용 접근 가능 여부
    public bool IsAccessibleForPlayer => !IsDisabled && !IsDisabledForPlayer;
    
    // AI용 접근 가능 여부  
    public bool IsAccessibleForAI 
    { 
        get 
        {
            // 기본 비활성화 체크
            if (IsDisabled || IsDisabledForAI) 
                return false;
            
            // GameObject 활성화 상태 체크
            if (!gameObject.activeInHierarchy) 
                return false;
            
            // PlaceState의 실제 진입 가능 여부 체크 (게임 로직)
            var placeState = GetComponentInChildren<PlaceState>();
            if (placeState != null)
            {
                // CanEnter가 비활성화되어 있으면 게임 로직상 진입 불가능
                if (placeState.CanEnter != null && !placeState.CanEnter.activeInHierarchy)
                    return false;
                
                // CanNotEnter가 활성화되어 있으면 진입 불가능
                if (placeState.CanNotEnter != null && placeState.CanNotEnter.activeInHierarchy)
                    return false;
            }
            
            return true;
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeBlockedPlaces());
    }

    private IEnumerator InitializeBlockedPlaces()
    {
        yield return null;

        _placeName = GetComponentInChildren<PlaceState>().PlaceNameSetting.ToString();
        // PrintConnections();
    }

    public int GetActiveConnectCount()
    {
        int count = 0;
        foreach(var place in ConnectPlaces)
        {
            if(!place.IsDisabled)
            {
                count++;
            }
        }

        return count;
    }

    public void CheckIfBlocked()
    {
        var currentPlace = MovePlaceManager.Instance.CurrentPlace;

        // 현재 장소 또는 연결 정보가 없으면 리턴
        if (currentPlace == null || currentPlace.ConnectPlaces == null)
        {
         //   Debug.Log($"[CheckIfBlocked] {placeState.PlaceNameSetting}: 버튼 또는 현재 장소가 없음");
        return;
        }

        var thisPlaceState = GetComponentInChildren<PlaceState>();
        string thisPlaceName = thisPlaceState != null ? thisPlaceState.PlaceNameSetting.ToString() : name;
        
        // 연결된 장소에 포함되어 있지 않으면 무조건 비활성화
        bool isConnected = currentPlace.ConnectPlaces.Contains(this);
        if (!isConnected)
        {
            // 플레이어용 비활성화 설정 (AI에는 영향 없음)
            IsDisabledForPlayer = true;
            
            foreach (var btn in GetComponentsInChildren<Button>(true))
                btn.interactable = false;

            //Debug.Log($"[CheckIfBlocked] 연결되지 않음 → {thisPlaceName}: 버튼 비활성화됨");
        return;
        }

        // 연결되어 있으므로, CanEnter가 true일 경우만 버튼 활성화
        bool canEnter = thisPlaceState != null && thisPlaceState.CanEnter.activeSelf;
        
        // 플레이어 접근 가능 여부 설정 (AI에는 영향 없음)
        IsDisabledForPlayer = !canEnter;

        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            btn.interactable = canEnter;
        }

        //Debug.Log($"[CheckIfBlocked] 연결됨 → {thisPlaceName}: CanEnter={canEnter} → 버튼 {(canEnter ? "활성화" : "비활성화")}");
    }

    // AI 전용 접근 가능 여부 체크 (플레이어 UI 제약 무시) - 중복이므로 제거해도 됨
    // public bool IsAccessibleForAI()
    // {
    //     // AI는 기본적인 IsDisabled와 GameObject 활성화 상태만 체크
    //     return !IsDisabled && gameObject.activeInHierarchy;
    // }

    // 디버깅용 메서드
    public void LogConnectionStatus()
    {
        Debug.Log($"[{name}] 연결 상태:");
        Debug.Log($"  - IsDisabled: {IsDisabled}");
        Debug.Log($"  - IsDisabledForPlayer: {IsDisabledForPlayer}");
        Debug.Log($"  - IsDisabledForAI: {IsDisabledForAI}");
        Debug.Log($"  - IsAccessibleForPlayer: {IsAccessibleForPlayer}");
        Debug.Log($"  - IsAccessibleForAI: {IsAccessibleForAI}");
        Debug.Log($"  - GameObject.active: {gameObject.activeInHierarchy}");
        
        var placeState = GetComponentInChildren<PlaceState>();
        if (placeState != null)
        {
            if (placeState.CanEnter != null)
                Debug.Log($"  - CanEnter.active: {placeState.CanEnter.activeInHierarchy}");
            if (placeState.CanNotEnter != null)
                Debug.Log($"  - CanNotEnter.active: {placeState.CanNotEnter.activeInHierarchy}");
        }
        
        Debug.Log($"  - 연결된 장소 수: {ConnectPlaces.Count}");
        
        foreach (var place in ConnectPlaces)
        {
            if (place != null)
            {
                Debug.Log($"    -> {place.name}: IsAccessibleForAI={place.IsAccessibleForAI}");
            }
        }
    }
}*/
/*using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class PlaceConnector : MonoBehaviour
{
    public List<PlaceConnector> ConnectPlaces = new List<PlaceConnector>();

    [SerializeField]
    private string _placeName;

    public bool IsDisabled = false;
    
    // 추가된 부분
    public bool IsDisabledForPlayer = false;  // 플레이어만 접근 불가
    public bool IsDisabledForAI = false;      // AI만 접근 불가
    
    // 플레이어용 접근 가능 여부
    public bool IsAccessibleForPlayer => !IsDisabled && !IsDisabledForPlayer;
    
    // AI용 접근 가능 여부  
    public bool IsAccessibleForAI 
    { 
        get 
        {
            // 기본 비활성화 체크
            if (IsDisabled || IsDisabledForAI) 
                return false;
            
            // GameObject 활성화 상태 체크
            if (!gameObject.activeInHierarchy) 
                return false;
            
            // PlaceState의 실제 진입 가능 여부 체크 (게임 로직)
            var placeState = GetComponentInChildren<PlaceState>();
            if (placeState != null)
            {
                // CanEnter가 비활성화되어 있으면 게임 로직상 진입 불가능
                if (placeState.CanEnter != null && !placeState.CanEnter.activeInHierarchy)
                    return false;
                
                // CanNotEnter가 활성화되어 있으면 진입 불가능
                if (placeState.CanNotEnter != null && placeState.CanNotEnter.activeInHierarchy)
                    return false;
            }
            
            return true;
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeBlockedPlaces());
    }

    private IEnumerator InitializeBlockedPlaces()
    {
        yield return null;

        _placeName = GetComponentInChildren<PlaceState>().PlaceNameSetting.ToString();
        // PrintConnections();
    }

    public int GetActiveConnectCount()
    {
        int count = 0;
        foreach(var place in ConnectPlaces)
        {
            if(!place.IsDisabled)
            {
                count++;
            }
        }

        return count;
    }

    public void CheckIfBlocked()
    {
        var currentPlace = MovePlaceManager.Instance.CurrentPlace;

        // 현재 장소 또는 연결 정보가 없으면 리턴
        if (currentPlace == null || currentPlace.ConnectPlaces == null)
        {
         //   Debug.Log($"[CheckIfBlocked] {placeState.PlaceNameSetting}: 버튼 또는 현재 장소가 없음");
        return;
        }

        var thisPlaceState = GetComponentInChildren<PlaceState>();
        string thisPlaceName = thisPlaceState != null ? thisPlaceState.PlaceNameSetting.ToString() : name;
        
        // 연결된 장소에 포함되어 있지 않으면 무조건 비활성화
        bool isConnected = currentPlace.ConnectPlaces.Contains(this);
        if (!isConnected)
        {
            // 플레이어용 비활성화 설정 (AI에는 영향 없음)
            IsDisabledForPlayer = true;
            
            foreach (var btn in GetComponentsInChildren<Button>(true))
                btn.interactable = false;

            //Debug.Log($"[CheckIfBlocked] 연결되지 않음 → {thisPlaceName}: 버튼 비활성화됨");
        return;
        }

        // 연결되어 있으므로, CanEnter가 true일 경우만 버튼 활성화
        bool canEnter = thisPlaceState != null && thisPlaceState.CanEnter.activeSelf;
        
        // 플레이어 접근 가능 여부 설정 (AI에는 영향 없음)
        IsDisabledForPlayer = !canEnter;

        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            btn.interactable = canEnter;
        }

        //Debug.Log($"[CheckIfBlocked] 연결됨 → {thisPlaceName}: CanEnter={canEnter} → 버튼 {(canEnter ? "활성화" : "비활성화")}");
    }

    // AI 전용 접근 가능 여부 체크 (플레이어 UI 제약 무시) - 중복이므로 제거해도 됨
    // public bool IsAccessibleForAI()
    // {
    //     // AI는 기본적인 IsDisabled와 GameObject 활성화 상태만 체크
    //     return !IsDisabled && gameObject.activeInHierarchy;
    // }

    // 디버깅용 메서드
    public void LogConnectionStatus()
    {
        Debug.Log($"[{name}] 연결 상태:");
        Debug.Log($"  - IsDisabled: {IsDisabled}");
        Debug.Log($"  - IsDisabledForPlayer: {IsDisabledForPlayer}");
        Debug.Log($"  - IsDisabledForAI: {IsDisabledForAI}");
        Debug.Log($"  - IsAccessibleForPlayer: {IsAccessibleForPlayer}");
        Debug.Log($"  - IsAccessibleForAI: {IsAccessibleForAI}");
        Debug.Log($"  - GameObject.active: {gameObject.activeInHierarchy}");
        
        var placeState = GetComponentInChildren<PlaceState>();
        if (placeState != null)
        {
            if (placeState.CanEnter != null)
                Debug.Log($"  - CanEnter.active: {placeState.CanEnter.activeInHierarchy}");
            if (placeState.CanNotEnter != null)
                Debug.Log($"  - CanNotEnter.active: {placeState.CanNotEnter.activeInHierarchy}");
        }
        
        Debug.Log($"  - 연결된 장소 수: {ConnectPlaces.Count}");
        
        foreach (var place in ConnectPlaces)
        {
            if (place != null)
            {
                Debug.Log($"    -> {place.name}: IsAccessibleForAI={place.IsAccessibleForAI}");
            }
        }
    }
}*/
/*using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class PlaceConnector : MonoBehaviour
{
    public List<PlaceConnector> ConnectPlaces = new List<PlaceConnector>();

    [SerializeField]
    private string _placeName;

    public bool IsDisabled = false;

    //추가
    public bool IsDisabledForPlayer = false;  // 플레이어만 접근 불가
    public bool IsDisabledForAI = false;      // AI만 접근 불가
    // 플레이어용 접근 가능 여부
    public bool IsAccessibleForPlayer => !IsDisabled && !IsDisabledForPlayer;
    // AI용 접근 가능 여부  
    public bool IsAccessibleForAI => !IsDisabled && !IsDisabledForAI;



    private void Start()
    {
        StartCoroutine(InitializeBlockedPlaces());
    }

    private IEnumerator InitializeBlockedPlaces()
    {
        yield return null;

        _placeName = GetComponentInChildren<PlaceState>().PlaceNameSetting.ToString();
       // PrintConnections();
    }

    public int GetActiveConnectCount()
    {
        int count = 0;
        foreach(var place in ConnectPlaces)
        {
            if(!place.IsDisabled)
            {
                count++;
            }
        }

        return count;
    }


    public void CheckIfBlocked()
    {
        var currentPlace = MovePlaceManager.Instance.CurrentPlace;

        // 현재 장소 또는 연결 정보가 없으면 리턴
        if (currentPlace == null || currentPlace.ConnectPlaces == null)
        {
         //   Debug.Log($"[CheckIfBlocked] {placeState.PlaceNameSetting}: 버튼 또는 현재 장소가 없음");
        return;
        }

        var thisPlaceState = GetComponentInChildren<PlaceState>();
        string thisPlaceName = thisPlaceState != null ? thisPlaceState.PlaceNameSetting.ToString() : name;

       // 연결된 장소에 포함되어 있지 않으면 무조건 비활성화
        bool isConnected = currentPlace.ConnectPlaces.Contains(this);
        if (!isConnected)
        {
            foreach (var btn in GetComponentsInChildren<Button>(true))
                btn.interactable = false;

            //Debug.Log($"[CheckIfBlocked] 연결되지 않음 → {thisPlaceName}: 버튼 비활성화됨");
        return;
        }

        // 연결되어 있으므로, CanEnter가 true일 경우만 버튼 활성화
        bool canEnter = thisPlaceState != null && thisPlaceState.CanEnter.activeSelf;

        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            btn.interactable = canEnter;
        }

         //Debug.Log($"[CheckIfBlocked] 연결됨 → {thisPlaceName}: CanEnter={canEnter} → 버튼 {(canEnter ? "활성화" : "비활성화")}");
    }






}
*/