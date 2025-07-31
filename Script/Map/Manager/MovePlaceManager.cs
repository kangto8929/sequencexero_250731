using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class MovePlaceManager : MonoBehaviour
{
    public SkillCoolTimeController DetectiveskillCoolTimeController;
    public SkillUIManager SkillUIManagerInstance;
    public DetectivePlaceinfo DetectivePlaceinfo;

    public PlaceConnector CurrentPlace;
    public PlaceState CurrentPlaceName;
    public PlaceNameType CurrentPlaceNameType = PlaceNameType.None;

    [SerializeField]
    private InGameButtonUI _uiInGameButton;

    public static MovePlaceManager Instance;

    [Header("Place Transition Settings")]
    public Image BackgroundImage; // 배경 이미지
    public Image FadeImage;       // 페이드 효과 이미지 (페이드인)
    public float FadeDuration = 1f;

    private void Awake()
    {
        Instance = this;
        // DOTween.Init(); // 필요하면 활성화
    }

    private void Start()
    {
        // 처음 CurrentPlace 자동 설정
        if (CurrentPlace == null && CurrentPlaceName != null)
        {
            CurrentPlace = CurrentPlaceName.GetComponentInParent<PlaceConnector>();
            //Debug.Log($"[Start] 초기 CurrentPlace 설정됨: {CurrentPlace?.name ?? "null"}");
        }

        CurrentPlaceName = null;
        CurrentPlaceNameType = PlaceNameType.None;

        if (FadeImage != null)
        {
            FadeImage.color = new Color(FadeImage.color.r, FadeImage.color.g, FadeImage.color.b, 0f);
            FadeImage.raycastTarget = false;
        }

        if (CurrentPlaceNameType == PlaceNameType.None)
        {
            UI_InGameManager.Instance.UpdateStatusIcon(PlaceStatus.Danger);
            //Debug.Log("초기 위치가 None이므로 Danger 아이콘 강제 표시");
        }

        RefreshMovablePlaces();
        UpdateAllPlaceButtons();

        // 게임 시작 시 스킬 쿨타임 초기화 (장소가 "없음" 상태)
        if (DetectiveskillCoolTimeController != null)
        {
            Debug.Log("[MovePlaceManager] 게임 시작 - 스킬 쿨타임 초기 상태 설정");
            // Start()에서 이미 초기화되므로 추가 호출 불필요
        }
    }



    public void MoveToPlace(PlaceState selectedPlace)
    {
        //Debug.Log($"[이동 시도] 현재 장소: {CurrentPlaceName?.PlaceNameSetting.ToString() ?? "없음"}, 이동할 장소: {selectedPlace.PlaceNameSetting}");
        PlaceState previousPlace = CurrentPlaceName; // 이전 장소 미리 저장
       
        if (CurrentPlaceName == selectedPlace)
        {
            if (_uiInGameButton != null && _uiInGameButton.InGame != null)
            {
                _uiInGameButton.InGame.SetActive(true);
            }
          // Debug.Log("[이동 취소] 현재 장소와 이동할 장소가 동일합니다.");
            return;
        }

        // 장소 이동 전 현재 상태 저장
        string previousPlaceName = CurrentPlaceName?.PlaceNameSetting.ToString() ?? "없음";
        
        CurrentPlaceName = selectedPlace;
        CurrentPlaceNameType = selectedPlace.PlaceNameSetting;
        CurrentPlace = CurrentPlaceName.GetComponentInParent<PlaceConnector>();

        //Debug.Log($"[이동 성공!] 장소 이동: {previousPlaceName} → {CurrentPlaceNameType}");

        //// ★ 여기서 CurrentRegion 업데이트 ★
    if (PlaceItemManager.Instance != null)
    {
        PlaceItemManager.Instance.SetCurrentRegion(CurrentPlaceNameType);
        //Debug.Log($"[MovePlaceManager] PlaceItemManager의 CurrentRegion이 {CurrentPlaceNameType}으로 갱신됨");
    }
    else
    {
        Debug.LogWarning("[MovePlaceManager] PlaceItemManager 인스턴스가 없습니다.");
    }

    // 스킬 쿨타임 처리 - 장소 이동 시 호출
    if (DetectiveskillCoolTimeController != null)
    {
        //Debug.Log("[MovePlaceManager] 장소 이동 완료 - 스킬 쿨타임 처리 시작");
        DetectiveskillCoolTimeController.OnMovedToOtherPlace();
    }
    //여기까지 CurrentRegion업데이트트

        // 스킬 쿨타임 처리 - 장소 이동 시 호출
        if (DetectiveskillCoolTimeController != null)
        {
            //Debug.Log("[MovePlaceManager] 장소 이동 완료 - 스킬 쿨타임 처리 시작");
            DetectiveskillCoolTimeController.OnMovedToOtherPlace();
        }

        _uiInGameButton.MapNotTouchImage.SetActive(true);

        FadeImage.DOFade(1f, FadeDuration).OnStart(() =>
        {
            FadeImage.raycastTarget = true;
        })
        .OnComplete(() =>
        {
            Sprite spaceSprite = selectedPlace.DayPlaceImage;

            if (TimeFlowManager.Instance != null && TimeFlowManager.Instance.IsNightStep())
            {
                if (selectedPlace.NightPlaceImage != null)
                {
                    spaceSprite = selectedPlace.NightPlaceImage;
                }
            }

            selectedPlace.PlayerPlaced.SetActive(true);
            previousPlace?.PlayerPlaced.SetActive(false);

            if (_uiInGameButton.InGame == null)
            {
                Debug.LogError("_uiGameButton.InGame is null!");
            }
            else
            {
                _uiInGameButton.InGame.SetActive(true);
            }

            BackgroundImage.sprite = spaceSprite;

            FadeImage.DOFade(0f, FadeDuration).OnComplete(() =>
            {
                FadeImage.raycastTarget = false;
               // Debug.Log("페이드 인 완료");

                RefreshMovablePlaces();
                UpdateAllPlaceButtons();

                //현재 위치 어딘지 확인인
        bool prepared = ItemSearchManager.Instance.PrepareSearch();
    //Debug.Log(prepared ? "탐색 준비 완료" : "탐색 준비 실패");
            });

            //적을 만난 상태에서 탈출한 경우 다시 탐색할 수 있게
            ItemSearchManager.Instance.SearchButton.gameObject.SetActive(true);


            //코루틴 여기서 // 탐정 스킬 관련
                // ✅ 쿨타임 코루틴 실행 시도
    if (DetectiveskillCoolTimeController != null)
    {
        if (DetectiveskillCoolTimeController.gameObject.activeInHierarchy)
        {
           // Debug.Log("[쿨타임 시작 시도] 페이드 완료 후 쿨타임 코루틴 실행");
           StartCoroutine(DetectiveskillCoolTimeController.CooldownRoutine());
        }
        /*else
        {
            Debug.LogWarning($"[쿨타임 실행 실패] {DetectiveskillCoolTimeController.gameObject.name} 오브젝트가 비활성 상태입니다.");
        }*/
    }
        });

//여기까지 탐정 스킬

    }

    public void UpdateCurrentPlaceBackground()
    {
        if (CurrentPlaceName == null)
        {
            Debug.LogWarning("[UpdateCurrentPlaceBackground] CurrentPlaceName이 설정되지 않음");
            //return;
        }

        Sprite spaceSprite = CurrentPlaceName.DayPlaceImage;

        if (TimeFlowManager.Instance != null && TimeFlowManager.Instance.IsNightStep())
        {
            if (CurrentPlaceName.NightPlaceImage != null)
            {
                spaceSprite = CurrentPlaceName.NightPlaceImage;
            }
        }

        BackgroundImage.sprite = spaceSprite;
        //Debug.Log("시간 변경에 따라 배경 이미지 갱신");
    }

    // 연결된 장소만 활성화
    public void RefreshMovablePlaces()
    {
        var allConnectors = FindObjectsOfType<PlaceConnector>();

        // 장소가 None일 경우 → 전부 이동 가능
        if (CurrentPlaceName == null || CurrentPlaceNameType == PlaceNameType.None)
        {
            foreach (var connector in allConnectors)
            {
                connector.IsDisabled = false;
                connector.CheckIfBlocked();
            }
            return;
        }

        var currentConnector = CurrentPlaceName.GetComponentInParent<PlaceConnector>();
        if (currentConnector == null)
        {
           // Debug.LogError("현재 장소에 연결된 Connector가 없음.");
            return;
        }

        foreach (var connector in allConnectors)
        {
            bool isConnected = currentConnector.ConnectPlaces.Contains(connector);
            connector.IsDisabled = !isConnected;
            connector.CheckIfBlocked();
        }
    }

    // 연결된 장소 중 CanEnter 활성 상태에 따라 버튼 활성/비활성 처리
    public void UpdateAllPlaceButtons()
    {
        if (CurrentPlace == null)
        {
            //Debug.LogWarning($"[UpdateAllPlaceButtons] 현재 장소가 없습니다. CurrentPlaceName: {(CurrentPlaceName != null ? CurrentPlaceName.PlaceNameSetting.ToString() : "null")}");
            return;
        }

        var allConnectors = FindObjectsOfType<PlaceConnector>();

        foreach (var connector in allConnectors)
        {
            bool isConnected = CurrentPlace.ConnectPlaces.Contains(connector);
            var placeState = connector.GetComponentInChildren<PlaceState>();

            // 연결된 장소만 처리
            if (isConnected && placeState != null)
            {
                bool canEnter = placeState.CanEnter.activeSelf;

                if (connector.transform.childCount > 0)
                {
                    Transform firstChild = connector.transform.GetChild(0);
                    Button button = firstChild.GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = canEnter;
                        //Debug.Log($"[버튼 처리] 연결됨: {placeState.PlaceNameSetting} | CanEnter: {canEnter} | 버튼 {(canEnter ? "활성화" : "비활성화")}");
                    }
                }
            }
            else
            {
                // 연결되지 않은 장소는 버튼 비활성화
                if (connector.transform.childCount > 0)
                {
                    Transform firstChild = connector.transform.GetChild(0);
                    Button button = firstChild.GetComponent<Button>();
                    if (button != null)
                    {
                        button.interactable = false;
                        var ps = connector.GetComponentInChildren<PlaceState>();
                        string name = ps != null ? ps.PlaceNameSetting.ToString() : connector.name;
                        //Debug.Log($"[버튼 처리] 연결되지 않음: {name} → 버튼 비활성화");
                    }
                }
            }
        }
    }
}