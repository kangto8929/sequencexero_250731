
//구글 플레이 게임즈의 닉네임을 가져와서 뒤끝의 닉네임에 저장까지 구현 완료
using BackEnd;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class BackendManager : MonoBehaviour
{
    private void Awake()
    {
        var bro = Backend.Initialize();   // 뒤끝 초기화

        if (bro.IsSuccess())
        {
            Debug.Log("뒤끝 초기화 성공 : " + bro);

#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log("현재 단말 GoogleHash : " + Backend.Utils.GetGoogleHash());
            Debug.Log("뒤끝 초기화 성공했다 아이가");
#endif
            InitializeGPGS();
            //PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        }
        else
        {
            Debug.LogError("뒤끝 초기화 실패 : " + bro);
        }
    }

    private void InitializeGPGS()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate(OnSignInResult);
    }

    void OnSignInResult(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            GetAccessCode(); // 뒤끝 연동
            Debug.Log("구글 플레이 게임즈 로그인 성공적.");
        }
        else
        {
            Debug.LogError("구글 플레이 게임즈 로그인 실패: " + status);
        }
    }

    void GetAccessCode()
    {
        PlayGamesPlatform.Instance.RequestServerSideAccess(false, authCode =>
        {
            Debug.Log($"구글 인증 코드 : {authCode}");

            // 인증 코드 → AccessToken
            Backend.BMember.GetGPGS2AccessToken(authCode, tokenRes =>
            {
                if (!tokenRes.IsSuccess())
                {
                    Debug.LogError($"GetGPGS2AccessToken 실패 : {tokenRes}");
                    return;
                }

                string accessToken = tokenRes.GetReturnValuetoJSON()["access_token"].ToString();

                // AccessToken으로 뒤끝 로그인
                Backend.BMember.AuthorizeFederation(accessToken, FederationType.GPGS2, loginRes =>
                {
                    if (!loginRes.IsSuccess())
                    {
                        Debug.LogError($"뒤끝 연동 로그인 실패: {loginRes}");
                        return;
                    }

                    Debug.Log("뒤끝 연동 로그인 성공.");

                    /* ───────── 구글 쪽 이름 확보 ───────── */
                    string gpgsName = (Social.localUser != null) ? Social.localUser.userName : "";

                    /* ───────── 닉네임 후보 결정 ───────── */
                    // Google Play Games 닉네임이 유효하지 않으면 임시 닉네임 생성
                    bool hasValidGpgsName = !string.IsNullOrEmpty(gpgsName) && gpgsName != "Player";
                    string candidateName;

                    if (hasValidGpgsName)
                    {
                        candidateName = gpgsName; // 정상 닉네임
                    }
                    else
                    {
                        candidateName = $"Player_{UnityEngine.Random.Range(1000, 9999)}"; // 임시 자동 생성
                        // 추가된 Debug.Log 
                        Debug.LogWarning($"구글 플레이 게임즈 닉네임이 유효하지 않아 임시 닉네임 생성 ▶ {candidateName}");
                    }

                    // 현재 뒤끝 서버에 저장된 닉네임 가져오기
                    string currentBackendNickName = Backend.UserNickName;

                    // Google Play Games 닉네임과 현재 Backend 닉네임이 다르면 업데이트 시도
                    if (currentBackendNickName != candidateName)
                    {
                        TryUpdateNickname(candidateName);
                    }
                    else
                    {
                        Debug.Log($"현재 뒤끝 닉네임 '{currentBackendNickName}'이(가) 구글 닉네임과 동일하여 업데이트 건너뜀.");
                    }
                });
            });
        });
    }

    // 닉네임 업데이트를 시도하는 헬퍼 함수
    void TryUpdateNickname(string nicknameToSet)
    {
        Backend.BMember.UpdateNickname(nicknameToSet, nickRes =>
        {
            if (nickRes.IsSuccess())
            {
                Debug.Log($"닉네임 등록/업데이트 완료 ▶ {nicknameToSet}");
            }
            else if (nickRes.GetStatusCode() == "409") // 중복 오류 (409 Conflict)
            {
                // 닉네임 중복 발생 시, 새로운 임시 닉네임 생성 후 재시도
                string newRandomName = $"Player_{UnityEngine.Random.Range(1000, 9999)}";
                Debug.LogWarning($"닉네임 중복 ▶ 재시도: {newRandomName}");
                TryUpdateNickname(newRandomName); // 재귀 호출
            }
            else
            {
                Debug.LogError($"닉네임 등록/업데이트 실패 ▶ {nickRes}");
            }
        });
    }
}


//뒤끝서버랑 구글 연동 완료
/*using BackEnd;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine;

public class BackendManager : MonoBehaviour
{
    private void Awake()
    {
        var bro = Backend.Initialize();   // 뒤끝 초기화

        if (bro.IsSuccess())
        {
            Debug.Log("뒤끝 초기화 성공 : " + bro);

#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log("현재 단말 GoogleHash : " + Backend.Utils.GetGoogleHash());
            Debug.Log("뒤끝 초기화 성공 했다 아이가");
#endif

            PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
        }
        else
        {
            Debug.LogError("뒤끝 초기화 실패 : " + bro);
        }
    }

    void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            GetAccessCode(); // 뒤끝 연동
            Debug.Log("구글 플레이 게임즈 로그인 성공적.");
        }
        else
        {
            Debug.LogError("구글 플레이 게임즈 로그인 실패: " + status);
        }
    }

    void GetAccessCode()
    {
        PlayGamesPlatform.Instance.RequestServerSideAccess(false, authCode =>
        {
            Debug.Log($"구글 인증 코드 : {authCode}");

            // 인증 코드 → AccessToken
            Backend.BMember.GetGPGS2AccessToken(authCode, tokenRes =>
            {
                if (!tokenRes.IsSuccess())
                {
                    Debug.LogError($"GetGPGS2AccessToken 실패 : {tokenRes}");
                    return;
                }

                string accessToken = tokenRes.GetReturnValuetoJSON()["access_token"].ToString();

                // AccessToken으로 뒤끝 로그인
                Backend.BMember.AuthorizeFederation(accessToken, FederationType.GPGS2, loginRes =>
                {
                    if (!loginRes.IsSuccess())
                    {
                        Debug.LogError($"뒤끝 로그인 실패 : {loginRes}");
                        return;
                    }

                    /' 1) 로컬에 저장된 값 (null 방어) 
                    string myInDate = Backend.UserInDate ?? "";   // null 방어
                    string myNickname = Backend.UserNickName ?? "";

                    // 2) 서버 JSON 파싱 - 모든 null 가능성 방어 
                    LitJson.JsonData json = null;
                    try
                    {
                        json = loginRes.GetReturnValuetoJSON();
                    }
                    catch
                    {
                        Debug.LogWarning("GetReturnValuetoJSON 파싱 실패 -> Raw : " + loginRes.GetReturnValue());
                    }

                    string gamerId = "";
                    string displayName = "";

                    if (json != null && json.IsObject && json.Keys != null)
                    {
                        if (json.Keys.Contains("gamerId"))
                            gamerId = json["gamerId"].ToString();

                        if (json.Keys.Contains("nickname"))
                            displayName = json["nickname"].ToString();
                    }
                    else
                    {
                        Debug.Log($"ReturnValuetoJSON == null 또는 Object 아님  -> Raw : {loginRes.GetReturnValue()}");
                    }

                    Debug.Log($"뒤끝 로그인 OK -> inDate:{myInDate}, gamerId:{gamerId}, nick:{(displayName != "" ? displayName : myNickname)}");

                    // TODO: 이 시점에 서버 DB 동기화나 로비 씬 이동 등 후처리
                });
            });
        });
    }
}*/
