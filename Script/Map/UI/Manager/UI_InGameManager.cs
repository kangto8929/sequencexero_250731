using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_InGameManager : MonoBehaviour
{

[Header("생존자 수")]
public TextMeshProUGUI CurrentPlayerCountText;
public int PlayerCount = 10;

    public static UI_InGameManager Instance;

    public GameObject StatusLeft, StatusRight;

    public Image StatusIconLeft;
    public Image StatusIconRight;

    public Sprite BlockedSprite;
    public Sprite PenaltySprite;



    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CurrentPlayerCountText.text = PlayerCount.ToString();
    }

    public void UpdatePlayerCount()
    {
        PlayerCount--;
        Debug.LogWarning($"생존자 수{ PlayerCount}");
        CurrentPlayerCountText.text = PlayerCount.ToString();
    }

    public void UpdateStatusIcon(PlaceStatus status)
    {
        switch(status)
        {
            case PlaceStatus.Danger:
                SetStatusIconActive(true, BlockedSprite);
                StatusLeft.SetActive(true);
                StatusRight.SetActive(true);
                Debug.Log("���� ���� ����");
                break;
            case PlaceStatus.Penalty:
                SetStatusIconActive(true, PenaltySprite);
                StatusLeft.SetActive(true);
                StatusRight.SetActive(true);
                Debug.Log("�г�Ƽ ����");
                break;
            default:
                SetStatusIconActive(false, null);
                StatusLeft.SetActive(false);
                StatusRight.SetActive(false);
                break;
        }
    }

    private void SetStatusIconActive(bool isActive, Sprite sprite)
    {
        StatusIconLeft.gameObject.SetActive(isActive);
        StatusIconRight.gameObject.SetActive(isActive);

        if(isActive && sprite != null)
        {
            StatusIconLeft.sprite = sprite;
            StatusIconRight.sprite = sprite;
        }
    }
}
