using System.Linq;
using UnityEngine;
using System.Collections;

public class s_Elf_Archer : Skill_Base
{
    private Coroutine skillCoroutine;

    public override void Set_Skill()
    {
        m_Player.AnimatorChange("IsSkill");

        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null;
        }

        SkillParticle.SetActive(true);
        Camera_Manager.instance.CameraShake();
        skillCoroutine = StartCoroutine(Set_Skill_Coroutine());
        base.Set_Skill();
    }

    public override void ReturnSkill()
    {
        SkillParticle.SetActive(false);
        base.ReturnSkill();
    }

    IEnumerator Set_Skill_Coroutine()
    {
        if (this == null || SkillParticle == null) yield break;

        // 1. 가까운 몬스터 5마리 찾기
        var nearestMonsters = monsters
            .Where(monster => monster != null)
            .OrderBy(monster => Vector3.Distance(m_Player.transform.position, monster.transform.position))
            .Take(5)
            .ToList();

        foreach (var monster in nearestMonsters)
        {
            if (monster == null) continue;

            Vector3 direction = (monster.transform.position - m_Player.transform.position).normalized;

            // 불렛 생성 및 발사
            Base_Manager.Pool.Pooling_OBJ("Attack_Helper").Get((value) =>
            {
                value.transform.position = m_Player.transform.position + Vector3.up * 1.0f;
                value.transform.rotation = Quaternion.LookRotation(direction);

                Bullet bullet = value.GetComponent<Bullet>();
                bullet.Init(monster.transform, m_Player.ATK, m_Player.Bullet_Name); // 진짜 몬스터를 타겟으로
            });

            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(1.0f);
        ReturnSkill();
    }
}
