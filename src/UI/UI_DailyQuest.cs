using UnityEngine;
using System.Collections.Generic;

public class UI_DailyQuest : UI_Base
{
    public Quest_Part QuestPanel;
    public Transform Content;
    List<Quest_Part> InitPanels = new List<Quest_Part>();
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
            InitPanels.Add(go);
            Garvage.Add(go.gameObject);
        }

        // 받은 퀘스트는 아래로 보낸다. 목록 순서와 저장 순서가 달라 퀘스트별 저장 위치로 확인한다.
        for (int i = 0; i < InitPanels.Count; i++)
        {
            if (InitPanels[i].IsCollected)
            {
                InitPanels[i].transform.SetAsLastSibling();
            }
        }
        return base.Init();
    }
}
