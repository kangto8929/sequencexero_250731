using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum PlaceNameType
{
    None,
    EcologicalRepository,//���� �����
    MirrorMaze,//�ſ� �̷�
    ArtificialBeach,//�ΰ� �ٴ尡
    Archive,//��� ������
    CrossroadsOfMemory,//����� ������
    MemoryTransmissionTerminal,//��� ���� �͹̳�
    DreamFactory,//�� ����
    Theater,//����
    RefugeeSettlement,//������ �ְ���
    FateExperimentChamber,//��� ���赿
    FantasyLibrary,//ȯ�� ������
    IncenseLaboratory,//��� ������
    SpaceStation,//���� ������
    ArtificialHatchery,//�ΰ� ��ȭ��
    NeuralConnectionRoom,//�Ű� ���ӽ�
    SonicResearchLab,//���� ������
    AncientForest,//������ ��
    Aquarium,//������
    AmethystGarden,//�ڼ��� ����
    DataCoreLaboratory,//������ �� �����

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

        // ������ ����Ʈ�� �����ؼ� �ߺ� ���� ���
        List<GameObject> shuffledPrefabs = new List<GameObject>(PlacePrefab);
        ShuffleList(shuffledPrefabs);

        // �θ� ��ġ���� �������� ���� ���, �θ� ����ŭ�� ��ġ
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

    // Fisher-Yates ����
    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            (list[i], list[rnd]) = (list[rnd], list[i]);
        }
    }
}
