using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillCoolTimeController : MonoBehaviour
{
    [SerializeField]
    private DetectivePlaceinfo _detectivePlaceinfo;
    public Button SkillButton;
    public Image CoolTimeImage;
    public GameObject CoolTimeUIObject;
    public SkillDataSO SkillData;

    private float _currentCooldown = 0f;
    private bool _isCooldownRunning = false;
    private bool _cooldownFinished = true;
    private bool _hasUsedSkill = false;
    private bool _initializedWithNoPlace = true; // 최초 시작 상태 추적용

    void Start()
    {
        // 처음에는 쿨타임은 끝난 상태지만 버튼은 비활성화 (눌릴 수 없음)
        _cooldownFinished = true;
        _hasUsedSkill = false;

        // 1. 게임 최초 실행 시 장소가 "없음"이면 쿨타임을 멈춤
        CoolTimeImage.fillAmount = 1f;
        CoolTimeUIObject.SetActive(true);
        SkillButton.interactable = false;

        Debug.Log("[초기화] 처음 시작 상태 → 버튼 비활성화, 쿨타임 끝난 상태");
    }

    public void OnSkillUsed()
{
    _hasUsedSkill = true;
    _cooldownFinished = false;
    _currentCooldown = SkillData.CooldownTime;
    CoolTimeImage.fillAmount = 1f;
    CoolTimeUIObject.SetActive(true);
    SkillButton.interactable = false;

    // UI 활성화
    SkillUIManager skillUIManager = FindObjectOfType<SkillUIManager>();
    if (skillUIManager != null)
    {
        skillUIManager.EnableSkill();
        //Debug.Log("[스킬 사용] LeftTopSkill UI 생성됨");
    }
}

    public void OnMovedToOtherPlace()
{
    //Debug.Log("[쿨타임 처리] 장소 이동됨. 프리팹 제거 및 쿨타임 조건 확인");

    // SkillUIManager의 leftTopUIParent의 0번째 자식 삭제
    SkillUIManager skillUIManager = FindObjectOfType<SkillUIManager>();
    if (skillUIManager != null)
    {
        skillUIManager.DisableSkill();

        if (skillUIManager.leftTopUIParent.childCount > 0)
        {
            Transform firstChild = skillUIManager.leftTopUIParent.GetChild(0);
            if (firstChild != null)
            {
                Destroy(firstChild.gameObject);
                //Debug.Log("[UI 정리] leftTopUIParent의 0번째 자식 삭제됨");
            }
            _detectivePlaceinfo.InformationPanel.SetActive(false);
        }
    }

    DetectivePlaceinfo detectivePlaceinfo = FindObjectOfType<DetectivePlaceinfo>();
    if (detectivePlaceinfo != null && detectivePlaceinfo.InformationPanel != null)
    {
        detectivePlaceinfo.InformationPanel.SetActive(false);
        //Debug.Log("[UI 정리] InformationPanel 비활성화됨");
    }

    string currentPlace = MovePlaceManager.Instance?.CurrentPlaceName?.PlaceNameSetting.ToString() ?? "없음";

    // 1. 게임 첫 시작: "없음" 상태 → 아무것도 하지 않음
    if (_initializedWithNoPlace && currentPlace == "없음")
    {
        //Debug.Log("[처음 상태] 장소 없음이므로 대기 중");
        return;
    }

    // 2. "없음" → 다른 장소로 이동한 첫 순간 → 쿨타임을 즉시 끝낸다
    if (_initializedWithNoPlace && currentPlace != "없음")
    {
        _initializedWithNoPlace = false;

        //Debug.Log("[처음 상태 → 장소 진입] 쿨타임 완료 처리 후 버튼 활성화");
        ForceCooldownFinishForFirstMove();
        return;
    }

    // 3. 스킬을 한 번도 사용하지 않은 상태에서 장소 이동 시
    if (!_hasUsedSkill)
    {
        if (_cooldownFinished)
        {
            //Debug.Log("[스킬 미사용 상태] 쿨타임 끝난 상태 유지");
            CoolTimeUIObject.SetActive(false);
            CoolTimeImage.fillAmount = 0f;
            SkillButton.interactable = true;
        }
        else
        {
            //Debug.Log("[스킬 미사용 상태] 쿨타임 작동");
            CoolTimeUIObject.SetActive(true);
            SkillButton.interactable = false;

            // ✅ 스킬을 사용하지 않았지만 쿨타임 중인 경우에도 쿨타임 진행 시작
            if (!_isCooldownRunning)
            {
                StartCoroutine(CooldownRoutine());
               // Debug.Log("[쿨타임 시작] 스킬 미사용 상태지만 쿨타임 남음 → 코루틴 시작");
            }
        }
        return;
    }

    // 4. 스킬을 사용한 상태에서 장소 이동 시
   // Debug.Log("[스킬 사용 후 이동] 쿨타임 진행");
    CoolTimeUIObject.SetActive(true);
    SkillButton.interactable = false;

    if (_cooldownFinished)
    {
        //Debug.Log("[쿨타임 유지] 쿨타임 이미 종료됨 → 재시작 안 함");
        HandleCooldownFinish();
        return;
    }


}


    public IEnumerator CooldownRoutine()
    {
        _isCooldownRunning = true;
        while (_currentCooldown > 0f)
        {
            _currentCooldown -= Time.deltaTime;
            CoolTimeImage.fillAmount = _currentCooldown / SkillData.CooldownTime;
            yield return null;
        }

        _cooldownFinished = true;
        _isCooldownRunning = false;
        HandleCooldownFinish();
    }

    private void HandleCooldownFinish()
    {
        SkillButton.interactable = true;

        if (SkillData.IsCombatOnly)
        {
            CoolTimeUIObject.SetActive(false);
            //Debug.Log("[쿨타임 종료] 전투 전용 → 쿨타임 UI 숨김");
        }
        else
        {
            CoolTimeUIObject.SetActive(true);
            //Debug.Log("[쿨타임 종료] 비전투 전용 → 쿨타임 UI 유지");
        }
    }

    // "없음" → 다른 장소로 이동한 첫 순간용 강제 쿨타임 종료
    private void ForceCooldownFinishForFirstMove()
    {
        _currentCooldown = 0f;
        _cooldownFinished = true;
        _isCooldownRunning = false;
        
        // 2. "없음" 상태에서 다른 장소로 이동하면 쿨타임을 즉시 끝낸다
        CoolTimeImage.fillAmount = 0f;
        CoolTimeUIObject.SetActive(false);
        SkillButton.interactable = true;
        
        //Debug.Log("[첫 이동] 쿨타임 즉시 종료 - fillAmount=0, UI숨김, 버튼활성화");
    }

    private void ForceCooldownFinish()
    {
        _currentCooldown = 0f;
        _cooldownFinished = true;
        _isCooldownRunning = false;
        HandleCooldownFinish();
    }

    public bool IsCooldownFinished() => _cooldownFinished;

    // 추가 메서드들
    public void SetCooldownUI(bool isCooldownActive, float fillAmount, bool interactable)
    {
        if (CoolTimeImage != null) CoolTimeImage.fillAmount = fillAmount;
        if (CoolTimeUIObject != null) CoolTimeUIObject.SetActive(isCooldownActive);
        if (SkillButton != null) SkillButton.interactable = interactable;
    }

    public bool IsCooldownActive()
    {
        return CoolTimeUIObject != null && CoolTimeUIObject.activeSelf;
    }

    public float CurrentFillAmount()
    {
        return CoolTimeImage != null ? CoolTimeImage.fillAmount : 1f;
    }
}