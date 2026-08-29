using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_Hunter : Skill_Base
{
    private Coroutine skillCoroutine;
    public override void Set_Skill()
    {
        base.Set_Skill();
        if (skillCoroutine != null)
        {
            StopCoroutine(skillCoroutine);
            skillCoroutine = null; // 중복 실행 방지
        }
        skillCoroutine = StartCoroutine(Set_Skill_Coroutine());
    }

    IEnumerator Set_Skill_Coroutine()
    {
        if (this == null || SkillParticle == null) yield break;
        m_Player.ATK_Speed = m_Player.ATK_Speed * 2.0f;
        SkillParticle.SetActive(true);
        yield return new WaitForSeconds(5.0f);
        m_Player.ATK_Speed = m_Player.ATK_Speed / 2.0f;
        SkillParticle.SetActive(false);
        ReturnSkill();
    }
}
