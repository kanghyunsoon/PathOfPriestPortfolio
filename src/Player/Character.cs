using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    Animator animator;

    public double HP;
    public double ATK;
    public float ATK_Speed;
    public bool isDead = false;

    protected float attack_Range = 3.0f; // 공격범위
    protected float target_Range = 5.0f; // 추격범위

    protected bool IsAttack = false;

    public bool isGetSkill = false;
    public bool SkillNoneAttack = false;

    [SerializeField] private Transform m_BulletTransform;

    public string Bullet_Name;

    protected Transform m_Target;

    public bool isUsingSkill = false;
    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected void InitAttack() => IsAttack = false;

    public void AnimatorChange(string temp)
    {
        if (this == null || animator == null) return;

        if (SkillNoneAttack)
        {
            if (isGetSkill) return;
        }

        animator.SetBool("IsIdle", false);
        animator.SetBool("IsMove", false);

        if (temp == "IsAttack" || temp == "IsClear" || temp == "IsDead" || temp == "IsSkill" || temp == "IsSad" )
        {

            if (temp == "IsAttack")
            {
                animator.speed = ATK_Speed;
            }
            animator.SetTrigger(temp);
            return;
        }

        animator.speed = 1.0f;
        animator.SetBool(temp, true);
    }



    // 이벤트로 실행
    protected virtual void Bullet()
    {
        if (m_Target == null) return;

        Base_Manager.Pool.Pooling_OBJ("Attack_Helper").Get((value) =>
        {
            value.transform.position = m_BulletTransform.position;
            value.GetComponent<Bullet>().Init(m_Target, ATK, Bullet_Name);
        });
    }

    protected virtual void Attack()
    {
        if (m_Target == null) return;

        Base_Manager.Pool.Pooling_OBJ("Attack_Helper").Get((value) =>
        {
            value.transform.position = m_Target.position;
            value.GetComponent<Bullet>().Attack_Init(m_Target, ATK);
        });
    }

    public virtual void GetDamage(double dmg) { }

    public virtual void Heal(double heal)
    {
        HP += heal;

        var goObj = Base_Manager.Pool.Pooling_OBJ("HIT_TEXT").Get((value) =>
        {
            value.GetComponent<HIT_TEXT>().Init(transform.position, heal,Color.green, true);
        });

    }

    protected void FindClosetTarget<T>(T[] targets) where T : Component
    {
        if (this == null || transform == null) return;

        var monsters = targets;
        Transform closetTarget = null;
        float maxDistance = target_Range; // 5.0f

        foreach(var monster in monsters)
        {
            if (monster != null)
            {
                float targetDistance = Vector3.Distance(transform.position, monster.transform.position);
                if (targetDistance < maxDistance)
                {
                    closetTarget = monster.transform;
                    maxDistance = targetDistance;
                }
            }
        }
        m_Target = closetTarget;

        if (m_Target != null && transform != null)
            transform.LookAt(m_Target.position);
    }
}
