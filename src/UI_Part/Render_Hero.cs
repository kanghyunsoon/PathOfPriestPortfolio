using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Render_Hero : MonoBehaviour
{
    public Transform[] circles;
    public Transform Pivot;
    public GameObject[] particles;

    public List<GameObject> CharacterOBJ = new List<GameObject>();
    private List<Character_Scriptable> spawnedCharacterData = new List<Character_Scriptable>(); // 이미 소환된 캐릭터 데이터

    public void GetParticle(bool m_B)
    {
        for(int i = 0; i<particles.Length; i++)
        {
            particles[i].SetActive(m_B);
        }
    }

    public void InitHero()
    {
        // 재귀적으로 레이어를 설정하는 로컬 함수 정의
        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;

            // 현재 오브젝트의 레이어 변경
            obj.layer = newLayer;

            // 모든 자식에 대해 재귀적으로 레이어 변경
            foreach (Transform child in obj.transform)
            {
                if (child != null)
                {
                    SetLayerRecursively(child.gameObject, newLayer);
                }
            }
        }

        for (int i = 0; i < CharacterOBJ.Count; i++)
        {
            Destroy(CharacterOBJ[i]);
        }
        CharacterOBJ.Clear();

        spawnedCharacterData.Clear();

        for (int i =0; i<Base_Manager.Data.m_Set_Character.Length; i++)
        {
            if(Base_Manager.Data.m_Set_Character[i] != null)
            {
                string temp = Base_Manager.Data.m_Set_Character[i].Data.m_CharacterName;

                var go = Instantiate(Resources.Load<GameObject>("Character/" + temp));
                CharacterOBJ.Add(go);
                // "Render_Layer"로 모든 자식의 레이어 설정
                int renderLayer = LayerMask.NameToLayer("Render_Layer");
                SetLayerRecursively(go, renderLayer);

                go.transform.rotation = Quaternion.Euler(0,0,0);
                go.transform.parent = transform;
                go.GetComponent<Player>().enabled = false;
                go.transform.position = circles[i].position;
                go.transform.LookAt(Pivot.position);


                Character_Scriptable characterScriptable = Base_Manager.Data.m_Set_Character[i].Data;
                spawnedCharacterData.Add(characterScriptable);
            }
        }
    }


}
