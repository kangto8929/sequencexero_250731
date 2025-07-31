using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using UnityEditor;

public class LoadingManager : MonoBehaviour
{
    public Slider LoadingSlider;
    public bool IsTutorialFinished = false;//튜토리얼 완료 여부
    private bool _sceneLoaded = false;//중복 방지


    private void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        int targetSceneIndex = IsTutorialFinished ? 2 : 1;
        Debug.Log($"[로딩 시작] 로드할 씬 인덱스: {targetSceneIndex}");

        AsyncOperation ao = SceneManager.LoadSceneAsync(targetSceneIndex);
        ao.allowSceneActivation = false;

        while (!ao.isDone)
        {
            float displayedProgress = Mathf.Clamp01(ao.progress / 0.9f);
            LoadingSlider.value = displayedProgress;
            Debug.Log($"[로딩 진행률] ao.progress = {ao.progress}, 보정된: {displayedProgress}");

            //  슬라이더가 100% 도달했으면 씬 활성화
            if (displayedProgress >= 1.0f && !_sceneLoaded)
            {
                _sceneLoaded = true;
                Debug.Log("[로딩 완료] 씬 활성화 준비됨");

                yield return new WaitForSeconds(1f);
                ao.allowSceneActivation = true;
                Debug.Log("[씬 전환 중]");
            }

            yield return null;
        }
    }

}
