using TMPro;
using UnityEngine;

public class UI_TimeFlow : MonoBehaviour
{



    [SerializeField]
    private TextMeshProUGUI _dayText;
    
    [SerializeField]
    private TextMeshProUGUI _timerText;
    
    [SerializeField]
    private GameObject _dayIcon;
    
    [SerializeField]
    private GameObject _nightIcon;

    private void Start()
    {
        UpdateAllUI();
    }

    private void Update()
    {
        UpdateTimerText(TimeFlowManager.Instance.Timer);
    }

    private void UpdateTimerText(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        _timerText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    public void UpdateAllUI()
    {
        UpdateDayText();
        UpdateIcons();
    }

    private void UpdateDayText()
    {
        int step = TimeFlowManager.Instance.CurrentStep;
        int day = (step/2) + 1;
        _dayText.text = day.ToString();
    }

    private void UpdateIcons()
    {
        bool isNight = TimeFlowManager.Instance.IsNightStep();
        _nightIcon.SetActive(isNight);
        _dayIcon.SetActive(!isNight);
    }
}
