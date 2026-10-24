using UnityEngine;
using UnityEngine.UI;

public class Loading_Login_Button : MonoBehaviour
{
    public static Loading_Login_Button Instance;
    public Button GuestLogin, GoogleLogin;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void GetLoginInitialize()
    {
        transform.gameObject.SetActive(true);
        // 로그인에 실패해 다시 호출되면 리스너가 쌓여 한 번 눌러도 로그인을 여러 번 시도했다.
        GuestLogin.onClick.RemoveAllListeners();
        GoogleLogin.onClick.RemoveAllListeners();
        GuestLogin.onClick.AddListener(() => Base_Manager.Firebase.GuestLogin());
        GoogleLogin.onClick.AddListener(() => Base_Manager.Firebase.OnGoogleLoginClicked());
    }

    public void OnCompletedInit()
    {
        transform.gameObject.SetActive(false);
    }
}
