using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float m_Speed;
    Transform m_Target;
    Vector3 m_TargetPos;
    double m_DMG;
    string m_CharacterName;
    bool GetHit = false;

    public ParticleSystem Attack_Particle;


    Dictionary<string, GameObject> m_Projectiles = new Dictionary<string, GameObject>();
    Dictionary<string, ParticleSystem> m_Muzzles = new Dictionary<string, ParticleSystem>();

    private void Awake()
    {
        Transform projectiles = transform.GetChild(0);
        Transform muzzles = transform.GetChild(1);



        for (int i = 0; i < projectiles.childCount; i++) m_Projectiles.Add(projectiles.GetChild(i).name, projectiles.GetChild(i).gameObject);
        for (int i = 0; i < muzzles.childCount; i++) m_Muzzles.Add(muzzles.GetChild(i).name, muzzles.GetChild(i).GetComponent<ParticleSystem>());
    }

    public void Attack_Init(Transform target, double dmg)
    {
        m_Target = target;

        if(m_Target != null)
        {
            m_Target.GetComponent<Character>().GetDamage(dmg);
        }

        GetHit = true;

        if (!Attack_Particle.isPlaying)
        {
            Attack_Particle.Play();
        }


        StartCoroutine(ReturnObject(Attack_Particle.main.duration));
    }

    public void Init(Transform target, double dmg, string Character_Name)
    {
        GetHit = false;
        m_Target = target;
        transform.LookAt(m_Target);

        m_TargetPos = m_Target.position;

        m_DMG = dmg;
        m_CharacterName = Character_Name;
        m_Projectiles[m_CharacterName].gameObject.SetActive(true);
    }

    private void Update()
    {
        if (GetHit) return;

        m_TargetPos.y = 0.5f;

        transform.position = Vector3.MoveTowards(transform.position, m_TargetPos, Time.deltaTime * m_Speed);

        if(Vector3.Distance(transform.position, m_TargetPos) <= 0.1f)
        {
            GetHit = true;

            // 날아가는 동안 보스가 파괴되거나 몬스터가 풀로 돌아가도 투사체는 반납한다.
            if (m_Target != null && m_Target.gameObject.activeInHierarchy)
            {
                m_Target.GetComponent<Character>().GetDamage(m_DMG);
            }

            m_Projectiles[m_CharacterName].gameObject.SetActive(false);
            m_Muzzles[m_CharacterName].Play();

            StartCoroutine(ReturnObject(m_Muzzles[m_CharacterName].main.duration));
        }
    }

    IEnumerator ReturnObject(float timer)
    {
        yield return new WaitForSeconds(timer);
        Base_Manager.Pool.m_pool_Dictionary["Attack_Helper"].Return(this.gameObject);

    }
}
