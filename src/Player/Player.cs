using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : Character
{
    public Character_Scriptable CH_Data;
    public ParticleSystem Provocation_Effect;
    public GameObject[] TrailObject;
    public string CH_Name;
    public int MP;
    Vector3 startPos;
    Quaternion rot;
    public bool MainCharacter = false;

    public float moveSpeed = 1f;
    private Vector3 targetPosition;
    private bool isDragging = false;
    // 메인 캐릭터 위치를 저장할 변수 추가
    private Vector3 mainCharacterPosition;
    private float formationRadius = 1f; // 방진의 반지름 크기
    private int formationCount = 6; // 최대 보조 캐릭터 수
    public bool closeRange = false;
    public bool ReadyCharacter;

    protected override void Start()
    {
        base.Start();
        Data_Set(Resources.Load<Character_Scriptable>("Scriptable/Character/" + CH_Name));

        if (!Spawner.m_Players.Contains(this))
            Spawner.m_Players.Add(this);

        Stage_Manager.m_ReadyEvent += OnReady;
        Stage_Manager.m_BossEvent += OnBoss;
        Stage_Manager.m_ClearEvent += OnClear;
        Stage_Manager.m_DeadEvent += OnDead;
        Stage_Manager.m_DungeonEvent += OnDungeon;
        Stage_Manager.m_DungeonClearEvent += OnDungeonClear;

        startPos = transform.position;
        rot = transform.rotation;
        isDragging = false;

        if (GetComponent<Speech_Character>()) GetComponent<Speech_Character>().Init(ReadyCharacter);

    }

    private void Data_Set(Character_Scriptable data)
    {
        CH_Data = data;
        Bullet_Name = CH_Data.m_CharacterName;
        attack_Range = data.m_Attack_Range;
        ATK_Speed = data.m_Attack_Speed;
        Set_ATKHP();
    }

    public void Set_ATKHP()
    {
        bool isDebuff = false;

        if (Base_Manager.Data.m_Data_Character.ContainsKey(CH_Data.name))
        {
            isDebuff = Base_Manager.Data.m_Data_Character[CH_Data.name].isDebuff;
        }

        double atk = Base_Manager.Player.Get_ATK(CH_Data.m_Rarity, Base_Manager.Data.m_Data_Character[CH_Data.name]);
        double hp = Base_Manager.Player.Get_HP(CH_Data.m_Rarity, Base_Manager.Data.m_Data_Character[CH_Data.name]);
        float aTK_Speed = isDebuff ? Base_Manager.Player.ATKSpeed() * 0.8f : Base_Manager.Player.ATKSpeed();


        if (CH_Data.name == "Rogue")
            ATK = atk / 2;
        else ATK = atk;

        HP = hp;
        ATK_Speed = aTK_Speed;

    }

    private void OnReady()
    {
        if (this == null) return;

        Initialize();
        if (!Spawner.m_Players.Contains(this))
            Spawner.m_Players.Add(this);
        isDragging = false;
    }

    private void OnDungeon(int value)
    {
        Initialize();
        Stage_Manager.isDungeon = true;
        if (!Spawner.m_Players.Contains(this))
            Spawner.m_Players.Add(this);
    }

    // 편성에서 빠진 영웅은 Character_Spawner가 파괴한다.
    // 정적 이벤트와 목록에 남으면 파괴된 영웅을 몬스터가 계속 찾으면서 예외가 난다.
    private void OnDestroy()
    {
        Stage_Manager.m_ReadyEvent -= OnReady;
        Stage_Manager.m_BossEvent -= OnBoss;
        Stage_Manager.m_ClearEvent -= OnClear;
        Stage_Manager.m_DeadEvent -= OnDead;
        Stage_Manager.m_DungeonEvent -= OnDungeon;
        Stage_Manager.m_DungeonClearEvent -= OnDungeonClear;

        Spawner.m_Players.Remove(this);
    }

    private void Initialize()
    {
        if (this == null || transform == null) return;

        AnimatorChange("IsIdle");
        isDead = false;
        Set_ATKHP();

        transform.position = startPos;
        transform.rotation = rot;
    }

    private void OnBoss()
    {
        AnimatorChange("IsIdle");
        if (Provocation_Effect != null)
        {
            Provocation_Effect.Play();
        }
    }

    private void OnClear()
    {
        AnimatorChange("IsClear");
    }

    private void OnDungeonClear(int value)
    {
        AnimatorChange("IsClear");
    }

    private void OnDead()
    {
        if (Spawner.m_Players.Contains(this))
        {
            Spawner.m_Players.Remove(this); // 리스트에서 제거
        }
    }

    public void Get_MP(int mp)
    {
        if (isGetSkill) return;
        if (MainCharacter) return;

        MP += mp;

        if (MP >= CH_Data.MaxMP)
        {
            MP = 0;

            if(GetComponent<Skill_Base>() != null)
            {
                GetComponent<Skill_Base>().Set_Skill();

            }
            isGetSkill = true;
        }
        Main_UI.instance.Character_StateCheck(this);
    }

    private void UpdateFormation()
    {
        if (this == null || transform == null || this.isDead == true) return;

        Player mainCharacter = Spawner.m_Players.Find(p => p.MainCharacter);
        if (mainCharacter == null) return;

        int count = 0;
        float angleStep = 360f / formationCount;

        foreach (var player in Spawner.m_Players)
        {
            if (player == null || player.transform == null) continue; // 삭제된 오브젝트는 건너뛰기
            if (player == mainCharacter) continue; // 메인 캐릭터는 제외

            float distanceToMain = Vector3.Distance(player.transform.position, mainCharacter.transform.position);

            float angle = count * angleStep * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * formationRadius;
            Vector3 formationTarget = mainCharacter.transform.position + offset;

            if (distanceToMain > formationRadius * (!closeRange ? 2.0f : 3.0f)) // 일정 거리 이상 벗어나면 포메이션 복귀
            {
                player.transform.position = Vector3.MoveTowards(player.transform.position, formationTarget, moveSpeed * Time.deltaTime);
                player.transform.LookAt(mainCharacter.transform.position);

                player.AnimatorChange("IsMove");
            }
            else
            {
                // 랜덤 이동 추가 (원형 영역 내에서 부드럽게 퍼지도록)
                float noiseX = Mathf.PerlinNoise(Time.time * 0.5f, count * 0.1f) - 0.5f;
                float noiseZ = Mathf.PerlinNoise(count * 0.1f, Time.time * 0.5f) - 0.5f;
                Vector3 randomOffset = new Vector3(noiseX, 0, noiseZ) * 0.2f; // 너무 많이 흔들리지 않도록 조절

                player.transform.position = Vector3.Lerp(player.transform.position, player.transform.position + randomOffset, Time.deltaTime * moveSpeed);

                player.AnimatorChange("IsIdle");
            }

            count++;
        }
    }

    private void Update()
    {
        if (isDead || isUsingSkill) return;

        if (Stage_Manager.m_State == Stage_State.Play || Stage_Manager.m_State == Stage_State.BossPlay)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                    isDragging = true;
                else
                    isDragging = false;
            }
            else if (Input.GetMouseButtonUp(0)) isDragging = false;

            if (isDragging && MainCharacter)
            {
                if (Camera.main == null) return;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    targetPosition = hit.point;
                }

                transform.LookAt(targetPosition);
                AnimatorChange("IsMove");

                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

                mainCharacterPosition = transform.position;
                UpdateFormation();
            }

            FindClosetTarget(Spawner.m_Monsters.ToArray());

            if (m_Target == null)
            {
                float targetPos = Vector3.Distance(transform.position, mainCharacterPosition);
                if (targetPos > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, mainCharacterPosition, Time.deltaTime);
                    transform.LookAt(mainCharacterPosition);
                    AnimatorChange("IsMove");
                }
                else
                {
                    transform.rotation = rot;
                    AnimatorChange("IsIdle");
                }
            }
            else
            {
                if (!m_Target.gameObject.activeSelf || m_Target.GetComponent<Character>().isDead)
                {
                    m_Target = null;
                    FindClosetTarget(Spawner.m_Monsters.ToArray());
                    if (m_Target == null) return;
                }

                float targetDistance = Vector3.Distance(transform.position, m_Target.position);

                if (targetDistance <= target_Range && targetDistance > attack_Range && IsAttack == false) // 현재 타겟이 추적범위안엔 있지만 어택범위 안엔 X
                {
                    if(!MainCharacter)
                    {
                        AnimatorChange("IsMove");
                        transform.position = Vector3.MoveTowards(transform.position, m_Target.position, Time.deltaTime);
                        transform.LookAt(m_Target.position);
                    }
                    else if(MainCharacter && !isDragging)
                    {
                        AnimatorChange("IsMove");
                        transform.position = Vector3.MoveTowards(transform.position, m_Target.position, Time.deltaTime);
                        transform.LookAt(m_Target.position);
                    }


                }
                else if (targetDistance <= attack_Range && IsAttack == false)
                {
                    IsAttack = true;
                    AnimatorChange("IsAttack");
                    Get_MP(5);
                    Invoke("InitAttack", 1.0f / ATK_Speed); // 함수를 몇초 뒤에 실행시켜라
                }
            }
        }
    }

    public void KnockBack()
    {
        StartCoroutine(Knockback_Coroutine(3.0f, 0.3f));
    }

    IEnumerator Knockback_Coroutine(float power, float duration)
    {
        Player mainCharacter = Spawner.m_Players.Find(p => p.MainCharacter);

        if (mainCharacter == null)
        {
            if (Spawner.m_Players.Count > 0)
            {
                mainCharacter = Spawner.m_Players[0];
            }
        }

            float t = duration;
        Vector3 force = this.transform.forward * -power;

        force.y = 0.0f;

        while(t > 0.0f)
        {
            t -= Time.deltaTime;

            if(Vector3.Distance(mainCharacter.transform.position, transform.position) < 2.0f)
            {
                transform.position += force * Time.deltaTime;
            }
            yield return null;
        }
    }

    public override void GetDamage(double dmg)
    {
        base.GetDamage(dmg);

        if (Stage_Manager.isDead) return;

        Delegate_Holder.Player_Hit(this);


        Get_MP(3);

        var goObj = Base_Manager.Pool.Pooling_OBJ("HIT_TEXT").Get((value) =>
         {
             value.GetComponent<HIT_TEXT>().Init(transform.position,dmg, Color.red, true);
         });

        HP -= dmg;

        if (!MainCharacter) Main_UI.instance.Character_StateCheck(this);

        if (HP <= 0)
        {

            DeadEvent();
        }
    }

    private void DeadEvent()
    {
        if (isDead)
        {
            Debug.LogWarning($"[{gameObject.name}] 이미 죽은 상태에서 DeadEvent 재호출됨");
            return;
        }
        isDead = true;

        if (Stage_Manager.isDungeon)
        {
            Stage_Manager.State_Change(Stage_State.DungeonFail);
            AnimatorChange("IsDead");
            m_Target = null;
            return;
        }

        if (Spawner.m_Players.Contains(this))
        {
            Spawner.m_Players.Remove(this);
        }

        AnimatorChange("IsDead");
        m_Target = null;

        if (Spawner.m_Players.Count <= 0)
        {
            if (!Stage_Manager.isDead)
            {
                Stage_Manager.State_Change(Stage_State.Dead);
                Stage_Manager.isDead = true; // 혹시 모를 대비
            }
        }

    }

    protected override void Attack()
    {

        base.Attack();
        if(m_Target != null)
        {
            for(int i = 0; i < TrailObject.Length; i++)
            {
                TrailObject[i].SetActive(true);
            }

            Delegate_Holder.Player_Attack(this, m_Target.GetComponent<Monster>());
            Invoke("TrailDisable", 1.0f);
        }
    }



    private void TrailDisable()
    {
        for (int i = 0; i < TrailObject.Length; i++)
        {
            TrailObject[i].SetActive(false);
        }
    }


}
