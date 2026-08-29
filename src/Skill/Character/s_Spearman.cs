using System.Linq;
using UnityEngine;
using System.Collections;

public class s_Spearman : Skill_Base
{
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
        Camera_Manager.instance.CameraShake();
        skillCoroutine = StartCoroutine(Set_Skill_Coroutine());
        base.Set_Skill();
    }

    public override void ReturnSkill()
    {
        SkillParticle.SetActive(false);
        m_Player.isUsingSkill = false;
        base.ReturnSkill();
    }

    IEnumerator Set_Skill_Coroutine()
    {
        if (this == null || SkillParticle == null) yield break;

        for (int i = 0; i < 6; i++)
        {
            foreach (var monster in monsters)
            {
                if (monster == null) continue;

                Vector3 toTarget = (monster.transform.position - transform.position).normalized;
                float distance = Vector3.Distance(transform.position, monster.transform.position);



                // 전방 2미터 내에 있는 몬스터만
                if (Vector3.Dot(transform.forward, toTarget) > 0.7f && distance <= 2.0f)
                {
                    monster.GetDamage(SkillDamage(100.0f));
                }
            }
            yield return new WaitForSeconds(0.25f);
        }
        ReturnSkill();
    }


}
