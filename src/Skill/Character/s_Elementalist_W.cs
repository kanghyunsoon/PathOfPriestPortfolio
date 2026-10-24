using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_Elementalist_W : Skill_Base
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

        SkillParticle.SetActive(true);
        int value = SkillParticle.transform.childCount;
        for(int i =0; i<value; i++)
        {
            // 몬스터가 모두 사라졌으면 빈 배열에서 고르다 예외가 나고 스킬 상태가 풀리지 않았다.
            Monster[] targets = monsters;
            if (targets.Length == 0) break;

            var meteor = SkillParticle.transform.GetChild(0).GetComponent<Meteor>();
            meteor.gameObject.SetActive(true);
            Vector3 pos = targets[Random.Range(0, targets.Length)].transform.position +
                new Vector3(Random.insideUnitSphere.x * 2.0f, 0.0f, Random.insideUnitSphere.z * 2.0f);

            meteor.transform.position = pos;
            meteor.Init(SkillDamage(150));
            yield return new WaitForSeconds(0.2f);
        }
        ReturnSkill();

    }
}
