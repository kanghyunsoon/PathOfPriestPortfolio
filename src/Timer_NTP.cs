using UnityEngine;
using System;
using System.Collections;
using System.Globalization;
using UnityEngine.Networking;

public class Timer_NTP : MonoBehaviour
{
    // 서버 UTC - 기기 UTC. 기기 시계를 바꿔도 게임 시간은 서버 기준으로 흐른다.
    private static TimeSpan serverOffset = TimeSpan.Zero;

    public static bool IsSynced { get; private set; }

    public static DateTime UtcNow => DateTime.UtcNow + serverOffset;

    // 밤 시간·일일 초기화는 사용자가 있는 지역 기준이라 기기 시간대로 바꿔서 쓴다.
    // (예전에는 시스템 언어로 지역을 추측해서 시간을 뺐는데, 영어를 쓰는 다른 나라 사용자의 날짜가 어긋났다.)
    public static DateTime NowTime => UtcNow.ToLocalTime();

    public static bool UpdateDateTime(string httpDate)
    {
        // HTTP Date 헤더는 RFC1123 형식의 GMT 시간이다.
        if (!DateTime.TryParseExact(httpDate, "r", CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime serverUtc))
        {
            return false;
        }

        serverOffset = serverUtc - DateTime.UtcNow;
        IsSynced = true;
        return true;
    }

    // 저장하는 날짜는 기기 언어 설정과 상관없이 UTC ISO-8601로 통일한다.
    public static string ToSaveString(DateTime time)
    {
        return time.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
    }

    public static bool TryParseSaved(string value, out DateTime utc)
    {
        utc = default;
        if (string.IsNullOrEmpty(value)) return false;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime parsed)
            && parsed.Kind == DateTimeKind.Utc)
        {
            utc = parsed;
            return true;
        }

        // 이전 버전 저장값은 DateTime.ToString() 결과라 기기 지역 형식이다. ("2025. 5. 1. 오후 3:20:11" 등)
        // 저장한 뒤 기기 언어가 바뀌었을 수도 있어서 한국어 형식도 같이 시도한다.
        CultureInfo[] cultures = { CultureInfo.CurrentCulture, KoreanCulture(), CultureInfo.InvariantCulture };
        foreach (CultureInfo culture in cultures)
        {
            if (DateTime.TryParse(value, culture, DateTimeStyles.AssumeLocal, out parsed))
            {
                utc = parsed.ToUniversalTime();
                return true;
            }
        }

        return false;
    }

    private static CultureInfo koreanCulture;

    private static CultureInfo KoreanCulture()
    {
        if (koreanCulture != null) return koreanCulture;

        try
        {
            koreanCulture = CultureInfo.GetCultureInfo("ko-KR");
        }
        catch (CultureNotFoundException)
        {
            koreanCulture = CultureInfo.InvariantCulture;
        }

        return koreanCulture;
    }
}

public class Time_Manager
{
    public float currentTime;
    public TimeSpan loginSpan;
    public Action<DateTime> OnTimeUpdated;

    // Date 헤더만 필요해서 본문 없이 HEAD로 요청한다.
    private const string serverUrl = "https://www.google.com";
    private const int maxAttempts = 3;

    public void Init()
    {
        Base_Manager.instance.StartCoroutine(CheckServerTime());
    }

    public IEnumerator CheckServerTime()
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using (UnityWebRequest request = UnityWebRequest.Head(serverUrl))
            {
                request.timeout = 5;
                yield return request.SendWebRequest();

                // 4xx/5xx 응답에도 Date 헤더는 붙어 오기 때문에 헤더가 있으면 그대로 쓴다.
                string serverDate = request.GetResponseHeader("Date");
                if (!string.IsNullOrEmpty(serverDate) && Timer_NTP.UpdateDateTime(serverDate))
                {
                    Debug.Log("Server Time (UTC) : " + Timer_NTP.UtcNow);
                    OnTimeUpdated?.Invoke(Timer_NTP.NowTime);
                    yield break;
                }

                Debug.LogWarning($"서버 시간 확인 실패 ({attempt}/{maxAttempts}) : {request.error}");
            }

            if (attempt < maxAttempts)
            {
                yield return new WaitForSecondsRealtime(2.0f);
            }
        }

        // 서버 시간 없이 진행하면 기기 시계로 오프라인 보상·일일 초기화를 조작할 수 있다.
        Base_Manager.instance.ShowNetErrorAndQuit();
    }
}
