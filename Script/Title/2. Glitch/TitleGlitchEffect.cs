using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TitleGlitchEffect : MonoBehaviour
{
    public RectTransform uiImage;
    public Material glitchMaterial;

    [Header("�۸�ġ ����")]
    public float glitchInterval = 3f;
    public float glitchDuration = 0.5f;
    public float glitchAmount = 0.1f;

    [Header("ȭ�� ��鸲 ����")]
    public float shakeAmount = 5f;
    public float shakeDuration = 0.05f;

    private Vector2 originalPos;
    private float glitchTimer = 0f;

    void Start()
    {
        if (uiImage == null)
            uiImage = GetComponent<RectTransform>();

        originalPos = uiImage.anchoredPosition;

        // �ʱ� ������
        glitchMaterial.SetFloat("_GlitchAmount", glitchAmount);
        glitchMaterial.SetFloat("_GlitchWidth", 0.1f);   // ���� ���� ���
        glitchMaterial.SetFloat("_GlitchHeight", 0.02f); // ���� ���� ���
        glitchMaterial.SetFloat("_GlitchActive", 0);
    }

    void Update()
    {
        glitchTimer += Time.deltaTime;

        if (glitchTimer >= glitchInterval)
        {
            glitchTimer = 0f;
            TriggerGlitchAndShake();
        }
    }

    void TriggerGlitchAndShake()
    {
        // 1. �۸�ġ Ȱ��ȭ
        glitchMaterial.SetFloat("_GlitchActive", 1);

        // 2. UI ��鸲
        Vector2 shake = originalPos + new Vector2(
            Random.Range(-shakeAmount, shakeAmount),
            Random.Range(-shakeAmount, shakeAmount)
        );

        uiImage.DOAnchorPos(shake, shakeDuration)
            .SetEase(Ease.Flash)
            .OnComplete(() => uiImage.DOAnchorPos(originalPos, shakeDuration));

        // 3. ���� �ð� �� �۸�ġ ����
        Invoke(nameof(StopGlitch), glitchDuration);
    }

    void StopGlitch()
    {
        glitchMaterial.SetFloat("_GlitchActive", 0);
    }
}
