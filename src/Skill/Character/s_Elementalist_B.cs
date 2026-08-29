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

            if(monsters.Length == 0)
            {
                yield break;
            }
            Vector3 basePos = monsters[Random.Range(0, monsters.Length)].transform.position;
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
