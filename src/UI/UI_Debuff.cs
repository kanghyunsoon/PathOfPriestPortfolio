using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UI_Debuff : UI_Base
{
    [SerializeField] private Transform Content;
    [SerializeField] private UI_Inventory_Part part;
    [SerializeField] private Image CharacterIcon, RarityIcon;
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private Button GiftButton;

    public static Character_Scriptable selectedDebuffCharacter;

    public override bool Init()
    {
        if (selectedDebuffCharacter != null)
        {
            Initialized(selectedDebuffCharacter);
        }

        return base.Init();
    }

    public void Initialized(Character_Scriptable data)
    {
        Render_Manager.instance.DEBUFF.ShowDebuffCharacter(data);

        NameText.text = Local_Manager.local_Data[data.m_CharacterName].Get_Data();
        CharacterIcon.sprite = Utils.Get_Atlas(data.name);
        CharacterIcon.SetNativeSize();
        RarityIcon.sprite = Utils.Get_Atlas(data.m_Rarity.ToString());

        GiftButton.onClick.RemoveAllListeners();
        GiftButton.onClick.AddListener(OnDebuffItemUsed);

        if (PlayerPrefs.HasKey($"DebuffItem_0_{data.name}"))
            SpawnSavedItems(data.name);
        else
            SpawnRandomItems(data.name);
    }

    private void SpawnRandomItems(string characterName)
    {
        var ownedItems = Base_Manager.Data.GetOwnedItems();
        var candidates = ownedItems
            .Where(item => item.Value.Data.rarity == Rarity.Common ||
                           item.Value.Data.rarity == Rarity.UnCommon ||
                           item.Value.Data.rarity == Rarity.Rare)
            .ToList();

        System.Random rand = new System.Random();
        candidates = candidates.OrderByDescending(item =>
        {
            float weight = item.Value.Data.rarity switch
            {
                Rarity.Common => 1.0f,
                Rarity.UnCommon => 0.7f,
                Rarity.Rare => 0.4f,
                _ => 0.1f
            };
            return rand.NextDouble() * weight;
        }).ToList();

        int count = Random.Range(1, 4);
        PlayerPrefs.SetInt($"DebuffItemTotal_{characterName}", Mathf.Min(count, candidates.Count));

        for (int i = 0; i < Mathf.Min(count, candidates.Count); i++)
        {
            var itemData = candidates[i];
            var rarity = itemData.Value.Data.rarity;

            int amount = rarity switch
            {
                Rarity.Common => Random.Range(10, 51),
                Rarity.UnCommon => Random.Range(5, 31),
                Rarity.Rare => Random.Range(1, 15),
                _ => 1
            };

            string itemKey = $"DebuffItem_{i}_{characterName}";
            string countKey = $"DebuffItemCount_{i}_{characterName}";

            PlayerPrefs.SetString(itemKey, itemData.Key);
            PlayerPrefs.SetInt(countKey, amount);

            var clonedHolder = new Holder { Level = itemData.Value.holder.Level, Count = amount };
            var spawned = Instantiate(part, Content);
            spawned.Init(itemData.Key, clonedHolder);
        }

        PlayerPrefs.Save();
    }

    private void SpawnSavedItems(string characterName)
    {
        int total = PlayerPrefs.GetInt($"DebuffItemTotal_{characterName}", 0);
        for (int i = 0; i < total; i++)
        {
            string itemKey = PlayerPrefs.GetString($"DebuffItem_{i}_{characterName}", "");
            int count = PlayerPrefs.GetInt($"DebuffItemCount_{i}_{characterName}", 1);

            if (!string.IsNullOrEmpty(itemKey) && Base_Manager.Data.m_Data_Item.ContainsKey(itemKey))
            {
                var clonedHolder = new Holder { Level = 0, Count = count };
                var spawned = Instantiate(part, Content);
                spawned.Init(itemKey, clonedHolder);
            }
        }
    }

    public static void OnDebuffItemUsed()
    {
        if (selectedDebuffCharacter == null)
        {
            Debug.LogError("[디버프] 선택된 캐릭터가 없습니다.");
            return;
        }

        string charKey = selectedDebuffCharacter.name;
        int total = PlayerPrefs.GetInt($"DebuffItemTotal_{charKey}", 0);
        bool canUseAll = true;

        for (int i = 0; i < total; i++)
        {
            string itemName = PlayerPrefs.GetString($"DebuffItem_{i}_{charKey}", "");
            int requiredCount = PlayerPrefs.GetInt($"DebuffItemCount_{i}_{charKey}");

            if (string.IsNullOrEmpty(itemName) || !Base_Manager.Data.m_Data_Item.ContainsKey(itemName))
            {
                canUseAll = false;
                break;
            }

            var holder = Base_Manager.Data.m_Data_Item[itemName].holder;
            if (holder.Count < requiredCount)
            {
                canUseAll = false;
                break;
            }
        }

        if (!canUseAll)
        {
            Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["MoreItem"].Get_Data(), Color.red);
            return;
        }

        for (int i = 0; i < total; i++)
        {
            string itemName = PlayerPrefs.GetString($"DebuffItem_{i}_{charKey}", "");
            int count = PlayerPrefs.GetInt($"DebuffItemCount_{i}_{charKey}");
            Base_Manager.Data.m_Data_Item[itemName].holder.Count -= count;

            PlayerPrefs.DeleteKey($"DebuffItem_{i}_{charKey}");
            PlayerPrefs.DeleteKey($"DebuffItemCount_{i}_{charKey}");
        }

        PlayerPrefs.DeleteKey($"DebuffItemTotal_{charKey}");
        PlayerPrefs.Save();

        Base_Manager.Data.m_Data_Character[selectedDebuffCharacter.name].isDebuff = false;
        Base_Canvas.instance.Get_Toast().Initialize(Local_Manager.local_Data["RemoveDebuff"].Get_Data(), Color.white);
        Utils.CloseAllPopupUI();
    }

    public override void DisableOBJ()
    {
        selectedDebuffCharacter = null;
        base.DisableOBJ();
    }
}
