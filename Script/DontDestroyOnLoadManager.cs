using UnityEngine;

public class DontDestroyOnLoadManager : MonoBehaviour
{
    void Awake()
    {
        // 이 오브젝트가 파괴되지 않도록 설정합니다.
        DontDestroyOnLoad(this.gameObject);
    }
}