using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class GPGS_Manager : MonoBehaviour
{
    public void Initialize()
    {
#if UNITY_ANDROID
        SignInAndReportScore(Data_Manager.m_Data.Level);
#endif
    }

    public void SignInAndReportScore(int value)
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate(result =>
        {
            if (result == SignInStatus.Success)
            {
                Debug.Log("Google Play Games sign-in succeeded.");
                SaveScore(value);
            }
            else
            {
                Debug.LogError("Google Play Games sign-in failed: " + result);
            }
        });
#endif
    }

    public void ShowReaderBoardUI()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.ShowLeaderboardUI(GPGSIds.leaderboard);
#endif
    }

    public void SaveScore(double damage)
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.ReportScore(
            (long)damage,
            GPGSIds.leaderboard,
            success =>
            {
                if (success)
                {
                    Debug.Log("Leaderboard score submitted.");
                }
            });
#endif
    }
}
