using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class Main_Quest : MonoBehaviour
{
    List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
    Dictionary<string, object> Quest;

    public static int monster_Index;
    [SerializeField] private TextMeshProUGUI TitleText, ExplaneText, CountText, RewardText;

    Quest_Type m_State;
    public static bool GetEnemy = false;
    bool Reward = false;
    [SerializeField] private GameObject HandObj, RewardImage;
    [SerializeField] private GameObject Lock;

    bool isQuestFinished = false;

    private void Start()
    {
        Data = CSV_Importer.Quest;
        NextQuest();
    }

    private void Update()
    {
        if (isQuestFinished) return;
        GetQuest();
    }

    public void NextQuest()
    {
        if (Data_Manager.m_Data.Quest_Count >= Data.Count)
        {
            isQuestFinished = true;
            // 모든 퀘스트 완료 처리
            RewardImage.SetActive(false);
            TitleText.gameObject.SetActive(false);
            ExplaneText.gameObject.SetActive(false);
            CountText.gameObject.SetActive(false);
            HandObj.SetActive(false);
            Lock.SetActive(true); // 락 표시

            return; // 다음 퀘스트로 진행하지 않음
        }

        isQuestFinished = false;

        // 다음 퀘스트 세팅
        monster_Index = 0;
        Quest = Data[Data_Manager.m_Data.Quest_Count];
        m_State = (Quest_Type)Enum.Parse(typeof(Quest_Type), Quest["Key"].ToString());

        if (m_State == Quest_Type.Monster) GetEnemy = true;
        TitleText.text = Local_Manager.local_Data["Quest"].Get_Data() + (Data_Manager.m_Data.Quest_Count + 1).ToString();
        ExplaneText.text = Localization_Counting(m_State);
        RewardText.text = Quest["Reward"].ToString();
    }

    void GetQuest()
    {
        Color color = Counting(m_State) >= Convert.ToInt32(Quest["Value"]) ? Color.green : Color.red;

        CountText.text = "(" + Counting(m_State).ToString() + "/" +Convert.ToInt32(Quest["Value"]) + ")";
        CountText.color = color;

        Reward = Counting(m_State) >= Convert.ToInt32(Quest["Value"]) ? true : false;

        if (HandObj.activeSelf != Reward) HandObj.SetActive(Reward);
    }

    public void GetQuestButton()
    {
        if (!Reward) return;

        Base_Manager.Pool.Pooling_OBJ("COIN_PARENT").Get((value) =>
        {
            value.GetComponent<COIN_PARENT>().Init(Camera.main.ScreenToWorldPoint(transform.position), false, Coin_type.Dia, Convert.ToInt32(Quest["Reward"]));
        });

        Counting_Reset(m_State);

        Data_Manager.m_Data.Quest_Count++;
        //Main_UI.instance.TextCheck();
        NextQuest();
    }

    private int Counting(Quest_Type type)
    {
        switch(type)
        {
            case Quest_Type.Monster: return monster_Index;
            case Quest_Type.Gold: return Data_Manager.m_Data.GoldDungeon_Count;
            case Quest_Type.Dia: return Data_Manager.m_Data.DiaDungeon_Count;
            case Quest_Type.Upgrade: return Data_Manager.m_Data.UpgradeCount;
            case Quest_Type.Hero: return Data_Manager.m_Data.Hero_Summon_Count;
            case Quest_Type.Stage: return Data_Manager.m_Data.Stage;
            case Quest_Type.Artifact: return Data_Manager.m_Data.Relic_Summon_Count;
        }
        return 0;
    }

    private string Localization_Counting(Quest_Type type)
    {
        switch (type)
        {
            case Quest_Type.Monster: return Local_Manager.local_Data["questMonster"].Get_Data();
            case Quest_Type.Gold: return Local_Manager.local_Data["questGold"].Get_Data();
            case Quest_Type.Dia: return Local_Manager.local_Data["questDia"].Get_Data();
            case Quest_Type.Upgrade: return Local_Manager.local_Data["questUpgrade"].Get_Data();
            case Quest_Type.Hero: return Local_Manager.local_Data["questHero"].Get_Data();
            case Quest_Type.Stage: return Local_Manager.local_Data["questStage"].Get_Data();
            case Quest_Type.Artifact: return Local_Manager.local_Data["questArtifact"].Get_Data();
        }
        return "";
    }

    private void Counting_Reset(Quest_Type type)
    {
        switch (type)
        {
            case Quest_Type.Monster:
                monster_Index = 0;
                break;
            case Quest_Type.Gold:
                Data_Manager.m_Data.GoldDungeon_Count = 0;
                break;
            case Quest_Type.Dia:
                Data_Manager.m_Data.DiaDungeon_Count = 0;
                break;
            case Quest_Type.Upgrade:
                Data_Manager.m_Data.UpgradeCount = 0;
                break;
            case Quest_Type.Hero:
                Data_Manager.m_Data.Hero_Summon_Count = 0;
                break;
            case Quest_Type.Stage:
                Data_Manager.m_Data.Stage = 0;
                break;
            case Quest_Type.Artifact:
                Data_Manager.m_Data.Relic_Summon_Count = 0;
                break;
        }
    }

}
