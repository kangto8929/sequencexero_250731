using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum PlaceNameType
{
    None,
    EcologicalRepository,//생태 저장소
    MirrorMaze,//거울 미로
    ArtificialBeach,//인공 바닷가
    Archive,//기록 보관소
    CrossroadsOfMemory,//기억의 교차로
    MemoryTransmissionTerminal,//기억 전송 터미널
    DreamFactory,//꿈 공장
    Theater,//극장
    RefugeeSettlement,//낙오자 주거지
    FateExperimentChamber,//운명 실험동
    FantasyLibrary,//환상 도서관
    IncenseLaboratory,//향기 연구소
    SpaceStation,//우주 정거장
    ArtificialHatchery,//인공 부화실
    NeuralConnectionRoom,//신경 접속실
    SonicResearchLab,//음파 연구소
    AncientForest,//오래된 숲
    Aquarium,//수족관
    AmethystGarden,//자수정 정원
    DataCoreLaboratory,//데이터 핵 실험실

}
public class PlaceSpawner : MonoBehaviour
{
    public RectTransform[] ParentsPosition;
    public GameObject[] PlacePrefab;

    private void Start()
    {
        SpawnPlaces();
    }

    void SpawnPlaces()
    {
        if (PlacePrefab.Length == 0 || ParentsPosition.Length == 0)
            return;

        // 프리팹 리스트를 셔플해서 중복 없이 사용
        List<GameObject> shuffledPrefabs = new List<GameObject>(PlacePrefab);
        ShuffleList(shuffledPrefabs);

        // 부모 위치보다 프리팹이 많을 경우, 부모 수만큼만 배치
        int count = Mathf.Min(shuffledPrefabs.Count, ParentsPosition.Length);

        for (int i = 0; i < count; i++)
        {
            GameObject prefabToSpawn = shuffledPrefabs[i];
            RectTransform parent = ParentsPosition[i];

            GameObject spawned = Instantiate(prefabToSpawn, parent);
            RectTransform prefabRectTransform = spawned.GetComponent<RectTransform>();
            prefabRectTransform.anchoredPosition = Vector2.zero;
            prefabRectTransform.localScale = Vector3.one;
            prefabRectTransform.localRotation = Quaternion.identity;
        }
    }

    // Fisher-Yates 셔플
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
}
