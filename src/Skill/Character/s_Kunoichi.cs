using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.Rendering;

public class s_Kunoichi : Skill_Base
{
    public float Radius = 5.0f;
    public float MoveSeed = 30.0f;
    public SkinnedMeshRenderer[] Renderers;

    bool isSlashing = false;
    private List<Vector3> starPoints;
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
        skillCoroutine = StartCoroutine(Set_Skill_Coroutine());
        base.Set_Skill();
    }

    public override void ReturnSkill()
    {
        if (SkillParticle != null)
            SkillParticle.SetActive(false);

        for (int i = 0; i < Renderers.Length; i++)
            Renderers[i].enabled = true;

        isSlashing = false;

        base.ReturnSkill();
    }

    IEnumerator Set_Skill_Coroutine()
    {

        isSlashing = true;
        Vector3 center = transform.position;
        starPoints = CalculateStarPoints(center, Radius);

        for(int i = 0; i< Renderers.Length; i++)
        {
            Renderers[i].enabled = false;
        }

        HashSet<Monster> damagedMonsters = new HashSet<Monster>(); // 데미지 중복 방지용

        foreach (Vector3 end in starPoints)
        {
            Vector3 start = transform.position;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime * MoveSeed / Vector3.Distance(start, end);
                transform.position = Vector3.Lerp(start, end, t);

                // 거리 기반으로 데미지 적용 (foreach → for문으로 변경)
                for (int i = 0; i < Spawner.m_Monsters.Count; i++)
                {
                    var monster = Spawner.m_Monsters[i];
                    if (monster == null || monster.isDead) continue;

                    float dist = Vector3.Distance(transform.position, monster.transform.position);
                    if (dist <= 1.0f && !damagedMonsters.Contains(monster))
                    {
                        monster.GetDamage(SkillDamage(130f));
                        damagedMonsters.Add(monster);
                    }
                }

                yield return null;
            }
        }

        ReturnSkill();
    }

    List<Vector3> CalculateStarPoints(Vector3 center, float radius)
    {
        List<Vector3> points = new List<Vector3>();
        float angleOffset = 90.0f;

        Vector3[] outerPoints = new Vector3[5];
        Vector3[] innerPoints = new Vector3[5];

        for (int i = 0; i < 5; i++)
        {
            float angleDeg = angleOffset + i * 72.0f;
            float rad = angleDeg * Mathf.Deg2Rad;
            outerPoints[i] = center + new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;

            // 내부각
            //float innerAngle = angleDeg + 36.0f;
            //float innerRad = innerAngle * Mathf.Deg2Rad;
            //innerPoints[i] = center + new Vector3(Mathf.Cos(innerRad), 0, Mathf.Sin(innerRad)) * (radius * 0.5f);
        }

        int[] starOrder = { 0, 2, 4, 1, 3 };

        foreach (int i in starOrder)
        {
            points.Add(outerPoints[i]);
        }

        return points;
    }
}
