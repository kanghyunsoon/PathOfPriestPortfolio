using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character_Spawner : MonoBehaviour
{
    public Transform[] SpawnTransform = new Transform[6];
    public static Player[] players = new Player[6];

    private void Awake()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            SpawnTransform[i] = transform.GetChild(i);
        }

        Stage_Manager.m_ReadyEvent += OnReady;
    }

    private void OnReady()
    {
        for(int  i =0; i < Base_Manager.Data.m_Set_Character.Length; i++)
        {
            var data = Base_Manager.Data.m_Set_Character[i];
            if (data != null)
            {
                if (players[i] != null)
                {
                    if (players[i].CH_Data != data.Data)
                    {
                        Destroy(players[i].gameObject);
                        MakePlayer(data, i);
                    }
                }
                else
                {
                    MakePlayer(data, i);
                }
            }
            else //캐릭터가 없는 경우 기존 캐릭터 제거
            {
                if (players[i] != null)
                {
                    if (players[i].gameObject != null)
                    {
                        Destroy(players[i].gameObject); // 삭제 전 체크
                    }
                    players[i] = null; // 참조 정리
                }
            }

        }
    }

    private void MakePlayer(Character_Holder data, int value)
    {
        string temp = data.Data.m_CharacterName;
        var go = Instantiate(Resources.Load<GameObject>("Character/" + temp));
        players[value] = go.GetComponent<Player>();
        go.transform.position = SpawnTransform[value].position;
        go.transform.LookAt(Vector3.zero);
    }
}
