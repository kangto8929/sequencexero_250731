using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using UnityEditor;

public class LoadingManager : MonoBehaviour
{
    public Slider LoadingSlider;
    public bool IsTutorialFinished = false;//Ʃ�丮�� �Ϸ� ����
    private bool _sceneLoaded = false;//�ߺ� ����


    private void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        int targetSceneIndex = IsTutorialFinished ? 2 : 1;
        Debug.Log($"[�ε� ����] �ε��� �� �ε���: {targetSceneIndex}");

        AsyncOperation ao = SceneManager.LoadSceneAsync(targetSceneIndex);
        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            float displayedProgress = Mathf.Clamp01(ao.progress / 0.9f);
            LoadingSlider.value = displayedProgress;
            Debug.Log($"[�ε� �����] ao.progress = {ao.progress}, ������: {displayedProgress}");

            //  �����̴��� 100% ���������� �� Ȱ��ȭ
            if (displayedProgress >= 1.0f && !_sceneLoaded)
            {
                _sceneLoaded = true;
                Debug.Log("[�ε� �Ϸ�] �� Ȱ��ȭ �غ��");

                yield return new WaitForSeconds(1f);
                ao.allowSceneActivation = true;
                Debug.Log("[�� ��ȯ ��]");
            }

            yield return null;
        }
    }

}
