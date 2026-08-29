using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Base_Manager : MonoBehaviour
{
    public static Base_Manager instance = null;

    #region Parameter
    private static Pool_Manager s_Pool = new Pool_Manager();
    private static Player_Manager s_Player = new Player_Manager();
    private static Data_Manager s_Data = new Data_Manager();
    private static Item_Manager s_Item = new Item_Manager();
    private static Character_Manager s_Character = new Character_Manager();
    private static Inventory_Manager s_Inventory = new Inventory_Manager();
    private static ADS_Manager s_ADS = new ADS_Manager();
    private static Firebase_Manager s_Firebase = new Firebase_Manager();
    private static IAP_Manager s_IAP = new IAP_Manager();
    private static Sound_Manager s_Sound = new Sound_Manager();
    private static Local_Manager s_Local = new Local_Manager();
    private static Quest_Manager s_Quest = new Quest_Manager();
    private static Time_Manager s_Time = new Time_Manager();
    private static GPGS_Manager s_GPGS = new GPGS_Manager();

    public static Pool_Manager Pool { get { return s_Pool; } }
    public static Player_Manager Player { get { return s_Player; } }
    public static Data_Manager Data { get { return s_Data; } }
    public static Item_Manager Item { get { return s_Item; } }
    public static Character_Manager Character { get { return s_Character; } }
    public static Inventory_Manager Inventory { get { return s_Inventory; } }
    public static ADS_Manager ADS { get { return s_ADS; } }
    public static Firebase_Manager Firebase { get { return s_Firebase; } }
    public static IAP_Manager IAP { get { return s_IAP; } }
    public static Sound_Manager Sound { get { return s_Sound; } }
    public static Local_Manager Local { get { return s_Local; } }
    public static Quest_Manager Quest { get { return s_Quest; } }
    public static Time_Manager m_Time { get { return s_Time; } }
    public static GPGS_Manager GPGS { get { return s_GPGS; } }

    #endregion

    public static bool isNight = false;

    public static bool isFast = false;

    public static bool GetGameStart = false;
    float Save_Timer = 0.0f;

    private float stayTimer = 0.0f;

    private void Awake()
    {
        initalize();
    }

    private void Update()
    {
        if (Data_Manager.m_Data.StayTime < 600)
        {
            stayTimer += Time.unscaledDeltaTime;

            if (stayTimer >= 1.0f)
            {
                stayTimer = 0.0f;
                Data_Manager.m_Data.StayTime += 1;
            }
        }

        DateTime now = Timer_NTP.NowTime;
        int hour = now.Hour;

        // 밤 10시(22시)부터 새벽 6시(6시 미만)까지 체크
        if (hour >= 22 || hour < 6)
            isNight = true; // 150% 증가 → 기존의 250%가 됨 (100% + 150%)
        else
            isNight = false;


        if (GetGameStart == false) return;

        Save_Timer += Time.unscaledDeltaTime;
        if (Save_Timer >= 10.0f)
        {
            Save_Timer = 0.0f;
            Firebase.WriteData();
        }

        for (int i = 0; i < Data_Manager.m_Data.Buff_Timers.Length; i++)
        {
            if (Data_Manager.m_Data.Buff_Timers[i] >= 0.0f)
            {
                Data_Manager.m_Data.Buff_Timers[i] -= Time.unscaledDeltaTime;
            }
        }

        if (Data_Manager.m_Data.Buff_x2 > 0.0f) Data_Manager.m_Data.Buff_x2 -= Time.unscaledDeltaTime;
    }

    private void initalize()
    {
        if(instance == null)
        {
            instance = this;

            Pool.Initalize(transform);
            ADS.Init();
            Firebase.Init();
            IAP.InitUnityIAP();
            Sound.Init();
            Local.Init();
            m_Time.Init();

            Invoke("InitializeDefaultAbilities", 1.0f);
            // StartCoroutine(ADCoroutine()); //광고 체크


            // Character.GetCharacter(0, "Hunter");

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

    }

    public void InitializeDefaultAbilities()
    {
        // 캐릭터 기본 능력 적용
        foreach (var characterHolder in Data.m_Set_Character)
        {
            if (characterHolder != null)
            {
                Debug.Log("1");
                Player.Set_Character_DefaultAbility(true, characterHolder.Data);
            }
        }

        // 아이템 기본 능력 적용
        foreach (var itemScriptable in Data.m_Set_Item)
        {
            if (itemScriptable != null)
            {
                Player.Set_Relic_DefaultAbility(true, itemScriptable);
            }
        }
    }

    // 광고 체크 ↓ /////////////////////////////////////////////////////////////////////////////////
    IEnumerator ADCoroutine()
    {
        yield return new WaitForSeconds(3.0f);
        // ADS.ShowInterstitialAds();
        ADS.ShowRewardedAds(GetReward);
    }
    private void GetReward() => Debug.Log("보상형 광고를 시청하고, 아이템을 획득하였습니다!.");
    // 광고체크 ↑ /////////////////////////////////////////////////////////////////////////////////

    public GameObject Instantiate_Path(string path)
    {
        return Instantiate(Resources.Load<GameObject>(path));
    }

    public void Return_Pool(float timer, GameObject obj, string path)
    {
        StartCoroutine(Return_Pool_Courtine(timer, obj, path));
    }

    public void Coroutine_Action(Action action, float timer)
    {
        StartCoroutine(Action_Coroutine(action , timer));
    }

    IEnumerator Return_Pool_Courtine(float timer, GameObject obj, string path)
    {
        yield return new WaitForSeconds(timer);
        Pool.m_pool_Dictionary[path].Return(obj);
    }

    IEnumerator Action_Coroutine(Action action, float timer)
    {
        yield return new WaitForSeconds(timer);
        action?.Invoke();
    }

    private void OnDestroy()
    {
        if (GetGameStart)
        {
            Firebase.WriteData();

        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
