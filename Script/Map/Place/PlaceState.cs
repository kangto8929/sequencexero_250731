using UnityEngine;
using UnityEngine.UI;

public class PlaceState : MonoBehaviour
{
    public PlaceNameType PlaceNameSetting;

    public GameObject CanEnter;
    public GameObject CanNotEnter;

    public GameObject BewareSystemCollapseIcon;
    public GameObject AlreadySystemCollapseIcon;

    public Sprite DayPlaceImage;
    public Sprite NightPlaceImage;

    public GameObject PlayerPlaced;//플레이어가 현재 있는 장소소

    public delegate void OnPlaceStatusChanged(PlaceStatus newStatus);
    public event OnPlaceStatusChanged PlaceStatusChanged;

    public void SetPlaceStatus(PlaceStatus newStatus)
    {
       // Debug.Log($"SetPlaceStatus ȣ��: {newStatus}");

        switch (newStatus)
        {
            case PlaceStatus.Danger:
                BewareSystemCollapseIcon?.SetActive(true);
                AlreadySystemCollapseIcon?.SetActive(false);
                break;
            case PlaceStatus.Penalty:
                BewareSystemCollapseIcon?.SetActive(false);
                AlreadySystemCollapseIcon?.SetActive(true);
                break;
            default:
                BewareSystemCollapseIcon?.SetActive(false);
                AlreadySystemCollapseIcon?.SetActive(false);
                break;
        }

        //UI������Ʈ
        PlaceStatusChanged?.Invoke(newStatus);
    }

    public PlaceStatus GetCurrentStatus()
    {
        if (AlreadySystemCollapseIcon != null && AlreadySystemCollapseIcon.activeSelf)
            return PlaceStatus.Penalty;
        else if (BewareSystemCollapseIcon != null && BewareSystemCollapseIcon.activeSelf)
            return PlaceStatus.Danger;
        return PlaceStatus.None;
    }

    public void UpdatePlaceStatus()
    {
        var newStatus = GetCurrentStatus();
        PlaceStatusChanged?.Invoke(newStatus);
    }




}
