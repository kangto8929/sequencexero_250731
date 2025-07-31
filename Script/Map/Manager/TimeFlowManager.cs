using System;
using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class TimeFlowManager : MonoBehaviour
{
     //테스트용으로 만든 코드로 삭제
     //public TextMeshProUGUI PlayerCount;


    //여기부터는 원래 있었떤 거거

    public static TimeFlowManager Instance;

    [SerializeField]
    private UI_TimeFlow _timeFlowUI;

    [SerializeField]
    private BlockedPlaceApplier _blockedPlaceApplier;

    [SerializeField]
    private DayCycleBlockedManager _dayCycleBlockedManager;

    public float Timer;
    private const float NightDuration = 20f; //135f; // 2분 15초
    private const float DayDuration = 20f;   //240f; // 4분

    public bool TimerRunning = false;

    public int CurrentStep;//현재 몇 단계인지

    private bool _gameOverTriggered = false;//게임 오버 중복 방지

    //AI움직임 관련 - 금지구역 게임 오버 관련
    [SerializeField]
    private List<EnemyMoveManager> _enemyMoveManagers;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TimerRunning = false;//원래 true였음음
        //원래 활성화였음음
        //StartStep();
        //TimerRunning = true;
    }

    private void Update()
    {
        if (!TimerRunning || _gameOverTriggered)
            return;

        Timer -= Time.deltaTime;

        if (Timer <= 0f)
        {
            Timer = 0f;
            TimerRunning = false;

            var ui = UI_InGameManager.Instance;

            bool isBlockedSpriteActive = ui.StatusLeft.activeSelf == true &&
                (ui == ui.BlockedSprite || ui.StatusIconRight.sprite == ui.BlockedSprite);

            bool isPenaltySpriteActive = ui.StatusLeft.activeSelf == true &&
                (ui.StatusIconLeft.sprite == ui.PenaltySprite || ui.StatusIconRight.sprite == ui.PenaltySprite);

            if (isBlockedSpriteActive || isPenaltySpriteActive)
            {
                 Debug.Log(" 게임 오버 - Blocked 또는 Penalty 스프라이트가 활성화된 상태에서 타이머 종료");
                _gameOverTriggered = true;
                // 추가 게임오버 처리
                return;
            }




           Debug.Log(" 상태 아이콘에 Blocked/Penalty 스프라이트 없음, 다음 단계로 이동");
            AdvanceStep();
            MovePlaceManager.Instance.CurrentPlaceName.UpdatePlaceStatus();
        }
    }

    public void AdvanceStep()
    {
        
        CurrentStep++;
        
        StartStep();

        TimerRunning = true;
        _timeFlowUI.UpdateAllUI();
        _dayCycleBlockedManager.OnTimeAdvance();

        MovePlaceManager.Instance.UpdateCurrentPlaceBackground();

        //낮/밤 아이콘 업데이트
        var currentPlace = MovePlaceManager.Instance.CurrentPlace;
        if (currentPlace != null)
        {
            var placeState = currentPlace.GetComponent<PlaceState>();

            if (placeState != null)
            {
               
                placeState.PlaceStatusChanged -= UI_InGameManager.Instance.UpdateStatusIcon;

                
                placeState.PlaceStatusChanged += UI_InGameManager.Instance.UpdateStatusIcon;

                
                placeState.UpdatePlaceStatus();
            }
        }

        MovePlaceManager.Instance.UpdateAllPlaceButtons();
        MovePlaceManager.Instance.RefreshMovablePlaces();

        foreach(var enemyMoveManager in _enemyMoveManagers)
        {
            enemyMoveManager.CheckAIStatusOnTimeChange();
        }



        
    }

    public void StartStep()
    {
         //밤이면 밤 시간 타이머
        if (CurrentStep % 2 == 0)
        {
            Timer = NightDuration;
        }
         //낮이면 낮 시간 타이머
        else
        {
            Timer = DayDuration;
        }

        Debug.Log("낮밤 바뀜 : "+ CurrentStep);
//AI움직임 게임 오버 관련련
        
    }

    public bool IsNightStep()
    {
        return CurrentStep % 2 == 0; ;//짝수면 밤 //홀수면 낮
    }

 //튜토리얼의 경우 퍼즈해야 함
    public void PauseTimer()
    {
        TimerRunning = false;
    }

    //다시 타이머 시작

    public void ResumeTimer()
    {
        TimerRunning = true;
    }





}



