using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

// public class User
// {
//     public string userName;
//     public int Stage;
// }
public partial class Firebase_Manager
{
    // 데이터만 저장하는 용도로 사용할 예정

    public Action OnFirebaseLoadComplete;
    public Dictionary<string, bool> _loadedDebuffDict = new Dictionary<string, bool>();

    // 서버 데이터를 모두 불러온 뒤에만 저장한다.
    // 불러오기 전에 저장하면 기존 데이터를 빈 값으로 덮어쓴다.
    private bool isDataLoaded = false;

    public void WriteData()
    {
        if (!isDataLoaded || currentUser == null)
        {
            return;
        }

        // // json
        // User user = new User();
        // user.userName = currentUser.UserId;
        // user.Stage = Data_Manager.m_Data.Stage;

        // string json = JsonUtility.ToJson(user);
        // //  JsonUtility.ToJson = 클래스를 json스트링으로 변환
        // // newtonsoft.json -> JsonConvert.SerializeObject -> 딕셔너리를 json화



        #region DEFAULT_DATA
        Data data = new Data();
        if (Data_Manager.m_Data != null)
        {
            data = Data_Manager.m_Data;
            DateTime now = Timer_NTP.NowTime;

            // 플레이 중에 자정을 넘긴 경우
            if (Timer_NTP.TryParseSaved(data.EndDate, out DateTime lastUtc))
            {
                ResetDailyIfNewDay(data, lastUtc.ToLocalTime(), now);
            }

            data.EndDate = Timer_NTP.ToSaveString(now);
        }

        string Default_json = JsonUtility.ToJson(data);
        reference.Child("USER").Child(currentUser.UserId).Child("DATA").SetRawJsonValueAsync(Default_json).ContinueWithOnMainThread(task =>
        {

            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("기본 데이터 쓰기 실패 : " + task.Exception.ToString());
            }

        });

        #endregion

        #region CHARACTER_DATA
        string Character_Json = JsonConvert.SerializeObject(Base_Manager.Data.Character_Holder);
        reference.Child("USER").Child(currentUser.UserId).Child("CHARACTER").SetRawJsonValueAsync(Character_Json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("케릭터 데이터 쓰기 실패 : " + task.Exception.ToString());
            }

        });
        #endregion

        #region ITEM_DATA
        string Item_json = JsonConvert.SerializeObject(Base_Manager.Data.Item_Holder);
        reference.Child("USER").Child(currentUser.UserId).Child("ITEM").SetRawJsonValueAsync(Item_json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("아이템 데이터 쓰기 실패 : " + task.Exception.ToString());
            }

        });
        #endregion

        #region SMELT_DATA
        string Smelt_json = JsonConvert.SerializeObject(Base_Manager.Data.m_Data_Smelt);
        reference.Child("USER").Child(currentUser.UserId).Child("SMELT").SetRawJsonValueAsync(Smelt_json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("각인 데이터 쓰기 실패 : " + task.Exception.ToString());
            }

        });
        #endregion

        #region MASTERY_DATA
        string masteryJson = JsonConvert.SerializeObject(Base_Manager.Data.m_Data_Mastery);
        reference.Child("USER").Child(currentUser.UserId).Child("MASTERY").SetRawJsonValueAsync(masteryJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("마스터리 데이터 저장 실패 : " + task.Exception.ToString());
            }
        });
        #endregion



        #region SET_CHARACTER
        string[] SetCharacterNames = new string[Base_Manager.Data.m_Set_Character.Length];

        for (int i = 0; i < Base_Manager.Data.m_Set_Character.Length; i++)
        {
            if (Base_Manager.Data.m_Set_Character[i] != null)
            {
                SetCharacterNames[i] = Base_Manager.Data.m_Set_Character[i].Data.m_CharacterName;
            }
            else
            {
                SetCharacterNames[i] = ""; // null 방지용 빈 문자열
            }
        }

        string SetCharacter_json = JsonConvert.SerializeObject(SetCharacterNames);
        reference.Child("USER").Child(currentUser.UserId).Child("SET_CHARACTER").SetRawJsonValueAsync(SetCharacter_json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("SetCharacter 저장 실패 : " + task.Exception.ToString());
            }
        });
        #endregion

        #region SET_ITEM
        string[] SetItemNames = new string[Base_Manager.Data.m_Set_Item.Length];

        for (int i = 0; i < Base_Manager.Data.m_Set_Item.Length; i++)
        {
            if (Base_Manager.Data.m_Set_Item[i] != null)
            {
                SetItemNames[i] = Base_Manager.Data.m_Set_Item[i].name;
            }
            else
            {
                SetItemNames[i] = ""; // null 방지용
            }
        }

        string SetItem_json = JsonConvert.SerializeObject(SetItemNames);
        reference.Child("USER").Child(currentUser.UserId).Child("SET_ITEM").SetRawJsonValueAsync(SetItem_json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("SetItem 저장 실패 : " + task.Exception.ToString());
            }
        });
        #endregion

        #region DEBUFF_CHARACTER
        Dictionary<string, bool> debuffDict = new Dictionary<string, bool>();
        foreach (var pair in Base_Manager.Data.m_Data_Character)
        {
            debuffDict[pair.Key] = pair.Value.isDebuff;
        }

        string Debuff_json = JsonConvert.SerializeObject(debuffDict);
        reference.Child("USER").Child(currentUser.UserId).Child("DEBUFF").SetRawJsonValueAsync(Debuff_json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("디버프 데이터 저장 실패 : " + task.Exception.ToString());
            }
        });
        #endregion

    }



    public void ReadData()
    {
        if (currentUser == null)
        {
            Debug.LogError(" [ReadData] currentUser가 null입니다! 로그인 처리 이후에 호출되었는지 확인하세요.");
            return;
        }

        Debug.Log(" [ReadData] 호출됨 / 유저 ID: " + currentUser.UserId);

        isDataLoaded = false;

        // DATA, CHARACTER, ITEM, SMELT, MASTERY
        const int subLoadCount = 5;
        int completeCount = 0;
        bool isDebuffLoaded = false;
        bool loadFailed = false;

        void TryFinalizeLoad()
        {
            Debug.Log($"[TryFinalizeLoad] completeCount: {completeCount}, isDebuffLoaded: {isDebuffLoaded}");
            if (completeCount >= subLoadCount && isDebuffLoaded)
            {
                if (loadFailed)
                {
                    // 일부만 불러온 채로 시작하면 다음 자동 저장이 서버 데이터를 덮어쓴다.
                    Debug.LogError("[TryFinalizeLoad] 불러오기에 실패한 데이터가 있어 게임을 시작하지 않습니다.");
                    Base_Canvas.instance.Get_UI("#NetError");
                    Base_Manager.instance.Invoke("QuitGame", 3f);
                    return;
                }

                Debug.Log("[TryFinalizeLoad] 모든 데이터 로드 완료. 초기화 실행.");
                isDataLoaded = true;
                Base_Manager.Data.Init(); // 캐릭터/아이템 세팅
                OnFirebaseLoadComplete?.Invoke();
                LoadingScene.instance.LoadingMain();
                LoadSetCharacter();
                LoadSetItem(); // m_Data_Item은 Data.Init() 이후에 채워진다
            }
        }

        void OnSubLoadComplete(string label)
        {
            completeCount++;
            Debug.Log($"[OnSubLoadComplete] {label} 로딩 완료. 현재 count: {completeCount}");
            TryFinalizeLoad();
        }



        #region DEFAULT_DATA
        reference.Child("USER").Child(currentUser.UserId).Child("DATA").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;

                // JsonUtility.FromJson => json문자열을 클래스로 변경
                // User user = JsonUtility.FromJson<User>(snapshot.GetRawJsonValue());
                // 데이터가 없으면 기본 데이터를 작성
                if (!snapshot.Exists)
                {
                    Debug.Log("[DEFAULT_DATA] 데이터 없음. 새로 생성");
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();

                    string now = Timer_NTP.ToSaveString(Timer_NTP.NowTime);
                    Data_Manager.m_Data = new Data
                    {
                        ATK = 50,
                        HP = 100,
                        StartDate = now,
                        EndDate = now,
                        Key = new int[] { 2, 2 },
                         DailyQuests = new bool[8], // 필수 초기화
                        Achievement_B = new bool[Base_Manager.Quest.Achievement_Lists.Count]
                    };

                    Utils.SetLastSessionEnd(null);
                    Base_Manager.Quest.Init(); // 여기 추가!
                }
                else
                {
                    var default_Data = JsonUtility.FromJson<Data>(snapshot.GetRawJsonValue());
                    Data data = default_Data ?? new Data();
                    DateTime now = Timer_NTP.NowTime;
                    data.StartDate = Timer_NTP.ToSaveString(now);

                    if (Timer_NTP.TryParseSaved(data.EndDate, out DateTime lastUtc))
                    {
                        Utils.SetLastSessionEnd(lastUtc);
                        ResetDailyIfNewDay(data, lastUtc.ToLocalTime(), now);
                    }
                    else
                    {
                        Debug.LogWarning("[DEFAULT_DATA] EndDate를 읽을 수 없어 오프라인 보상을 건너뜁니다 : " + data.EndDate);
                        Utils.SetLastSessionEnd(null);
                    }

                    Data_Manager.m_Data = data;
                    Base_Manager.Quest.Init();

                }

            }
            else
            {
                Debug.LogError("데이터 읽기 실패 : " + task.Exception.ToString());
                loadFailed = true;
            }
            OnSubLoadComplete("DATA");
        });
        #endregion

        #region CHARACTER_DATA
        reference.Child("USER").Child(currentUser.UserId).Child("CHARACTER").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {

                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, Holder>>(snapshot.GetRawJsonValue());
                    Base_Manager.Data.Character_Holder = data;
                }
                else
                {
                    Debug.Log("[CHARACTER_DATA] 데이터 없음. 빈 딕셔너리 생성");
                    Base_Manager.Data.Character_Holder = new Dictionary<string, Holder>(); // null 방지
                }

            }
            else
            {
                Debug.LogError($"[CHARACTER_DATA] 불러오기 실패: {task.Exception}");
                loadFailed = true;
            }

            OnSubLoadComplete("CHARACTER");

        });
        #endregion

        #region Item_Data
        reference.Child("USER").Child(currentUser.UserId).Child("ITEM").GetValueAsync().ContinueWithOnMainThread(task =>
       {
           if (!task.IsFaulted && !task.IsCanceled)
           {
               DataSnapshot snapshot = task.Result;

               if (snapshot.Exists)
               {
                   var data = JsonConvert.DeserializeObject<Dictionary<string, Holder>>(snapshot.GetRawJsonValue());
                   Base_Manager.Data.Item_Holder = data;
               }
               else
               {
                   Debug.Log("[ITEM_DATA] 데이터 없음. 빈 딕셔너리 생성");
                   Base_Manager.Data.Item_Holder = new Dictionary<string, Holder>(); // null 방지
               }
           }
           else
           {
               Debug.LogError($"[ITEM_DATA] 불러오기 실패: {task.Exception}");
               loadFailed = true;
           }
           OnSubLoadComplete("ITEM");
       });
        #endregion

        #region Smelt_Data
        reference.Child("USER").Child(currentUser.UserId).Child("SMELT").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    var data = JsonConvert.DeserializeObject<List<Smelt_Holder>>(snapshot.GetRawJsonValue());
                    Base_Manager.Data.m_Data_Smelt = data;
                }
                else
                {
                    Debug.Log("[SMELT_DATA] 데이터 없음. 빈 리스트 생성");
                    Base_Manager.Data.m_Data_Smelt = new List<Smelt_Holder>(); // null 방지
                }

            }
            else
            {
                Debug.LogError($"[SMELT_DATA] 불러오기 실패: {task.Exception}");
                loadFailed = true;
            }
            OnSubLoadComplete("SMELT");


        });
        #endregion

        #region MASTERY_DATA
        reference.Child("USER").Child(currentUser.UserId).Child("MASTERY").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<Status_Holder, float>>(snapshot.GetRawJsonValue());
                    Base_Manager.Data.m_Data_Mastery = data;
                }
                else
                {
                    Debug.Log("[MASTERY_DATA] 데이터 없음. 빈 딕셔너리 생성");
                    Base_Manager.Data.m_Data_Mastery = new Dictionary<Status_Holder, float>();
                }
            }
            else
            {
                Debug.LogError($"[MASTERY_DATA] 불러오기 실패: {task.Exception}");
                loadFailed = true;
            }
            OnSubLoadComplete("MASTERY");
        });
        #endregion


        #region DEBUFF_CHARACTER
        reference.Child("USER").Child(currentUser.UserId).Child("DEBUFF").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, bool>>(snapshot.GetRawJsonValue());

                    foreach (var pair in data)
                    {
                        _loadedDebuffDict[pair.Key] = pair.Value;
                    }
                }
                else
                {
                    Debug.Log("[DEBUFF] 디버프 데이터 없음");
                }
            }
            else
            {
                Debug.LogError($"[DEBUFF] 불러오기 실패: {task.Exception}");
                loadFailed = true;
            }
            isDebuffLoaded = true;
            TryFinalizeLoad();
        });
        #endregion

        // 여기를 수정: 게스트가 아닐 경우에만 GPGS 실행
        if (PlayerPrefs.GetInt("Login") == (int)LoginType.Google)
        {
            Base_Manager.GPGS.Initialize();
        }
        Base_Manager.ADS.Init();
    }

    // 마지막 저장 이후 날짜가 바뀌었으면 일일 콘텐츠를 초기화한다. (사용자 기기 시간대 기준)
    // 예전에는 Day 값만 비교해서 1월 5일 -> 2월 5일처럼 날짜 숫자가 같으면 초기화되지 않았다.
    private void ResetDailyIfNewDay(Data data, DateTime lastLocal, DateTime nowLocal)
    {
        if (nowLocal.Date <= lastLocal.Date)
        {
            return;
        }

        data.Key[0] = 2;
        data.Key[1] = 2;
        data.Hero_ADS_Count = 0;
        data.Relic_ADS_Count = 0;

        // DailyQuest
        data.DailyAttendance = 1;
        data.LevelUp = 0;
        data.Dungeon = 0;
        data.Summon = 0;
        data.ADS = 0;
        data.UseDiamond = 0;
        data.MonsterKill = 0;
        data.StayTime = 0;

        if (data.DailyQuests != null)
        {
            for (int i = 0; i < data.DailyQuests.Length; i++) data.DailyQuests[i] = false;
        }
    }

    private void LoadSetCharacter()
    {
        reference.Child("USER").Child(currentUser.UserId).Child("SET_CHARACTER").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string[] names = JsonConvert.DeserializeObject<string[]>(snapshot.GetRawJsonValue());
                    for (int i = 0; i < names.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(names[i]) && Base_Manager.Data.m_Data_Character.ContainsKey(names[i]))
                        {
                            Base_Manager.Data.m_Set_Character[i] = Base_Manager.Data.m_Data_Character[names[i]];
                        }
                        else
                        {
                            Base_Manager.Data.m_Set_Character[i] = null; // 존재하지 않으면 null
                        }
                    }
                }

                Base_Manager.instance.InitializeDefaultAbilities();
            }
            else
            {
                Debug.LogError("SET_CHARACTER 불러오기 실패 : " + task.Exception.ToString());
            }
        });

    }

    private void LoadSetItem()
    {
        reference.Child("USER").Child(currentUser.UserId).Child("SET_ITEM").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string[] itemNames = JsonConvert.DeserializeObject<string[]>(snapshot.GetRawJsonValue());
                    for (int i = 0; i < itemNames.Length && i < Base_Manager.Data.m_Set_Item.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(itemNames[i]) && Base_Manager.Data.m_Data_Item.ContainsKey(itemNames[i]))
                        {
                            Base_Manager.Data.m_Set_Item[i] = Base_Manager.Data.m_Data_Item[itemNames[i]].Data;
                        }
                        else
                        {
                            Base_Manager.Data.m_Set_Item[i] = null;
                        }
                    }
                }
                else
                {
                    Debug.Log("SET_ITEM 데이터가 존재하지 않습니다.");
                }

                // 다시 불러올 때 유물 효과가 중복으로 연결되지 않게 비우고 연결한다.
                Delegate_Holder.ClearEvent();
                Relic_Manager.instance.Initialize();
                Base_Manager.instance.InitializeDefaultAbilities();
            }
            else
            {
                Debug.LogError("SET_ITEM 불러오기 실패 : " + task.Exception.ToString());
            }
        });
    }
}
