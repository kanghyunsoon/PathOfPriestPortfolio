using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main_Character : Skill_Base
{
    float delay = 5.0f;
    Coroutine coroutine = null;
    private void Start()
    {
        Stage_Manager.m_ReadyEvent += OnReady;
    }

    public override void Set_Skill()
    {
        if (m_Player.isDead == true) return;
        m_Player.isGetSkill = true;
        m_Player.AnimatorChange("IsSkill");

        var character = HP_Check();
        m_Player.transform.LookAt(character.transform.position);

        SkillParticle.gameObject.SetActive(true);
        SkillParticle.transform.position = character.transform.position;

        character.Heal(SkillDamage(110));

        Invoke("ReturnSkill",1.0f);
        base.Set_Skill();
    }

    public override void ReturnSkill()
    {
        SkillParticle.gameObject.SetActive(false);
        base.ReturnSkill();
    }

    public void OnReady()
    {
        if (coroutine != null) StopCoroutine(coroutine);

        float skillBonus = Base_Manager.Quest.Achievement_Status_Data.Skill; // 업적에서 스킬 쿨타임 감소 수치 (예: 20%)
        float modifiedDelay = delay * (1.0f - skillBonus / 100.0f); // 쿨타임 감소 적용

        coroutine = StartCoroutine(SkillCoroutine(modifiedDelay));
    }

    IEnumerator SkillCoroutine(float value)
    {
        float timer = value;
        while(timer > 0.0f)
        {
            timer -= Time.deltaTime;
            Main_UI.instance.Main_Character_Fill.fillAmount = timer / value;
            yield return null;
        }
        Set_Skill();

    }
}
