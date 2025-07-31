using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

public class AIMovePlaceManager : MonoBehaviour
{
    
    private PlaceConnector _currentPlace;
    public PlaceState CurrentPlaceName;
    public PlaceNameType CurrentPlaceNameType = PlaceNameType.None;

    // CurrentPlace 프로퍼티 추가 (EnemyMoveManager에서 접근용)
    public PlaceConnector CurrentPlace => _currentPlace;


    public void Initialize(PlaceConnector startPlace, PlaceState startState)
    {
        _currentPlace = startPlace;
        CurrentPlaceName = startState;

        // AI는 무조건 None으로 시작 (모든 장소 이동 가능)
        CurrentPlaceNameType = PlaceNameType.None;

        Debug.Log($"[AI 초기화] 시작 장소: {_currentPlace?.name ?? "null"} | 상태: {CurrentPlaceNameType} (강제 None 설정)");

        // 초기화 시 연결된 장소들의 IsDisabled 상태 확인
        if (_currentPlace != null)
        {
            Debug.Log($"[AI 초기화] {_currentPlace.name}에서 연결된 장소들 상태:");
            foreach (var place in _currentPlace.ConnectPlaces)
            {
                Debug.Log($"  - {place.name}: IsDisabled={place.IsDisabled}");
            }
        }
    }

    public PlaceConnector FindSafeDestination()
{
    var safePlace = BlockedPlaceSetter.Instance?.SelectedFinalConfig?.FinalSafePlace;
    if (safePlace != null && safePlace.IsAccessibleForAI)
    {
        Debug.LogWarning($"[AI] {gameObject.name} 안전 장소로 이동 시도: {safePlace.name}");
        return safePlace;
    }
    Debug.LogWarning("안전한 장소를 찾지 못했습니다.");
    return null;
}

public bool TryEscapeSmart()
{
    if (TryEscapeToSafePlace())
        return true;

    return MoveToEscapeStep();
}

// 안전 장소로 탈출 시도
private bool TryEscapeToSafePlace()
{
    var safePlace = FindSafeDestination();
    if (safePlace == null)
        return false;

    return MoveToPlace(safePlace);
}


    public PlaceConnector FindEscapeStep()
{
    if (_currentPlace == null)
    {
        Debug.LogWarning("현재 위치가 없습니다.");
        return null;
    }

    

    List<PlaceConnector> escapePath = AIEscapePathfinder.FindEscapeRoute(_currentPlace);

    if (escapePath != null && escapePath.Count > 0)
    {
        Debug.Log($"{gameObject.name} 탈출 경로 첫 단계: {escapePath[0].name}");
        return escapePath[0];
    }
    else
    {
        Debug.Log("탈출 가능한 경로가 없습니다.");
        return null;
    }
}

    // 새 함수: 탈출 경로가 있으면 그 쪽으로 이동 시도
    public bool MoveToEscapeStep()
    {
        PlaceConnector step = FindEscapeStep();
        if (step == null)
            return false;

        return MoveToPlace(step);
    }

    public bool MoveToPlace(PlaceConnector targetPlace)
    {
        if (targetPlace == null)
            return false;

        // None 상태면 연결 확인 없이 이동 가능
        if (CurrentPlaceNameType == PlaceNameType.None)
        {
            Debug.Log("[AI 이동] 현재 위치가 None이므로 연결 무시하고 이동 허용");
            UpdateCurrentPlace(targetPlace);
            return true;
        }

        // AI는 실제 연결 상태와 AI 접근 가능 여부만 확인
        if (!_currentPlace.ConnectPlaces.Contains(targetPlace) || !targetPlace.IsAccessibleForAI)
        {
            Debug.LogWarning($"[AI 이동 실패] AI 접근 불가능한 장소: {targetPlace.name}");
            return false;
        }

        UpdateCurrentPlace(targetPlace);
        return true;
    }

    private void UpdateCurrentPlace(PlaceConnector newPlace)
    {
        _currentPlace = newPlace;
        CurrentPlaceName = newPlace.GetComponentInChildren<PlaceState>();
        CurrentPlaceNameType = CurrentPlaceName != null ? CurrentPlaceName.PlaceNameSetting : PlaceNameType.None;

        Debug.Log($"[AI 위치 갱신] 새 장소: {CurrentPlaceNameType}");
    }

    public List<PlaceConnector> GetAvailableMoves()
    {
        //Debug.Log($"[GetAvailableMoves] 현재 CurrentPlaceNameType: {CurrentPlaceNameType}");
        //Debug.Log($"[GetAvailableMoves] None 체크: {CurrentPlaceNameType == PlaceNameType.None}");

        List<PlaceConnector> available = new List<PlaceConnector>();
        var allConnectors = UnityEngine.Object.FindObjectsOfType<PlaceConnector>();

        if (CurrentPlaceNameType == PlaceNameType.None)
        {
            Debug.Log("[AI 이동 가능 확인] None 상태이므로 모든 장소 이동 가능");
            foreach (var connector in allConnectors)
            {
                if (connector.IsAccessibleForAI)
                    available.Add(connector);
            }
            //Debug.Log($"[AI None 상태] 이동 가능한 장소 수: {available.Count}");
            return available;
        }

        Debug.Log($"[AI 일반 상태] {CurrentPlaceNameType}에서 연결된 장소 확인");

        // AI는 실제 연결 상태만 체크 (플레이어 UI 제약 무시)
        foreach (var place in _currentPlace.ConnectPlaces)
        {
            if (place != null && place.IsAccessibleForAI)
            {
                Debug.Log($"  - 이동 가능: {place.GetComponentInChildren<PlaceState>().name}");
                available.Add(place);
            }
            else if (place != null)
            {
                Debug.Log($"  - 이동 불가능: {place.name} (IsAccessibleForAI: {place.IsAccessibleForAI})");
                Debug.Log($"현재 장소 {CurrentPlaceNameType}");
            }
        }

        Debug.Log($"[AI 일반 상태] 이동 가능한 장소 수: {available.Count} 곳곳");
        return available;
    }
}

