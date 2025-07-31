using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    [Header("탐정 전용 스킬 데이터")]
    public SkillDataSO detectiveSkill;

    [Header("UI 연결")]
    public Transform leftTopUIParent;  // public으로 유지 (SkillCoolTimeController에서 접근)
    public DetectivePlaceinfo detectivePlaceinfo;

    private GameObject skillUIInstance;

    public void EnableSkill()
    {
        if (skillUIInstance == null && detectiveSkill.LeftTopSkill != null)
        {
            skillUIInstance = Instantiate(detectiveSkill.LeftTopSkill, leftTopUIParent);

            // 버튼 설정
            Button skillButton = skillUIInstance.GetComponentInChildren<Button>();
            if (skillButton != null)
            {
                skillButton.onClick.RemoveAllListeners();
                skillButton.onClick.AddListener(() =>
                {
                    detectivePlaceinfo.OnClickTouchCount();
                });
            }
        }

        if (skillUIInstance != null)
        {
            skillUIInstance.SetActive(true);
        }
    }

    public void DisableSkill()
    {
        if (skillUIInstance != null)
        {
            Destroy(skillUIInstance);
            skillUIInstance = null;
        }
    }

    // SkillCoolTimeController에서 사용할 수 있도록 추가 메서드
    public void ClearFirstChildOfLeftTopParent()
    {
        if (leftTopUIParent != null && leftTopUIParent.childCount > 0)
        {
            Transform firstChild = leftTopUIParent.GetChild(0);
            if (firstChild != null)
            {
                Destroy(firstChild.gameObject);
                Debug.Log("[SkillUIManager] leftTopUIParent의 0번째 자식 삭제됨");
            }
        }
    }

}