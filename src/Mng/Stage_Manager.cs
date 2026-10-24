using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Stage_Manager
{
    // State 패턴

    public static Stage_State m_State;
    public static int MaxCount = 3;
    public static int Count;
    public static int Dungeon_Count = 30;

    public static bool isDead = false;
    public static bool isDungeon = false;
    public static int DungeonType = 0;
    public static int DungeonLevel = 0;


    // 델리게이트 체인
    // 하나의 델리게이트가 여러함수를 참조할 수 있다

    public static OnReadyEvent m_ReadyEvent;
    public static OnPlayEvent  m_PlayEvent;
    public static OnBossEvent  m_BossEvent;
    public static OnBossPlayEvent m_BossPlayEvent;
    public static OnClearEvent m_ClearEvent;
    public static OnDeadEvent  m_DeadEvent;
    public static OnDungeonEvent m_DungeonEvent;
    public static OnDungeonClearEvent m_DungeonClearEvent;
    public static OnDungeonFailEvent m_DungeonFailEvent;

    public static void State_Change(Stage_State state, int value = 0, float inPlaytime = 1.2f)
    {
        m_State = state;
        switch(state)
        {
            case Stage_State.Ready:
                MaxCount = int.Parse(CSV_Importer.SpawnDesignRow(Data_Manager.m_Data.Stage)["MaxCount"].ToString());

                m_ReadyEvent?.Invoke();
                Base_Manager.instance.Coroutine_Action(() => State_Change(Stage_State.Play), inPlaytime);
                break;
            case Stage_State.Play :
                // Debug.Log("IsPlay");
                m_PlayEvent?.Invoke();
                break;
            case Stage_State.Boss :
                Count = 0;
               // Debug.Log("IsBoss");
                m_BossEvent?.Invoke();
                break;
            case Stage_State.BossPlay:
               // Debug.Log("IsBossPlay");
                m_BossPlayEvent?.Invoke();
                break;
            case Stage_State.Clear:
                Data_Manager.m_Data.Stage++;
               // Debug.Log("IsClear");
                m_ClearEvent?.Invoke();
                break;
            case Stage_State.Dead :
                isDead = true;
                m_DeadEvent?.Invoke();
                break;
            case Stage_State.Dungeon:
                isDungeon = true;
                DungeonType = value;
                Dungeon_Count = 30;
                m_DungeonEvent?.Invoke(value);
                Base_Manager.instance.Coroutine_Action(() => State_Change(Stage_State.Play), 1.2f);
                break;
            case Stage_State.DungeonClear:
                isDungeon = false;
                m_DungeonClearEvent?.Invoke(value);
                break;
            case Stage_State.DungeonFail:
                isDungeon = false;
                m_DungeonFailEvent?.Invoke();
                break;
        }
    }


}
