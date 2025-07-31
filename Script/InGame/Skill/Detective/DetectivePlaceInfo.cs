using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetectivePlaceinfo : MonoBehaviour
{
    [Header("InfomationPanel")]
    public GameObject InformationPanel;  // public으로 유지 (SkillCoolTimeController에서 접근)
    [SerializeField]
    private RectTransform _middlePart;
    public TextMeshProUGUI DialogText;

    [Header("WritingSettings")]
    public float TopPadding = 30f;
    public float LeftRightMargin = 40f;
    public float BottomMargin = 0f;

    private LayoutElement _middleLayout;
    private int _currentTouchCount = 0;

    void Awake()
    {
        _middleLayout = _middlePart.GetComponent<LayoutElement>();
    }

    void Start()
    {
        _currentTouchCount = 0;
        InformationPanel.SetActive(false);
    }

    void Update()
    {
        if (InformationPanel.activeSelf)
        {
            UpdateBubble();//활성화 된 상태면 현재 몇 개 남았는지 보여주는 걸로로
            TryShowCurrentPlaceItems();
        }
    }

    public void OnClickTouchCount() // 버튼 전용 함수
    {
        SetTouchCount(true); // 무조건 효과음 나오게
    }

    public void SetTouchCount(bool playSound = true)
    {
        _currentTouchCount++; // 터치 카운트 증가

        if (_currentTouchCount % 2 == 0)
        {
            InformationPanel.SetActive(false);
            // 사운드 없음
        }
        else
        {
            InformationPanel.SetActive(true);

            // 아이템 정보 출력
            // TryShowCurrentPlaceItems();

            if (playSound)
            {
                return;
                //SoundManager.Instance.ButtonSFX();
            }
        }
    }

    //현재 위치 아이템 출력
    public void TryShowCurrentPlaceItems()
    {
        var currentRegion = MovePlaceManager.Instance?.CurrentPlaceName?.GetComponent<PlaceItemRegion>();
        if (currentRegion == null)
        {
            SetDialogText("이 지역에는 아이템이 없습니다.");
            return;
        }

        string infoText = currentRegion.GetFormattedItemList();
        SetDialogText(infoText);
    }

    public void UpdateBubble()
    {
        DialogText.margin = new Vector4(LeftRightMargin, TopPadding, LeftRightMargin, BottomMargin);
        LayoutRebuilder.ForceRebuildLayoutImmediate(DialogText.rectTransform);
        float targetHeight = DialogText.preferredHeight + DialogText.margin.y + DialogText.margin.w;
        _middleLayout.preferredHeight = targetHeight;
    }

    public void SetDialogText(string text)
    {
        DialogText.text = text;
        UpdateBubble();
    }

    // SkillCoolTimeController에서 사용할 수 있도록 추가 메서드
    public void ForceHideInformationPanel()
    {
        if (InformationPanel != null)
        {
            InformationPanel.SetActive(false);
            Debug.Log("[DetectivePlaceinfo] InformationPanel 강제 비활성화됨");
        }
    }
}