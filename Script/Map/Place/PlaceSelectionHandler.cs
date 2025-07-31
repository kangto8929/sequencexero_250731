using UnityEngine;
using UnityEngine.UI;

public class PlaceSelectionHandler : MonoBehaviour
{
    [SerializeField]
    private PlaceState _placeState;

    [SerializeField]
    private PlaceItemRegion _placeItemRegion;
    private void Awake()
    {
        _placeState.PlaceStatusChanged += OnPlaceStatusChanged;
    }

    private void OnDestroy()
    {
        _placeState.PlaceStatusChanged -= OnPlaceStatusChanged;
    }

    public void OnPlaceSelected()
    {
       /* ItemSearchManager.Instance.EnemyCharacter.SetActive(false);
       // SFX_Manager.Instance.ButtonSFX();

        _placeState.UpdatePlaceStatus();

        Button placeButton = GetComponent<Button>();
        if (_placeState.CanNotEnter.activeSelf == true)
        {
            placeButton.interactable = false;
            return;
        }

        MovePlaceManager.Instance.MoveToPlace(_placeState);

        // 현재 탐색 장소를 PlaceItemManager에 설정
        PlaceItemManager.Instance.CurrentRegion = _placeItemRegion;
        */
        //ItemSearchManager.Instance.EnemyCharacter.SetActive(false);
        ItemSearchManager.Instance.AllEnemies.ForEach(enemy => enemy.EnemyCharacter.SetActive(false));
        
        // SFX_Manager.Instance.ButtonSFX();

        // 팝업 UI 상태 확인
        if (ItemSearchManager.Instance != null && ItemSearchManager.Instance.DiscoveryParent != null)
        {
            // DiscoveryPanret의 자식이 있는지 확인
            if (ItemSearchManager.Instance.DiscoveryParent.childCount > 0)
            {
                // 0번째 자식 가져오기
                Transform firstChild = ItemSearchManager.Instance.DiscoveryParent.GetChild(0);

                // 자식에 ItemPopupUI 컴포넌트가 있는지 확인
                ItemPopupUI itemPopup = firstChild.GetComponent<ItemPopupUI>();
                if (itemPopup != null)
                {
                    // ItemPopupUI의 XButton이 할당되어 있고 활성화 상태인지 확인
                    if (itemPopup.XButton != null && itemPopup.XButton.activeSelf)
                    {
                        //Debug.Log("ItemPopupUI의 XButton이 현재 활성화되어 있습니다.");
                        itemPopup.OnXButtonClicked();
                         Debug.LogWarning("가방에 보관중.");
                        // 여기에 XButton이 활성화되어 있을 때 수행할 추가 로직을 넣을 수 있습니다.
                        // 예: 장소 이동을 막거나, 경고 메시지를 표시하거나 등
                    }
                    else if (itemPopup.XButton != null && !itemPopup.XButton.activeSelf)
                    {
                        //Debug.Log("ItemPopupUI의 XButton은 비활성화되어 있습니다.");
                        itemPopup.OnDiscardClicked();
                        itemPopup.OnXButtonClicked();
                        Debug.LogWarning("네 탓이야 버리고 와.");
                        
                    }

                    
                    else
                    {
                      //  return;
                        Debug.LogWarning("ItemPopupUI에 XButton이 할당되어 있지 않습니다.");
                    }
                }
                else
                {
                   // return;
                    Debug.Log("DiscoveryPanret의 0번째 자식에 ItemPopupUI 컴포넌트가 없습니다.");
                }
            }
            else
            {
                //return;
                Debug.Log("DiscoveryPanret에 자식 오브젝트가 없습니다.");
            }
        }
        else
        {
            //return;
            Debug.LogWarning("ItemSearchManager.Instance 또는 DiscoveryPanret가 null입니다.");
        }


        _placeState.UpdatePlaceStatus();

        Button placeButton = GetComponent<Button>();
        if (_placeState.CanNotEnter.activeSelf == true)
        {
            placeButton.interactable = false;
            return;
        }

        MovePlaceManager.Instance.MoveToPlace(_placeState);

        // 현재 탐색 장소를 PlaceItemManager에 설정
        PlaceItemManager.Instance.CurrentRegion = _placeItemRegion;
    }

    private void OnPlaceStatusChanged(PlaceStatus newStatus)
    {
        //Debug.Log($"OnPlaceStatusChanged 현재 상태: {newStatus}");
        UI_InGameManager.Instance.UpdateStatusIcon(newStatus);
    }
}

