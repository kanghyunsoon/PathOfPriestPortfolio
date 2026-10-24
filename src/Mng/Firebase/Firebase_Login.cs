using System;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public partial class Firebase_Manager
{
    public void OnGoogleLoginClicked()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status != SignInStatus.Success)
            {
                Debug.LogError("Google Play Games sign-in failed: " + status);
                Loading_Login_Button.Instance?.GetLoginInitialize();
                return;
            }

            PlayGamesPlatform.Instance.RequestServerSideAccess(false, SignInToFirebaseWithPlayGamesCode);
        });
#else
        Debug.LogError("Google Play Games sign-in is only available on Android.");
        Loading_Login_Button.Instance?.GetLoginInitialize();
#endif
    }

    private void SignInToFirebaseWithPlayGamesCode(string serverAuthCode)
    {
        if (string.IsNullOrEmpty(serverAuthCode))
        {
            Debug.LogError("Google Play Games did not return a server auth code.");
            Loading_Login_Button.Instance?.GetLoginInitialize();
            return;
        }

        Credential credential = PlayGamesAuthProvider.GetCredential(serverAuthCode);
        auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Firebase Play Games sign-in failed: " + task.Exception);
                Loading_Login_Button.Instance?.GetLoginInitialize();
                return;
            }

            currentUser = task.Result;
            CompleteLogin(LoginType.Google);
        });
    }

    public void GuestLogin()
    {
        if (auth == null)
        {
            Debug.LogError("Firebase Auth is not initialized.");
            return;
        }

        if (auth.CurrentUser != null)
        {
            currentUser = auth.CurrentUser;
            CompleteLogin(LoginType.Guest);
            return;
        }

        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Anonymous Firebase sign-in failed: " + task.Exception);
                Loading_Login_Button.Instance?.GetLoginInitialize();
                return;
            }

            currentUser = task.Result.User;
            CompleteLogin(LoginType.Guest);
        });
    }

    private void CompleteLogin(LoginType loginType)
    {
        if (!PlayerPrefs.HasKey("BGM"))
        {
            PlayerPrefs.SetFloat("BGM", 1.0f);
        }

        if (!PlayerPrefs.HasKey("VFX"))
        {
            PlayerPrefs.SetFloat("VFX", 1.0f);
        }

        PlayerPrefs.SetInt("Login", (int)loginType);
        PlayerPrefs.Save();
        Loading_Login_Button.Instance?.OnCompletedInit();
        ReadData();
    }

    public void DeleteCurrentAccount(Action<bool, string> completion)
    {
        if (auth == null || auth.CurrentUser == null)
        {
            completion?.Invoke(false, "No signed-in account was found.");
            return;
        }

        currentUser = auth.CurrentUser;
        LoginType loginType = (LoginType)PlayerPrefs.GetInt("Login");

        if (loginType == LoginType.Google)
        {
            ReauthenticateGoogleAndDelete(completion);
            return;
        }

        DeleteRemoteDataAndUser(completion);
    }

    private void ReauthenticateGoogleAndDelete(Action<bool, string> completion)
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (status != SignInStatus.Success)
            {
                completion?.Invoke(false, "Google Play Games reauthentication failed: " + status);
                return;
            }

            PlayGamesPlatform.Instance.RequestServerSideAccess(true, serverAuthCode =>
            {
                if (string.IsNullOrEmpty(serverAuthCode))
                {
                    completion?.Invoke(false, "A server authentication code was not returned.");
                    return;
                }

                Credential credential = PlayGamesAuthProvider.GetCredential(serverAuthCode);
                currentUser.ReauthenticateAsync(credential).ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        completion?.Invoke(false, "Account reauthentication failed: " + task.Exception);
                        return;
                    }

                    DeleteRemoteDataAndUser(completion);
                });
            });
        });
#else
        completion?.Invoke(false, "Google account deletion is only available on Android.");
#endif
    }

    private void DeleteRemoteDataAndUser(Action<bool, string> completion)
    {
        FirebaseUser userToDelete = currentUser;
        string userId = userToDelete.UserId;

        // 삭제하는 동안 자동 저장이 데이터를 다시 만들지 않게 막는다. 실패하면 저장을 다시 허용한다.
        isDataLoaded = false;

        reference.Child("USER").Child(userId).RemoveValueAsync().ContinueWithOnMainThread(databaseTask =>
        {
            if (databaseTask.IsCanceled || databaseTask.IsFaulted)
            {
                isDataLoaded = true;
                completion?.Invoke(false, "Saved game data deletion failed: " + databaseTask.Exception);
                return;
            }

            userToDelete.DeleteAsync().ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCanceled || authTask.IsFaulted)
                {
                    isDataLoaded = true;
                    completion?.Invoke(false, "Firebase account deletion failed: " + authTask.Exception);
                    return;
                }

                auth.SignOut();
                currentUser = null;
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Base_Manager.GetGameStart = false;
                completion?.Invoke(true, "Account and saved game data were deleted.");
            });
        });
    }
}
