using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BlockedPlaceApplier : MonoBehaviour
{
    private string _blockX = "BlockPlaceX";

    public void ApplyDay(int dayIndex)
    {
        List<PlaceConnector> setBlockedPlaces = BlockedPlaceSetter.Instance.BlockPlacesByDays[dayIndex].Places;

        foreach (var blockedPlace in setBlockedPlaces)
        {
            blockedPlace.IsDisabled = true;

            //  BlockPlaceX 태그 가진 자식 오브젝트 활성화
            foreach(Transform child in blockedPlace.transform)
            {
                if(child.CompareTag(_blockX))
                {
                    child.gameObject.SetActive(true);

                    Button[] siblingButtons = blockedPlace.GetComponentsInChildren<Button>(true);
                    foreach (var btn in siblingButtons)
                    {
                        btn.interactable = false;
                    }

                }
            }

            var placeName = blockedPlace.GetComponentInChildren<PlaceState>();
            if (placeName != null)
            {
                placeName.CanEnter.SetActive(false);
                placeName.CanNotEnter.SetActive(true);
                placeName.BewareSystemCollapseIcon.SetActive(false);
                placeName.AlreadySystemCollapseIcon.SetActive(false);
                
                /*Button[] buttons = blockedPlace.GetComponentsInChildren<Button>();
                foreach (var btn in buttons)
                {
                    btn.interactable = false;
                }*/

            }


            Button[] buttons = blockedPlace.GetComponentsInChildren<Button>();
            foreach (var btn in buttons)
            {
                btn.interactable = false;
            }
        }

        //RefreshConnections();
    }
    

    public void RestoreDay(int dayIndex)
    {
        List<PlaceConnector> willNotBlockedPlaces = BlockedPlaceSetter.Instance.BlockPlacesByDays[dayIndex].Places;

        foreach (var restorePlace in willNotBlockedPlaces)
        {
            restorePlace.IsDisabled = false;

            foreach (Transform child in restorePlace.transform)
            {
                if (child.CompareTag(_blockX))
                {
                    child.gameObject.SetActive(false);
                }
            }

            var placeName = restorePlace.GetComponentInChildren<PlaceState>();
            if (placeName != null)
            {
                placeName.CanEnter.SetActive(true);
                placeName.CanNotEnter.SetActive(false);
                placeName.AlreadySystemCollapseIcon.SetActive(true);


                MovePlaceManager.Instance.UpdateAllPlaceButtons();
                MovePlaceManager.Instance.RefreshMovablePlaces();
                 Debug.Log($"[RestoreDay] 금지구역 이미지 비활성화됨: 장소명: {placeName.name})");
            }
            else
            {
               Debug.LogWarning($"[RestoreDay] PlaceName 컴포넌트가 없습니다: {placeName}");
            }


        }

        //RefreshConnections();
    }

    public void ShowBlockedPlaces(int dayIndex)
    {
        if (dayIndex < 0 || dayIndex >= BlockedPlaceSetter.Instance.BlockPlacesByDays.Count)
        {
            Debug.LogWarning("잘못된 일차 인덱스입니다.");
            return;
        }

        Debug.Log($"{dayIndex + 1}일차 금지구역 목록:");
        foreach (var place in BlockedPlaceSetter.Instance.BlockPlacesByDays[dayIndex].Places)
        {
            PlaceState nameComponent = place.GetComponentInChildren<PlaceState>();
            nameComponent.BewareSystemCollapseIcon.SetActive(true);
        }

       
        

    }

    /*private void RefreshConnections()
    {
        foreach (var place in BlockedPlaceSetter.Instance.AllPlaces)
        {
            place.CheckIfBlocked();
        }
    }*/


    public void UpdatePlaceButtonactivity()
    {
        var currentPlace = MovePlaceManager.Instance.CurrentPlace;

        foreach (var place in BlockedPlaceSetter.Instance.AllPlaces)
        {
            Button[] buttons = place.GetComponentsInChildren<Button>(true);
            var placeState = place.GetComponentInChildren<PlaceState>();

            bool isConnected = currentPlace != null && currentPlace.ConnectPlaces.Contains(place);
            bool isBlocked = place.IsDisabled;
            bool isCanNotEnterActive = placeState != null && placeState.CanNotEnter.activeSelf;

            bool shouldDisable = !isConnected || isBlocked || isCanNotEnterActive;

            foreach (var button in buttons)
            {
                button.interactable = !shouldDisable;
            }

            if (placeState != null)
            {
                placeState.CanEnter.SetActive(!shouldDisable);
                placeState.CanNotEnter.SetActive(shouldDisable);
            }
        }
    }


    


}

