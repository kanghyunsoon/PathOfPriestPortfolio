using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
   private int m_Count;
   private float m_SpawnTime;

    public static List<Monster> m_Monsters = new List<Monster>();
    public static List<Player> m_Players = new List<Player>();
    private static Monster BossPrefab;

    void Awake()
    {
        BossPrefab = Resources.Load<Monster>("Pool_OBJ/Boss");
    }

    Coroutine coroutine;

    public GameObject[] Maps;

    bool m_IsBossSpawned = false;

    private void Start()
    {
        Stage_Manager.m_ReadyEvent += OnReady;
        Stage_Manager.m_PlayEvent += OnPlay;
        Stage_Manager.m_BossEvent += OnBoss;
        Stage_Manager.m_DungeonEvent += OnDungeon;
    }

    public void OnReady()
    {
        m_IsBossSpawned = false;
        Initialize();
        for (int i =0; i<Maps.Length; i++) Maps[i].SetActive(false);

        m_Count = int.Parse(CSV_Importer.Spawn_Design[Data_Manager.m_Data.Stage]["Spawn_Count"].ToString());
        m_SpawnTime = float.Parse(CSV_Importer.Spawn_Design[Data_Manager.m_Data.Stage]["Spawn_Timer"].ToString());
    }

    public void OnPlay()
    {
        if (Stage_Manager.isDungeon) return;
        coroutine = StartCoroutine(SpawnCoroutine(m_Count, m_SpawnTime));
    }

    public void OnBoss()
    {
        m_IsBossSpawned = true;
        Initialize();
        StartCoroutine(BossSetCoroutine());
    }

    public void OnDungeon(int value)
    {
        Maps[value].SetActive(true);
        Initialize();
        if(value == 0)
        {
            coroutine = StartCoroutine(SpawnCoroutine(30, -1, Stage_Manager.DungeonLevel * 5));
        }
        else if(value == 1)
        {
            StartCoroutine(BossSetCoroutine());
        }


    }

    private void Initialize()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        for (int i = 0; i < m_Monsters.Count; i++)
        {
            if(m_Monsters[i].isDead == false)
            {
                m_Monsters[i].isDead = true;

                if (!m_Monsters[i].isBoss)
                {
                    Base_Manager.Pool.m_pool_Dictionary[m_Monsters[i].poolKey].Return(m_Monsters[i].gameObject);
                }
                else
                {
                    Destroy(m_Monsters[i].gameObject);
                }
            }
        }
        m_Monsters.Clear();
    }

    IEnumerator BossSetCoroutine()
    {
        // 현재 보스 스테이지가 아닐 경우 보스 소환 막기
        if (Stage_Manager.m_State != Stage_State.Boss && Stage_Manager.m_State != Stage_State.Dungeon)
            yield break;


        yield return new WaitForSeconds(2.0f);
        Player mainCharacter = FindActivePlayer();

        Monster monster = null;

        if(!Stage_Manager.isDungeon)
        {
            monster = Instantiate(BossPrefab, mainCharacter.transform.position, Quaternion.Euler(0, 180, 0));
            monster.Init();
        }
        else
        {

            monster = Instantiate(Resources.Load<Monster>("Pool_OBJ/Gold"), mainCharacter.transform.position, Quaternion.Euler(0, 180, 0));

            monster.Init((Stage_Manager.DungeonLevel) * 5);
        }


        Vector3 pos = monster.transform.position;

        for (int i = 0;  i< m_Players.Count; i++)
        {
            if(Vector3.Distance(m_Players[i].transform.position, pos) <= 3.0f)
            {
                m_Players[i].transform.LookAt(monster.transform.position);
                m_Players[i].KnockBack();
            }
        }
        yield return new WaitForSeconds(1.5f);

        m_Monsters.Add(monster);

        Stage_Manager.State_Change(Stage_State.BossPlay);


    }

    IEnumerator SpawnCoroutine(int count, float spawnTime, int stage = 0)
    {
        if (m_IsBossSpawned || Stage_Manager.m_State == Stage_State.BossPlay)
            yield break;


        if (!m_IsBossSpawned)
        {
            Vector3 pos;
            int m_value = count - m_Monsters.Count;

            Player mainCharacter = FindActivePlayer();

            if (mainCharacter == null)
            {
                yield break; // 중단하여 NullReferenceException 방지
            }

            for (int i = 0; i < m_value; i++)
            {
                pos = mainCharacter.transform.position + Random.insideUnitSphere * 5.0f;
                pos.y = 0.0f;

                while (Vector3.Distance(pos, mainCharacter.transform.position) <= 3.0f)
                {
                    pos = mainCharacter.transform.position + Random.insideUnitSphere * 5.0f;
                    pos.y = 0.0f;
                }

                // GC - Garvage Collerctor

                // Instantiate(생성자)
                // Destroy(파괴자)
                string MonsterName = "";
                float RandomValue = Random.Range(0.0f, 100.0f);

                if(RandomValue < 80)
                {
                    MonsterName = "Monster";
                }
                else if (RandomValue < 99)
                {
                    MonsterName = "Monster_Longdistance";
                }
                else
                {
                    MonsterName = "Monster_Elete";
                }



                var goObj = Base_Manager.Pool.Pooling_OBJ(MonsterName).Get((value) =>
                {
                    if (value == null) return;

                    Monster monster = value.GetComponent<Monster>();
                    if (monster == null) return;


                    monster.poolKey = MonsterName;
                    value.GetComponent<Monster>().Init(stage);
                    value.transform.position = pos;
                    value.transform.LookAt(mainCharacter.transform.position);

                    m_Monsters.Add(value.GetComponent<Monster>());
                });

            }

            yield return new WaitForSeconds(spawnTime);

            if(!m_IsBossSpawned)
            {
                if (spawnTime > 0.0f )
                {
                    coroutine = StartCoroutine(SpawnCoroutine(count, spawnTime));
                }

            }
        }
    }
    private Player FindActivePlayer()
    {
        // 메인 캐릭터가 존재하면 그대로 반환
        Player mainCharacter = m_Players.Find(p => p.MainCharacter);
        if (mainCharacter != null) return mainCharacter;

        // 메인 캐릭터가 없으면 살아있는 첫 번째 플레이어 반환
        if (m_Players.Count > 0) return m_Players[0];

        return null; // 모든 캐릭터가 삭제되었으면 null 반환
    }

}
