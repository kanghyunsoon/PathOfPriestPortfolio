using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;

public class ADS_Manager
{
    public bool TestMode = false;
    // 운영 광고 단위 ID는 공개 저장소에서 제외했습니다.
    public readonly string banner_Android_id       = "YOUR_ANDROID_BANNER_AD_UNIT_ID";
    public readonly string interstitial_Android_id = "YOUR_ANDROID_INTERSTITIAL_AD_UNIT_ID";
    public readonly string reward_Android_id       = "YOUR_ANDROID_REWARDED_AD_UNIT_ID";

    public readonly string banner_ios_id           = "YOUR_IOS_BANNER_AD_UNIT_ID";
    public readonly string interstitial_ios_id     = "YOUR_IOS_INTERSTITIAL_AD_UNIT_ID";
    public readonly string reward_ios_id           = "YOUR_IOS_REWARDED_AD_UNIT_ID";

    // Test
    public readonly string banner_Android_Sample_id = "ca-app-pub-3940256099942544/6300978111";
    public readonly string interstitial_Android_Sample_id = "ca-app-pub-3940256099942544/1033173712";
    public readonly string reward_Android_Sample_id = "ca-app-pub-3940256099942544/5224354917";

    public readonly string banner_ios_Sample_id = "ca-app-pub-3940256099942544/2934735716";
    public readonly string interstitial_ios_Sample_id = "ca-app-pub-3940256099942544/4411468910";
    public readonly string reward_ios_Sample_id = "ca-app-pub-3940256099942544/1712485313";

    BannerView     _banner;             // 배너
    InterstitialAd _intertstitialAd;    // 전면광고
    RewardedAd     _rewardedAd;         // 보상형 광고
    AdRequest      _adRequest;

    Action _rewardCallback;
    bool _initialized;

    public void Init()
    {
        // Base_Manager와 로그인 후 ReadData 양쪽에서 호출된다. 이벤트가 중복 등록되지 않게 한 번만 초기화한다.
        if (_initialized) return;
        _initialized = true;

        MobileAds.Initialize(initStatus => { });
        PrepareADS();

    }

    private void PrepareADS()
    {
        string banner_Android;
        string interstitial_Android;
        string reward_Android;

        string banner_Ios;
        string interstitial_Ios;
        string reward_Ios;

        if (TestMode)
        {
            banner_Android       = banner_Android_Sample_id;
            interstitial_Android = interstitial_Android_Sample_id;
            reward_Android       = reward_Android_Sample_id;

            banner_Ios           = banner_ios_Sample_id;
            interstitial_Ios     = interstitial_ios_Sample_id;
            reward_Ios           = reward_ios_Sample_id;
        }
        else
        {
            banner_Android       = banner_Android_id;
            interstitial_Android = interstitial_Android_id;
            reward_Android       = reward_Android_id;

            banner_Ios           = banner_ios_id;
            interstitial_Ios     = interstitial_ios_id;
            reward_Ios           = reward_ios_id;
        }

        _adRequest = new AdRequest();
        _adRequest.Keywords.Add("unity-admob-sample");

        // BannerView(banner_Android);
        // InterstitialAd.Load(interstitial_Android, _adRequest, OnAdLoadCallback);
        RewardedAd.Load(reward_Android, _adRequest, OnAdRewardCallback);

    }

    // 리워드 광고
    private void OnAdRewardCallback(RewardedAd ad, LoadAdError error)
    {
        if (error != null || ad == null)
        {
            Debug.LogError("보상형 광고 준비 실패! : " + error);
            return;
        }

        Debug.Log("보상형 광고 로드! : " + ad.GetResponseInfo());

        // 이전 광고 객체는 다시 쓰지 않으므로 해제한다.
        if (_rewardedAd != null && _rewardedAd != ad)
        {
            _rewardedAd.Destroy();
        }
        _rewardedAd = ad;
        RegisterEventHandler(_rewardedAd);
    }

    private void RegisterEventHandler(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("보상형 광고가 닫혔습니다.");
            PrepareADS();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("보상형 광고가 실패하였습니다. : " + error);
            PrepareADS();
        };
        // OnAdPaid는 광고 노출(수익) 이벤트라 끝까지 보지 않아도 호출된다.
        // 보상은 Show()의 보상 콜백에서만 지급한다.
    }

    public void ShowRewardedAds(Action rewardCallBack)
    {
        // 광고 제거를 구매했으면 광고 없이 바로 보상
        if (Data_Manager.m_Data.ADS_Remove)
        {
            Data_Manager.m_Data.ADS++;
            rewardCallBack?.Invoke();
            return;
        }

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardCallback = rewardCallBack;
            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log(reward.Type + " : " + reward.Amount);

                Base_Canvas.instance.StartCoroutine(DelayRewardCallback());

            });
        }
        else
        {
            Debug.Log("광고 준비중입니다.");
            PrepareADS();
        }
    }

    private IEnumerator DelayRewardCallback()
    {
        yield return null; // 한 프레임 기다림
        // 일일 퀘스트의 광고 시청 횟수는 보상을 실제로 받았을 때만 센다.
        // (예전에는 광고가 준비되지 않아도 버튼만 누르면 올라갔다)
        Data_Manager.m_Data.ADS++;
        _rewardCallback?.Invoke();
        _rewardCallback = null;
    }

    // 배너광고
    public void BannerView(string banner_id)
    {
        if(_banner != null)
        {
            _banner.Destroy();
            _banner = null;
        }
        AdSize adaptiveSize = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);

        //_banner = new BannerView(banner_id, AdSize.Banner, AdPosition.Top);
        // _banner.LoadAd(_adRequest);

    }

    // 전면광고
    public void ShowInterstitialAds()
    {
        if(_intertstitialAd != null && _intertstitialAd.CanShowAd())
        {
            _intertstitialAd.Show();
        }
        else
        {
            PrepareADS();
        }
    }

    private void OnAdLoadCallback(InterstitialAd ad, LoadAdError error)
    {
        if(error != null || ad == null)
        {
            Debug.LogError("전면 광고 로드 실패! : " + error);
            return;
        }

        Debug.Log("전면 광고 로드! : " + ad.GetResponseInfo());

        _intertstitialAd = ad;
        RegisterReloadHandler(_intertstitialAd);
    }

    private void RegisterReloadHandler(InterstitialAd ad)
    {
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("전면 광고가 닫혔습니다.");
            PrepareADS();
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("전면 광고가 실패하였습니다. : " + error);
            PrepareADS();
        };
    }

}
