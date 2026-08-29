using UnityEngine;
using System.Collections;
using System.Linq;
public class s_Paladin : Skill_Base
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

        SkillParticle.SetActive(true);
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

        // 스킬 시전 위치 고정
        Vector3 skillOrigin = transform.position;

        float duration = 2.0f;
        float interval = 0.25f;
        int repeatCount = Mathf.FloorToInt(duration / interval);

        for (int i = 0; i < repeatCount; i++)
        {
            foreach (var monster in monsters)
            {
                if (monster == null) continue;

                float distance = Vector3.Distance(skillOrigin, monster.transform.position);
                if (distance <= 3.0f)
                {
                    monster.GetDamage(SkillDamage(70.0f)); // 70% 데미지
                }
            }

            yield return new WaitForSeconds(interval);
        }
        ReturnSkill();
    }
}
