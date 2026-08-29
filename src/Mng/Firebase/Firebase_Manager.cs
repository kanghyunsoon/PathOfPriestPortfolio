using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public partial class Firebase_Manager
{
    private FirebaseAuth auth;
    public FirebaseUser currentUser;
    private DatabaseReference reference;

    public enum LoginType
    {
        None = 0,
        Guest = 1,
        Google = 2
    }

    public void Init()
    {
        LoginType savedLoginType = (LoginType)PlayerPrefs.GetInt("Login");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted || task.Result != DependencyStatus.Available)
            {
                Debug.LogError("Firebase initialization failed: " + task.Exception);
                Loading_Login_Button.Instance?.GetLoginInitialize();
                return;
            }

            auth = FirebaseAuth.DefaultInstance;
            currentUser = auth.CurrentUser;
            reference = FirebaseDatabase.DefaultInstance.RootReference;

            switch (savedLoginType)
            {
                case LoginType.Guest:
                    Loading_Login_Button.Instance?.OnCompletedInit();
                    GuestLogin();
                    break;

                case LoginType.Google:
                    Loading_Login_Button.Instance?.OnCompletedInit();
                    OnGoogleLoginClicked();
                    break;

                default:
                    Loading_Login_Button.Instance?.GetLoginInitialize();
                    break;
            }

            Debug.Log("Firebase initialized.");
        });
    }
}
