using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class Achievement_Part : MonoBehaviour
{
    [SerializeField] private GameObject ContentPart;
    [SerializeField] private Transform Content;
    [SerializeField] private TextMeshProUGUI Title, RewardTitle;
    [SerializeField] private Image Lock;
    [SerializeField] private Button CollectButton;
    [SerializeField] private GameObject Collected;
    List<GameObject> Garvage = new List<GameObject>();
    int valueCount;
    Achievement Data;
    UI_Achievement parent;

    public void Init(Achievement data, int value, UI_Achievement parentData)
    {
        parent = parentData;
        Data = data;
        CollectButton.onClick.RemoveAllListeners();
        switch (data.type)
        {
            case Achievement_Type.Hero:
                bool CanCollect = true;
                valueCount = value;

                if(Garvage.Count > 0)
                {
                    for (int i = 0; i < Garvage.Count; i++) Destroy(Garvage[i]);
                    Garvage.Clear();
                }

                if(Data_Manager.m_Data.Achievement_B[valueCount] == true)
                {
                    Collected.gameObject.SetActive(true);
                }

                for (int i = 0; i < data.Achievement_Characters.Count; i++)
                {
                    var character = data.Achievement_Characters[i].character;
                    var go = Instantiate(ContentPart, Content);
                    go.gameObject.SetActive(true);
                    Garvage.Add(go);

                    var image = go.transform.Find("Character_Icon").GetComponent<Image>();
                    image.sprite = Utils.Get_Atlas(character.m_CharacterName);
                    image.SetNativeSize();

                    var m_Text = go.transform.GetComponentInChildren<TextMeshProUGUI>();

                    bool IsOwned =
                     Base_Manager.Data.m_Data_Character.ContainsKey(character.m_CharacterName) &&
                     Base_Manager.Data.m_Data_Character[character.m_CharacterName].holder.Count > 0;

                    if (!IsOwned)
                    {
                        CanCollect = false;
                        m_Text.text = "0 / " + data.Achievement_Characters[i].Level;
                        m_Text.color = Color.red;
                        continue;
                    }

                    int level = Base_Manager.Data.m_Data_Character[character.m_CharacterName].holder.Level;
                    bool LevelCompleted = level + 1 >= data.Achievement_Characters[i].Level;

                    m_Text.text = string.Format("{0}/{1}", level + 1, data.Achievement_Characters[i].Level);
                    m_Text.color = LevelCompleted ? Color.green : Color.red;

                    if (!LevelCompleted)
                        CanCollect = false;

                }

                // 마지막 영웅 상태만 반영되던 잠금 표시를 전체 조건으로 정한다.
                Lock.gameObject.SetActive(!CanCollect);
                if(CanCollect)
                {
                    CollectButton.onClick.AddListener(() => GetCollect(valueCount));
                }
                break;
        }

        Title.text = Local_Manager.local_Data[data.Title].Get_Data();
        var temp = (int)Mathf.Sign(data.RewardValue) == 1 ? " +" : " ";
        RewardTitle.text = Local_Manager.local_Data[data.GetRewardStatus.ToString()].Get_Data() + temp + data.RewardValue + "%";
    }

    private void GetCollect(int valueCount)
    {
        if (Data_Manager.m_Data.Achievement_B[valueCount]) return;

        Data_Manager.m_Data.Achievement_B[valueCount] = true;
        Base_Manager.Quest.LoadAchievement_Data();
        Base_Manager.Player.UpdateStats();
        parent.Init();
    }

}
