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
        // 공격 속도를 직접 곱했다 나누면 그 사이 능력치가 다시 계산될 때 기본값의 절반으로 남는다.
        SetAtkSpeedMultiplier(2.0f);
        SkillParticle.SetActive(true);
        yield return new WaitForSeconds(5.0f);
        SkillParticle.SetActive(false);
        ReturnSkill();
    }

    public override void ReturnSkill()
    {
        // 스테이지 전환이나 사망으로 스킬이 중간에 끝나도 배율을 되돌린다.
        SetAtkSpeedMultiplier(1.0f);
        base.ReturnSkill();
    }

    private void SetAtkSpeedMultiplier(float multiplier)
    {
        Player player = GetComponent<Player>();
        if (player != null) player.SetSkillAtkSpeedMultiplier(multiplier);
    }
}
