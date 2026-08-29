using System.Collections;
using UnityEngine;

public class Monster : Character
{
    public float m_speed;

    bool isSpawn = false;

    public double  R_Attack_Range;
    public bool isBoss = false;
    public bool isDungeon = false;
    public bool isLongDistance = false;
    public bool isElete = false;
    double Max_HP;


    public string poolKey;

    protected override void Start()
    {
        base.Start();
        Stage_Manager.m_DeadEvent += OnDead;
    }

    double GetBossScale(int stage)
    {
        if (stage < 100) return 10;
        else if (stage < 300) return 10 + (stage - 100) * 0.2; // 100~300: 10 → 50
        else return 50 + (stage - 300) * 0.25; // 300~500: 50 → 100
    }

    private void GetStatMultipliers(out float atkMultiplier, out float hpMultiplier, out float atkspeedMultiplier)
    {
        atkMultiplier = 1.0f;
        hpMultiplier = 1.0f;
        atkspeedMultiplier = 1.0f;

        if (isLongDistance)
        {
            atkMultiplier *= 0.8f;
            hpMultiplier *= 0.5f;
            atkspeedMultiplier *= 0.7f;
        }

        if (isElete)
        {
            atkMultiplier *= 1.5f;
            hpMultiplier *= 2.0f;
        }
    }


    public void Init(int value = 0)
    {
        isDead = false;
        isSpawn = false;

        float NightSet = Base_Manager.isNight ? 1.5f : 1.0f;
        int stageValue = isDungeon ? value : Data_Manager.m_Data.Stage;
        double scale = isBoss ? GetBossScale(stageValue) : 1.0;

        float atkMultiplier, hpMultiplier, atkspeedMultiplier;
        GetStatMultipliers(out atkMultiplier, out hpMultiplier, out atkspeedMultiplier);

        ATK = isBoss ? (Utils.Data.stageData.ATK(stageValue) * scale) * NightSet : Utils.Data.stageData.ATK(value) * NightSet * atkMultiplier;
        HP  = isBoss ? (Utils.Data.stageData.HP(stageValue)  * scale) * NightSet : Utils.Data.stageData.HP(value) * NightSet * hpMultiplier;

        ATK_Speed = Base_Manager.isNight ? 0.7f * atkspeedMultiplier : 0.5f * atkspeedMultiplier;
        Max_HP = HP;
        if(!isBoss) transform.localScale = isElete ? new Vector3(0.6f, 0.6f, 0.6f) : new Vector3(0.4f, 0.4f, 0.4f);

        attack_Range = (float)R_Attack_Range;
        target_Range = Mathf.Infinity;

        if(isBoss && !isDungeon)
        {
            StartCoroutine(SkillCoroutine());
        }

        StartCoroutine(Spawn_Start());
    }

    IEnumerator SkillCoroutine()
    {
        yield return new WaitForSeconds(3.0f);
        GetComponent<Skill_Base>().Set_Skill();

        StartCoroutine(SkillCoroutine());
    }

    public void OnDead()
    {
        if (this == null || gameObject == null) return;

        StopAllCoroutines();
        if (isDungeon) return;
        AnimatorChange("IsIdle");
    }

    private void Update()
    {
        if (!isSpawn || isDungeon) return;

        if (Stage_Manager.m_State == Stage_State.Play || Stage_Manager.m_State == Stage_State.BossPlay)
        {
            FindClosetTarget<Player>(Spawner.m_Players.ToArray());

            if (m_Target == null) return;

            float targetDistance = Vector3.Distance(transform.position, m_Target.position);

            if (targetDistance > attack_Range && !IsAttack)
            {
                AnimatorChange("IsMove");
                transform.position = Vector3.MoveTowards(transform.position, m_Target.position, Time.deltaTime);
            }
            else if (targetDistance <= attack_Range && !IsAttack)
            {
                IsAttack = true;
                AnimatorChange("IsAttack");
                Invoke("InitAttack", 1.0f);
            }

            transform.LookAt(m_Target.position);
        }
    }



    IEnumerator Spawn_Start()
    {
        float current = 0.0f;
        float percent = 0.0f;
        float start = 0.0f;
        float end = transform.localScale.x;

        while(percent < 1)
        {
            current += Time.deltaTime;
            percent = current / 0.2f;
            float LerpPos = Mathf.Lerp(start, end, percent);
            transform.localScale = new Vector3(LerpPos, LerpPos, LerpPos);
            yield return null;
        }
        yield return new WaitForSeconds(0.3f);
        isSpawn = true;
    }

    public override void GetDamage(double dmg)
    {
        if (isDead) return;

        bool critical = Critical(ref dmg);

        Base_Manager.Sound.Play(Sound.Effect, "Hit");

        Base_Manager.Pool.Pooling_OBJ("HIT_TEXT").Get((value) =>
        {
            value.GetComponent<HIT_TEXT>().Init(transform.position, dmg, Color.white, false,  critical);
        });

        HP -= dmg;

        if(isBoss)
        {
            Main_UI.instance.Boss_Slider_Count(HP, Max_HP);
        }

        if(HP<=0)
        {
            isDead = true;
            Delegate_Holder.Monster_Dead(this);
            Dead_Event();
        }
    }

    private void Dead_Event()
    {
        if(Main_Quest.GetEnemy)
        {
            Main_Quest.monster_Index++;
        }
        Data_Manager.m_Data.MonsterKill++;

        if (Stage_Manager.isDungeon)
        {
            Stage_Manager.Dungeon_Count--;
            Main_UI.instance.Dungeon_Monster_Slider_Count();
            if(isBoss)
            {
                Stage_Manager.State_Change(Stage_State.DungeonClear,Stage_Manager.DungeonType);
            }
        }
        else
        {
            if (!isBoss)
            {
                if (!Stage_Manager.isDead)
                {
                    Stage_Manager.Count++;
                    Main_UI.instance.Monster_Slider_Count();
                }
                // 추가
                if (Stage_Manager.Count >= Stage_Manager.MaxCount && Stage_Manager.m_State != Stage_State.Boss)
                {
                    Stage_Manager.State_Change(Stage_State.Boss);
                }
            }
            else
            {
                Stage_Manager.State_Change(Stage_State.Clear);
            }
        }
        Spawner.m_Monsters.Remove(this);


        Base_Manager.Pool.Pooling_OBJ("Smoke").Get((value) =>
        {
            value.transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
            Base_Manager.instance.Return_Pool(value.GetComponent<ParticleSystem>().main.duration, value, "Smoke");
        });

        Base_Manager.Pool.Pooling_OBJ("COIN_PARENT").Get((value) =>
        {
            value.GetComponent<COIN_PARENT>().Init(transform.position, false);
        });

        var Items = Base_Manager.Item.GetDropSet();

        for (int i = 0; i < Items.Count; i++)
        {
            float dropChance = Base_Manager.Player.ItemDropPercentage(); // 드랍률 계산
            float roll = Random.Range(0f, 100f); // 0~100 사이 주사위

            if (roll <= dropChance)
            {
                Base_Manager.Pool.Pooling_OBJ("Item_Obj").Get((value) =>
                {
                    value.GetComponent<Item_OBJ>().Init(transform.position, Items[i]);
                });
            }
        }

        if (!isBoss)
        {
            Base_Manager.Pool.m_pool_Dictionary[poolKey].Return(this.gameObject);
        }
        else
        {
            StopAllCoroutines();
            Destroy(this.gameObject);
        }
    }

    protected bool Critical(ref double dmg)
    {
        float RandomValue = Random.Range(0.0f, 100.0f);
        if (RandomValue <= Base_Manager.Player.CriticalPercentage())
        {
            dmg *= Base_Manager.Player.CriticalDamage() / 100;
            return true;
        }
        else return false;
    }

    private void OnDestroy()
    {
        // 혹시라도 구독이 남아있으면 해제
        if (!isBoss)
        {
            Stage_Manager.m_DeadEvent -= OnDead;
        }
    }

}
