using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Scriptable", menuName ="Object/Character", order = int.MaxValue)]

public class Character_Scriptable : ScriptableObject
{
    public string m_CharacterName;
    public float m_Attack_Range;
    public float m_Attack_Speed;
    public Rarity m_Rarity;
    public int MaxMP;
    public string Description;
    public string Skill_Title;
    public string Skill_Name;
    public string Skill_Description;
    public string Skill_Image;
    public bool Main_Character;
}
