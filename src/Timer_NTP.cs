using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;

public class Timer_NTP : MonoBehaviour
{
    private static long diffTicks = 0L;
    private static DateTime syncTime = DateTime.Now;

    public static DateTime NowTime
    {
        get
        {
            DateTime baseTime = DateTime.Now.AddTicks(diffTicks);
            return ApplyRegionOffset(baseTime);
        }
    }

    // 지역별 시간 보정
    private static DateTime ApplyRegionOffset(DateTime time)
    {
        switch (Application.systemLanguage)
        {
            case SystemLanguage.English: // 미국 사용자라고 판단
                return time.AddHours(-13); // 한국 기준 -13시간
            case SystemLanguage.Korean:  // 한국
            case SystemLanguage.Japanese: // 일본
                return time; // 동일 GMT+9
            default:
                return time; // 기타 국가는 보정 없음
        }
    }

    public static void UpdateDateTime(string dataStr)
    {
        if (DateTime.TryParse(dataStr, out DateTime serverTime))
        {
            syncTime = serverTime;
            diffTicks = (syncTime - DateTime.Now).Ticks;
        }
    }


}

public class Time_Manager
{
    public float currentTime;
    public TimeSpan loginSpan;
    public Action<DateTime> OnTimeUpdated;
    private static bool isCheckIn = false;

    private const string serverUrl = "https://eyelisten.tistory.com/1";

    public void Init()
    {
        Base_Manager.instance.StartCoroutine(CheckServerTime());
    }

    public IEnumerator CheckServerTime()
    {
        UnityWebRequest request = UnityWebRequest.Get(serverUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Base_Canvas.instance.Get_UI("#NetError");
            Base_Manager.instance.Invoke("QuitGame", 3f);
        }
        else
        {
            string serverDate = request.GetResponseHeader("Date");
            if (DateTime.TryParse(serverDate, out DateTime serverTime))
            {
                Debug.Log("Server Time : " + serverTime);
                Timer_NTP.UpdateDateTime(serverDate);
            }
            else
            {
                Debug.LogError("Invalid server date Format.");
            }
        }
    }


}
