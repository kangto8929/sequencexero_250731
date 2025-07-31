using UnityEngine;
using System.Collections.Generic;

public class PlaceConnectorManager : MonoBehaviour
{
    public static PlaceConnectorManager Instance;

    // 모든 PlaceConnector를 리스트로 관리
    public List<PlaceConnector> AllPlaces = new List<PlaceConnector>();

    private void Awake()
    {
        Instance = this;
    }

    // PlaceNameType에 맞는 PlaceConnector 반환
    public PlaceConnector GetPlaceConnectorByPlaceName(PlaceNameType placeName)
    {
        foreach(var place in AllPlaces)
        {
            var placeState = place.GetComponentInChildren<PlaceState>();
            if(placeState != null && placeState.PlaceNameSetting == placeName)
            {
                return place;
            }
        }

        Debug.LogWarning($"[GetPlaceConnectorByPlaceName] {placeName}에 해당하는 PlaceConnector가 없습니다.");
        return null;
    }
}
