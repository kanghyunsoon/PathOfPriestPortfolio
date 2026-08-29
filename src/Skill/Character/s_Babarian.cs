using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class s_Babarian : Skill_Base
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

        Base_Manager.Sound.Play(Sound.Effect, "Babarian");
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

        for (int i = 0; i<5; i++)
        {
            for (int j = 0; j < monsters.Count(); j++)
            {
                if(Distance(transform.position,monsters[j].transform.position, 1.5f))
                {
                    monsters[j].GetDamage(SkillDamage(130.0f));
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
        ReturnSkill();
    }


}
