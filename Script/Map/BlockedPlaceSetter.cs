using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockedPlacesByDay
{
      [Header("금지구역 리스트")]
    public List<PlaceConnector> Places = new List<PlaceConnector>();
}

[System.Serializable]
public class FinalBlockConfig
{
     [Header("마지막 금지구역 5곳")]
    public List<PlaceConnector> FinalBlockedPlaces = new List<PlaceConnector>(); // �ݵ�� 5�� ����

   [Header("마지막까지 금지구역이 되지 않는 1곳")]
    public PlaceConnector FinalSafePlace;
}

public class BlockedPlaceSetter : MonoBehaviour
{
    [SerializeField]
    private DayCycleBlockedManager _dayCycleBlockedManager;

    public static BlockedPlaceSetter Instance;

    // 클래스 최상단에 추가
public FinalBlockConfig SelectedFinalConfig { get; private set; }


    void Start()
    {
        Instance = this;
        StartCoroutine(DelayedStart());
        SetupBlockedPlaces();
    }

    [Header("모든 장소소")]
    public List<PlaceConnector> AllPlaces = new List<PlaceConnector>();

    [Header("1-4일차 금지구역 장소소")]
    public List<BlockedPlacesByDay> BlockPlacesByDays = new List<BlockedPlacesByDay>();

     // 마지막 금지구역 5곳 + 안전구역 1곳 조합 리스트 (여러 개)
    [Header("마지막 금지구역 및 장소소")]
    public List<FinalBlockConfig> FinalConfigs = new List<FinalBlockConfig>();

     // 금지구역 개수 (4,5,5,5 등)
    private readonly int[] _blockedCountByDay = new int[] { 4, 5, 5, 5 };

   // 마지막 조합을 기준으로 세팅하는 함수
    public void SetupBlockedPlaces()
    {
        BlockPlacesByDays.Clear();

        if (FinalConfigs == null || FinalConfigs.Count == 0)
        {
            Debug.LogError("finalConfigs 조합이 없습니다.");
            return;
        }

       // 1. 마지막 금지구역 조합 무작위 선택
        SelectedFinalConfig = FinalConfigs[Random.Range(0, FinalConfigs.Count)];
        List<PlaceConnector> lastDayBlocked = new List<PlaceConnector>(SelectedFinalConfig.FinalBlockedPlaces);
        PlaceConnector finalSafePlace = SelectedFinalConfig.FinalSafePlace;

        // 2. 전체 장소에서 마지막 금지구역 5곳 + 안전지대 1곳 제외 → 나머지 14곳
        List<PlaceConnector> remainingPlaces = new List<PlaceConnector>(AllPlaces);
        foreach (var blocked in lastDayBlocked)
        {
            remainingPlaces.Remove(blocked);
        }
        remainingPlaces.Remove(finalSafePlace);// 안전지대는 절대 금지되지 않음

        // 3. 1~3일차 금지구역 무작위 배치
        int[] dayBlockedCount = _blockedCountByDay;  // 예: {4,5,5,5}
        int dayCount = dayBlockedCount.Length;
        int daysExceptLast = dayCount - 1;

        if (remainingPlaces.Count < dayBlockedCount[0] + dayBlockedCount[1] + dayBlockedCount[2])
        {
            Debug.LogError("금지구역으로 배치할 장소가 부족합니다.");
            return;
        }

        // 리스트 초기화
        List<BlockedPlacesByDay> daysData = new List<BlockedPlacesByDay>();
        for (int i = 0; i < daysExceptLast; i++)
            daysData.Add(new BlockedPlacesByDay());
        BlockedPlacesByDay lastDayData = new BlockedPlacesByDay();

        Shuffle(remainingPlaces);// 무작위로 섞기

        int index = 0;
        for (int day = 0; day < daysExceptLast; day++)
        {
            int count = dayBlockedCount[day];
            for (int i = 0; i < count; i++)
            {
                daysData[day].Places.Add(remainingPlaces[index++]);
            }
        }

       // 마지막 날 금지구역 설정
        lastDayData.Places.AddRange(lastDayBlocked);

        // 결과 저장
        for (int i = 0; i < daysExceptLast; i++)
            BlockPlacesByDays.Add(daysData[i]);
        BlockPlacesByDays.Add(lastDayData);

       Debug.Log(" 금지구역 배치 완료: 마지막 조합 - " + string.Join(", ", lastDayBlocked.ConvertAll(p => p.name)));
    Debug.Log(" 안전 장소 - " + finalSafePlace.name);

        StartCoroutine(DelayedStart());
    }


     // 리스트 셔플 함수
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[r];
            list[r] = temp;
        }
    }


    private IEnumerator DelayedStart()
    {
        yield return null;
        _dayCycleBlockedManager.OnTimeAdvance();
    }
}

