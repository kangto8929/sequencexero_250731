using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using TMPro;

public class GPGSManager1 : MonoBehaviour
{
    [SerializeField] TMP_Text textBox;
    public static GPGSManager1 Instance { get; private set; }
    private bool isAuthenticated = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGPGS();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeGPGS();
    }

    private void InitializeGPGS()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
         PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(OnSignInResult);
    }

    private void OnSignInResult(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            textBox.text = "Google Play Games sign-in successful.";
            Debug.Log("Google Play Games sign-in successful.");
            isAuthenticated = true;
        }
        else
        {
            textBox.text = "Google Play Games sign-in failed: " + status;
            Debug.LogError("Google Play Games sign-in failed: " + status);
            isAuthenticated = false;
        }
    }

    public bool IsAuthenticated()
    {
        return isAuthenticated;
    }
}