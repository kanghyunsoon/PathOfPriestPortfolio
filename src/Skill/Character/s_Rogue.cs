using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class s_Rogue : Skill_Base
{
    public ParticleSystem Expolosion_Particle;
    private Coroutine skillCoroutine;

    public override void Set_Skill()
    {
        m_Player.AnimatorChange("IsSkill");
        m_Player.isUsingSkill = true;
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null; // 중복 실행 방지
        }

        //Base_Manager.Sound.Play(Sound.Effect, "Babarian");
        SkillParticle.SetActive(true);
        skillCoroutine = StartCoroutine(Set_Skill_Coroutine());
        base.Set_Skill();
    }

    public override void ReturnSkill()
    {
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        SkillParticle.SetActive(false);
        m_Player.isUsingSkill = false;
        base.ReturnSkill();
    }

    IEnumerator Set_Skill_Coroutine()
    {
        if (this == null || SkillParticle == null) yield break;
        int attackCount = 0;
        List<Monster> attackedMonsters = new List<Monster>();

        while (attackCount < 6)
        {
            if (m_Player == null || m_Player.HP <= 0 || !gameObject.activeInHierarchy )
            {
                ReturnSkill();
                yield break;
            }

            Monster targetMonster = GetNearestUntargetedMonster(attackedMonsters);

            if (targetMonster != null)
            {
                attackedMonsters.Add(targetMonster);

                // 순간이동 및 방향 전환
                Vector3 backPosition = targetMonster.transform.position - targetMonster.transform.forward * 0.5f;
                transform.position = backPosition;
                transform.LookAt(targetMonster.transform);

                // 이펙트 재생 및 데미지
                Expolosion_Particle.Play();
                targetMonster.GetDamage(SkillDamage(150.0f));

                attackCount++;
            }
            else
            {
                ReturnSkill();
                break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }


    Monster GetNearestUntargetedMonster(List<Monster> alreadyAttacked)
    {
        Monster nearest = null;
        float minDistance = float.MaxValue;

        foreach (var monster in monsters)
        {
            if (monster == null || alreadyAttacked.Contains(monster)) continue;

            float distance = Vector3.Distance(transform.position, monster.transform.position);
            if (distance <= 5.0f && distance < minDistance)
            {
                minDistance = distance;
                nearest = monster;
            }
        }

        return nearest;
    }
}
