using UnityEngine;

public class Skill_Base : MonoBehaviour
{
    public GameObject SkillParticle;

    protected Monster[] monsters { get { return Spawner.m_Monsters.ToArray(); } }
    protected Player[] players { get { return Spawner.m_Players.ToArray(); } }

    protected Character m_Player { get { return GetComponent<Character>(); } }

    private bool isDestroyed = false;

    private void Start()
    {
        Stage_Manager.m_DeadEvent += OnDead;
        Stage_Manager.m_ReadyEvent += OnReady;
        Stage_Manager.m_PlayEvent += OnPlay;
    }

    public virtual void Set_Skill()
    {

    }

    protected double SkillDamage(double value)
    {
        return m_Player.ATK * (value / 100.0f);
    }

    protected bool Distance(Vector3 startPos, Vector3 endPos, float distanceValue)
    {
        return Vector3.Distance(startPos, endPos) <= distanceValue;
    }

    private void OnDestroy()
    {
        isDestroyed = true;
        Stage_Manager.m_DeadEvent -= OnDead;
        Stage_Manager.m_ReadyEvent -= OnReady;
        Stage_Manager.m_PlayEvent -= OnPlay;

        // 삭제될 때도 스킬 종료 처리
        ReturnSkill();
    }

    private void OnDead()
    {
        if (isDestroyed) return;
        StopAllCoroutines();
        ReturnSkill(); // 추가: 스킬 리셋
    }

    private void OnReady()
    {
        if (isDestroyed) return;
        StopAllCoroutines();
        ReturnSkill(); // 추가: 스킬 리셋
    }

    private void OnPlay()
    {
        if (isDestroyed) return;
        StopAllCoroutines();
        ReturnSkill(); // 추가: 스킬 리셋
    }

    public virtual void ReturnSkill()
    {
        if (SkillParticle != null)
            SkillParticle.SetActive(false);

        if (m_Player != null)
        {
            var playerComponent = m_Player.GetComponent<Player>();
            if (playerComponent != null)
            {
                playerComponent.isGetSkill = false;
            }
            m_Player.AnimatorChange("IsIdle");
        }
    }

    public Character HP_Check()
    {
        Character player = null;
        double hpCount = Mathf.Infinity;

        for (int i = 0; i < players.Length; i++)
        {
            double hp = players[i].HP;
            if (hp < hpCount)
            {
                hpCount = hp;
                player = players[i];
            }
        }
        return player;
    }
}
