using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(LevelDesign))]
public class LevelDesignEditor : Editor
{
    LevelDesign design = null;
    public override void OnInspectorGUI()
    {
        design = (LevelDesign)target;

        EditorGUILayout.LabelField("Level Design", EditorStyles.boldLabel);

        LevelData data = design.levelData;
        StageData s_Data = design.stageData;


        DrawGraph(data, s_Data);
        EditorGUILayout.Space();

        DrawDefaultInspector();
    }


    private void DrawGraph(LevelData data, StageData s_Data)
    {
        // Rect rect = GUILayoutUtility.GetRect(200, 100);
        // Handles.DrawSolidRectangleWithOutline(rect, Color.black, Color.white);
        //
        // Vector3[] curvePoint_ATK = GrapgDesign(rect, data.C_ATK);
        // Handles.color = Color.green;
        // Handles.DrawAAPolyLine(3, curvePoint_ATK); // 굵기, 수치
        //
        // Vector3[] curvePoint_HP = GrapgDesign(rect, data.C_HP);
        // Handles.color = Color.red;
        // Handles.DrawAAPolyLine(3, curvePoint_HP); // 굵기, 수치
        //
        // Vector3[] curvePoint_EXP = GrapgDesign(rect, data.C_EXP);
        // Handles.color = Color.blue;
        // Handles.DrawAAPolyLine(3, curvePoint_EXP); // 굵기, 수치
        //
        // Vector3[] curvePoint_MAXEXP = GrapgDesign(rect, data.C_MAXEXP);
        // Handles.color = Color.white;
        // Handles.DrawAAPolyLine(3, curvePoint_MAXEXP); // 굵기, 수치
        //
        // Vector3[] curvePoint_MONEY = GrapgDesign(rect, data.C_MONEY);
        // Handles.color = Color.yellow;
        // Handles.DrawAAPolyLine(3, curvePoint_MONEY); // 굵기, 수치

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("레벨 데이터", EditorStyles.boldLabel);
        GetColorGUI("ATK",     StringMethod.ToCurrencyString(Utils.CalculatedValue(data.B_ATK,    design.currentLevel, data.C_ATK)),    Color.green);
        GetColorGUI("HP",      StringMethod.ToCurrencyString(Utils.CalculatedValue(data.B_HP,     design.currentLevel, data.C_HP)),     Color.red);
        GetColorGUI("EXP",     StringMethod.ToCurrencyString(Utils.CalculatedValue(data.B_EXP,    design.currentLevel, data.C_EXP)),    Color.blue);
        GetColorGUI("MAX_EXP", StringMethod.ToCurrencyString(Utils.CalculatedValue(data.B_MAXEXP, design.currentLevel, data.C_MAXEXP)), Color.white);
        GetColorGUI("MONEY",   StringMethod.ToCurrencyString(Utils.CalculatedValue(data.B_MONEY,  design.currentLevel, data.C_MONEY)),  Color.yellow);
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("스테이지 데이터", EditorStyles.boldLabel);
        GetColorGUI("ATK",   StringMethod.ToCurrencyString(Utils.CalculatedValue(s_Data.B_ATK,   design.currentStage, s_Data.M_ATK)),   Color.green);
        GetColorGUI("HP",    StringMethod.ToCurrencyString(Utils.CalculatedValue(s_Data.B_HP,    design.currentStage, s_Data.M_HP)),    Color.red);
        GetColorGUI("MONEY", StringMethod.ToCurrencyString(Utils.CalculatedValue(s_Data.B_MONEY, design.currentStage, s_Data.M_MONEY)), Color.yellow);
    }

    void GetColorGUI(string baseTemp, string dataTemp, Color color)
    {
        GUIStyle colorLabel = new GUIStyle(EditorStyles.label);
        colorLabel.normal.textColor = color;

        EditorGUILayout.LabelField(baseTemp + " : " + dataTemp, colorLabel);
    }



  // private Vector3[] GrapgDesign(Rect rect, float data)
  // {
  //     Vector3[] curvePoint = new Vector3[100];
  //     for(int i =0; i<100; i++)
  //     {
  //         float t = i / 99.0f;
  //         float curveValue = Mathf.Pow(t, data);
  //         curvePoint[i] = new Vector3(
  //             rect.x + t * rect.width, // x
  //             rect.y + rect.height - curveValue * rect.height, // y
  //             0);
  //     }
  //     return curvePoint;
  // }
}
#endif
