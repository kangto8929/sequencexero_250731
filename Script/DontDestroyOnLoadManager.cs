using UnityEngine;

public class DontDestroyOnLoadManager : MonoBehaviour
{
    void Awake()
    {
        // �� ������Ʈ�� �ı����� �ʵ��� �����մϴ�.
        DontDestroyOnLoad(this.gameObject);
    }
}