using UnityEngine;


[CreateAssetMenu(fileName = "Smelt_Data", menuName = "Smelt Data/Smelt")]

public class Smelt_Scriptable : ScriptableObject
{

    // 1~5개의 능력치가 랜덤하게 나오는 확률
    [Header("## Appear")]
    public float[] Count_Value;

    [Space(20f)]
    [Header("## Percentage")]
    public Percentage[] ATK_percentage; // ATK 담당
    public Percentage[] HP_percentage; // ATK 담당
    public Percentage[] Money_percentage;
    public Percentage[] Item_percentage;
    public Percentage[] Skill_percentage;
    public Percentage[] ATK_Speed_percentage;
    public Percentage[] Critical_P_percentage;
    public Percentage[] Critical_D_percentage;

}
