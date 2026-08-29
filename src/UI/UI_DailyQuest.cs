using UnityEngine;
using System.Collections.Generic;

public class UI_DailyQuest : UI_Base
{
    public Quest_Part QuestPanel;
    public Transform Content;
    List<Transform> InitPanels = new List<Transform>();
    List<GameObject> Garvage = new List<GameObject>();


    public override bool Init()
    {
        if (Garvage.Count > 0)
        {
            for (int i = 0; i < Garvage.Count; i++) Destroy(Garvage[i]);

            Garvage.Clear();
            InitPanels.Clear();
        }

        foreach (var quest in Base_Manager.Quest.activeQuests)
        {
            var go = Instantiate(QuestPanel, Content);
            go.gameObject.SetActive(true);
            go.Init(quest, this);
            InitPanels.Add(go.transform);
            Garvage.Add(go.gameObject);
        }

        for (int i = 0; i < InitPanels.Count; i++)
        {
            if (Data_Manager.m_Data.DailyQuests[i] == true)
            {
                InitPanels[i].SetAsLastSibling();
            }
        }
        return base.Init();
    }
}
