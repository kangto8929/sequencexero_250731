using UnityEngine;
using UnityEngine.UI;

public class ChangeButtonFirstImage : Button
{
    private Image _childGrandImage; // 0번째 자식의 0번째 자식 Image

    protected override void Start()
    {
        base.Start();
        // Start에서 미리 찾기 시도는 해둬도 되고 없어도 무방
        RefreshChildImage();
    }

    private void RefreshChildImage()
    {
        _childGrandImage = null;  // 초기화

        if (transform.childCount > 0)
        {
            Transform firstChild = transform.GetChild(0); // 0번째 자식
            if (firstChild.childCount > 0)
            {
                Transform grandChild = firstChild.GetChild(0); // 손자 (0번째 자식의 0번째 자식)
                _childGrandImage = grandChild.GetComponent<Image>();

                if (_childGrandImage != null)
                {
                    return;
                    //Debug.Log("손자 이미지 할당 완료");
                }
                else
                {
                    return;
                    //Debug.LogWarning("손자에 Image 컴포넌트가 없습니다.");
                }
            }
            else
            {
                return;
                //Debug.LogWarning("0번째 자식이 자식을 가지고 있지 않습니다.");
            }
        }
        else
        {
            return;
            //Debug.LogWarning("버튼에 자식이 없습니다.");
        }
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

        // 자식 이미지가 없거나 파괴됐으면 다시 찾아보기
        if (_childGrandImage == null || _childGrandImage.Equals(null))
        {
            RefreshChildImage();
        }

        if (_childGrandImage == null)
            return;

        Color targetColor;

        switch (state)
        {
            case SelectionState.Normal:
                targetColor = colors.normalColor;
                break;
            case SelectionState.Highlighted:
                targetColor = colors.highlightedColor;
                break;
            case SelectionState.Pressed:
                targetColor = colors.pressedColor;
                break;
            case SelectionState.Selected:
                targetColor = colors.selectedColor;
                break;
            case SelectionState.Disabled:
                targetColor = colors.disabledColor;
                break;
            default:
                targetColor = Color.white;
                break;
        }

        targetColor.a = 1f; // 알파 고정

        if (instant)
        {
            _childGrandImage.canvasRenderer.SetColor(targetColor);
            _childGrandImage.color = targetColor;
        }
        else
        {
            _childGrandImage.CrossFadeColor(targetColor, colors.fadeDuration, true, true);
        }
    }
}
