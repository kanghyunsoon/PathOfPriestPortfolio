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

    public void WriteData()
    {
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
            DateTime LastDate = DateTime.Parse(data.EndDate);
            data.EndDate = Timer_NTP.NowTime.ToString();

            if (GetDataItem(LastDate, DateTime.Now))
            {
                data.Key[0] = 2;
                data.Key[1] = 2;
                data.Hero_ADS_Count = 0;
                data.Relic_ADS_Count = 0;
            }
        }

        string Default_json = JsonUtility.ToJson(data);
        reference.Child("USER").Child(currentUser.UserId).Child("DATA").SetRawJsonValueAsync(Default_json).ContinueWithOnMainThread(task =>
        {

            if (!task.IsCompleted)
            {
                Debug.LogError("기본 데이터 쓰기 실패 : " + task.Exception.ToString());
            }

        });

        #endregion

        #region CHARACTER_DATA
        string Character_Json = JsonConvert.SerializeObject(Base_Manager.Data.Character_Holder);
        reference.Child("USER").Child(currentUser.UserId).Child("CHARACTER").SetRawJsonValueAsync(Character_Json).ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("케릭터 데이터 쓰기 실패 : " + task.Exception.ToString());
            }

        });
        #endregion

        #region ITEM_DATA
        string Item_json = JsonConvert.SerializeObject(Base_Manager.Data.Item_Holder);
        reference.Child("USER").Child(currentUser.UserId).Child("ITEM").SetRawJsonValueAsync(Item_json).ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("아이템 데이터 쓰기 실패 : " + task.Exception.ToString());
            }

        });
        #endregion

        #region SMELT_DATA
        string Smelt_json = JsonConvert.SerializeObject(Base_Manager.Data.m_Data_Smelt);
        reference.Child("USER").Child(currentUser.UserId).Child("SMELT").SetRawJsonValueAsync(Smelt_json).ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted)
            {
                Debug.LogError("각인 데이터 쓰기 실패 : " + task.Exception.ToString());
            }

        });
        #endregion

        #region MASTERY_DATA
        string masteryJson = JsonConvert.SerializeObject(Base_Manager.Data.m_Data_Mastery);
        reference.Child("USER").Child(currentUser.UserId).Child("MASTERY").SetRawJsonValueAsync(masteryJson).ContinueWithOnMainThread(task =>
        {
            if (!task.IsCompleted)
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
            if (!task.IsCompleted)
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
            if (!task.IsCompleted)
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
            if (!task.IsCompleted)
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


        int completeCount = 0;
        bool isDebuffLoaded = false;

        void TryFinalizeLoad()
        {
            Debug.Log($"[TryFinalizeLoad] completeCount: {completeCount}, isDebuffLoaded: {isDebuffLoaded}");
            if (completeCount >= 4 && isDebuffLoaded)
            {
                Debug.Log("[TryFinalizeLoad] 모든 데이터 로드 완료. 초기화 실행.");
                Base_Manager.Data.Init(); // 캐릭터/아이템 세팅
                OnFirebaseLoadComplete?.Invoke();
                LoadingScene.instance.LoadingMain();
                LoadSetCharacter();
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
            if (task.IsCompleted)
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

                    Data_Manager.m_Data = new Data
                    {
                        ATK = 50,
                        HP = 100,
                        StartDate = DateTime.Now.ToString(),
                        EndDate = DateTime.Now.ToString(),
                        Key = new int[] { 2, 2 },
                         DailyQuests = new bool[8], // 필수 초기화
                        Achievement_B = new bool[Base_Manager.Quest.Achievement_Lists.Count]
                    };

                    Base_Manager.Quest.Init(); // 여기 추가!
                }
                else
                {
                    var default_Data = JsonUtility.FromJson<Data>(snapshot.GetRawJsonValue());
                    Data data = default_Data ?? new Data();
                    data.StartDate = Timer_NTP.NowTime.ToString();

                    DateTime startDate = DateTime.Parse(data.StartDate);
                    DateTime endDate = DateTime.Parse(data.EndDate);

                    if (GetDataItem(startDate, endDate))
                    {
                        data.Key[0] = 2;
                        data.Key[1] = 2;
                        // DailyQuest
                        data.DailyAttendance = 1;
                        data.LevelUp = 0;
                        data.Dungeon = 0;
                        data.Summon = 0;
                        data.ADS = 0;
                        data.UseDiamond = 0;
                        data.MonsterKill = 0;
                        data.StayTime = 0;

                        for (int i = 0; i < data.DailyQuests.Length; i++) data.DailyQuests[i] = false;
                    }

                    Data_Manager.m_Data = data;
                    Base_Manager.Quest.Init();

                }

            }
            else
            {
                Debug.LogError("데이터 읽기 실패 : " + task.Exception.ToString());
            }
        });
        #endregion

        #region CHARACTER_DATA
        reference.Child("USER").Child(currentUser.UserId).Child("CHARACTER").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
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
            }

            OnSubLoadComplete("CHARACTER");

        });
        #endregion

        #region Item_Data
        reference.Child("USER").Child(currentUser.UserId).Child("ITEM").GetValueAsync().ContinueWithOnMainThread(task =>
       {
           if (task.IsCompleted)
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
           }
           OnSubLoadComplete("ITEM");
       });
        #endregion

        #region Smelt_Data
        reference.Child("USER").Child(currentUser.UserId).Child("SMELT").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
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
            }
            OnSubLoadComplete("SMELT");


        });
        #endregion

        #region MASTERY_DATA
        reference.Child("USER").Child(currentUser.UserId).Child("MASTERY").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
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
            }
            OnSubLoadComplete("MASTERY");
        });
        #endregion


        #region SET_ITEM
        reference.Child("USER").Child(currentUser.UserId).Child("SET_ITEM").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    string[] itemNames = JsonConvert.DeserializeObject<string[]>(snapshot.GetRawJsonValue());
                    for (int i = 0; i < itemNames.Length; i++)
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

                Relic_Manager.instance.Initialize();
            }
            else
            {
                Debug.LogError("SET_ITEM 불러오기 실패 : " + task.Exception.ToString());
            }
        });
        #endregion

        #region DEBUFF_CHARACTER
        reference.Child("USER").Child(currentUser.UserId).Child("DEBUFF").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
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

    private bool GetDataItem(DateTime startTime, DateTime endTime)
    {
        if (startTime.Day != endTime.Day)
        {
            return true;
        }
        return false;
    }

    private void LoadSetCharacter()
    {
        reference.Child("USER").Child(currentUser.UserId).Child("SET_CHARACTER").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
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
            }
            else
            {
                Debug.LogError("SET_CHARACTER 불러오기 실패 : " + task.Exception.ToString());
            }
        });

    }
}
