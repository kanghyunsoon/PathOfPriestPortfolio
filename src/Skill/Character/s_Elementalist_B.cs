using UnityEngine;
using System.Collections;

public class s_Elementalist_B : Skill_Base
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
        int count = SkillParticle.transform.childCount;

        for (int i = 0; i < count; i++)
        {
            var magicCircle = SkillParticle.transform.GetChild(0).GetComponent<MagicCircle>();
            if (magicCircle == null) continue;

            Monster[] targets = monsters;
            if (targets.Length == 0)
            {
                // 바로 끝내면 스킬 사용 상태가 남아 다음 스테이지까지 스킬을 다시 못 썼다.
                break;
            }
            Vector3 basePos = targets[Random.Range(0, targets.Length)].transform.position;
            Vector3 offset = new Vector3(Random.insideUnitSphere.x * 2.0f, 0.0f, Random.insideUnitSphere.z * 2.0f);
            Vector3 spawnPos = basePos + offset;

            magicCircle.transform.position = spawnPos;
            magicCircle.gameObject.SetActive(true);
            magicCircle.Init(SkillDamage(150)); // 데미지 계산 함수

            yield return new WaitForSeconds(0.5f);
        }
        ReturnSkill();

    }
}
