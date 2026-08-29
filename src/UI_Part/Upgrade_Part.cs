using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Upgrade_Part : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Level,UpgradeLevel;
    [SerializeField] private Image Character_Icon;
    [SerializeField] private Animator anim;

    public void Init(int BeforeLevel, int level, Character_Holder character_holder = null, Item_Holder item_holder = null )
    {
        Level.text = BeforeLevel.ToString();
        UpgradeLevel.text = level.ToString();
        if(character_holder != null)
        {
            Character_Icon.sprite = Utils.Get_Atlas(character_holder.Data.name);
        }
        else
        {
            Character_Icon.sprite = Utils.Get_Atlas(item_holder.Data.name);
        }
        Character_Icon.SetNativeSize();
        anim.SetTrigger("Default");
    }
}
