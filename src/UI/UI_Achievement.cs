using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class UI_Achievement : UI_Base
{
    [SerializeField] private Achievement_Part part;
    [SerializeField] private Transform Content;
    [SerializeField] private TextMeshProUGUI StatusText;
    List<Transform> InitPanels = new List<Transform>();
    List<GameObject> Garvage = new List<GameObject>();

    public override bool Init()
    {
        int achievementCount = Base_Manager.Quest.Achievement_Lists.Count;

        if (Data_Manager.m_Data.Achievement_B == null ||
            Data_Manager.m_Data.Achievement_B.Length != achievementCount)
        {
            Debug.LogWarning("[업적] Achievement_B 배열 길이가 맞지 않아 재생성합니다.");

            // 기존 데이터를 유지하면서 확장
            bool[] old = Data_Manager.m_Data.Achievement_B;
            Data_Manager.m_Data.Achievement_B = new bool[achievementCount];

            for (int i = 0; i < Mathf.Min(old.Length, achievementCount); i++)
            {
                Data_Manager.m_Data.Achievement_B[i] = old[i];
            }
        }

        if (Garvage.Count > 0)
        {
            for (int i = 0; i < Garvage.Count; i++) Destroy(Garvage[i]);
            Garvage.Clear();
            InitPanels.Clear();
        }


        for(int i =0; i< Base_Manager.Quest.Achievement_Lists.Count; i++)
        {
            var go = Instantiate(part, Content);
            go.gameObject.SetActive(true);
            go.Init(Base_Manager.Quest.Achievement_Lists[i], i, this);
            InitPanels.Add(go.transform);
            Garvage.Add(go.gameObject);
        }

        for(int i = 0; i< InitPanels.Count; i++)
        {
            if(Data_Manager.m_Data.Achievement_B[i] == true)
            {
                InitPanels[i].SetAsLastSibling();
            }
        }
        CheckStatus();

        return base.Init();
    }

    private void CheckStatus()
    {
        var temp = Base_Manager.Quest.Achievement_Status_Data;
        StatusText.text = string.Format("{0} / {1}\n{2} / {3}\n{4} / {5}\n{6} / {7}",
            LocalTemp(Status_Holder.ATK, temp.ATK),
            LocalTemp(Status_Holder.HP, temp.HP),
            LocalTemp(Status_Holder.Money, temp.Money),
            LocalTemp(Status_Holder.Item, temp.Item),
            LocalTemp(Status_Holder.ATK_Speed, temp.ATK_Speed),
            LocalTemp(Status_Holder.Skill, temp.Skill),
            LocalTemp(Status_Holder.Critical_P, temp.Critical_P),
            LocalTemp(Status_Holder.Critical_D, temp.Critical_D));

    }

    private string PlusAndMinus(float value)
    {
        var SignTemp = (int)Mathf.Sign(value) == 1 ? " +" : " ";
        return SignTemp;
    }

    private string LocalTemp(Status_Holder holder, float value)
    {
        string temp = Local_Manager.local_Data[holder.ToString()].Get_Data() +
            PlusAndMinus(value) +
            string.Format("{0:0.0}", value)
            + "%" ;
        return temp;
    }
}
