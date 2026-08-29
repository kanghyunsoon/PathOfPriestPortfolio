using UnityEngine;
using System.Collections;
using System.Linq;
public class s_Berserker : Skill_Base
{
    private Coroutine skillCoroutine;
    public override void Set_Skill()
    {
        m_Player.AnimatorChange("IsSkill");
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null; // 중복 실행 방지
        }

        //Base_Manager.Sound.Play(Sound.Effect, "Babarian");
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

        // 전방 2m 이내 몬스터를 필터링하고 거리순으로 정렬
        var frontMonsters = monsters
            .Where(monster =>
            {
                if (monster == null) return false;

                Vector3 toTarget = (monster.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, monster.transform.position);

                return Vector3.Dot(transform.forward, toTarget) > 0.8f && distance <= 2.0f;
            })
            .OrderBy(monster => Vector3.Distance(transform.position, monster.transform.position))
            .ToList();

        // 순차적으로 데미지 부여 (0.15초 간격)
        foreach (var monster in frontMonsters)
        {
            if (monster == null) continue;

            monster.GetDamage(SkillDamage(200.0f));
            yield return new WaitForSeconds(0.15f); // 간격을 두고 차례대로 타격
        }

        yield return new WaitForSeconds(1.5f);  // 여유 시간
        ReturnSkill();
    }
}
