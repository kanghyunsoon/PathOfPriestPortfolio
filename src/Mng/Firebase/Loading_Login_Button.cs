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
        GuestLogin.onClick.AddListener(() => Base_Manager.Firebase.GuestLogin());
        GoogleLogin.onClick.AddListener(() => Base_Manager.Firebase.OnGoogleLoginClicked());
    }

    public void OnCompletedInit()
    {
        transform.gameObject.SetActive(false);
    }
}
