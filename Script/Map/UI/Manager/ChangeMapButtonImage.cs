using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class ChangeMapButtonImage : Button
{
    private List<Image> _targetChildImages = new List<Image>();

    protected override void Start()
    {
        base.Start();

        // 모든 자식 이미지 중 자기 자신 제외
        var allImages = GetComponentsInChildren<Image>(includeInactive: true)
                        .Where(img => img.gameObject != this.gameObject)
                        .ToList();

        // 인덱스 0, 2, 3 → 실제로는 자식 오브젝트의 1번째, 3번째, 4번째 Image
        if (allImages.Count > 0 && allImages[0] != null)
            _targetChildImages.Add(allImages[0]);

        if (allImages.Count > 2 && allImages[2] != null)
            _targetChildImages.Add(allImages[2]);

        if (allImages.Count > 3 && allImages[3] != null)
            _targetChildImages.Add(allImages[3]);

        // 디버깅용: 어떤 이미지가 들어갔는지 확인
        for (int i = 0; i < _targetChildImages.Count; i++)
        {
            if (_targetChildImages[i] == null)
                Debug.LogWarning($"_targetChildImages[{i}] is null!");
            else
                Debug.Log($"_targetChildImages[{i}] = {_targetChildImages[i].gameObject.name}");
        }
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

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

        foreach (var img in _targetChildImages)
        {
            if (img == null) continue;

            if (instant)
            {
                img.canvasRenderer.SetColor(targetColor);
            }
            else
            {
                img.CrossFadeColor(targetColor, colors.fadeDuration, true, true);
            }
        }
    }
}
