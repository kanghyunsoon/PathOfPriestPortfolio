using UnityEngine;
using System.Collections;
public class MagicCircle : MonoBehaviour
{
    public ParticleSystem Expolosion_Particle;
    public Transform Circle;

    Transform parentTransform;

    public void Init(double dmg)
    {
        if (parentTransform == null)
        {
            parentTransform = transform.parent;
        }
        transform.parent = null;
        StartCoroutine(Meteor_Coroutine(dmg));
        // 발동할 때마다 구독이 쌓이지 않게 한 번만 연결한다.
        Stage_Manager.m_ReadyEvent -= DisableOBJ;
        Stage_Manager.m_ReadyEvent += DisableOBJ;
    }

    private void DisableOBJ()
    {
        StopAllCoroutines();
        transform.parent = parentTransform;
        this.gameObject.SetActive(false);
        Stage_Manager.m_ReadyEvent -= DisableOBJ;
    }

    IEnumerator Meteor_Coroutine(double dmg)
    {
        Circle.localScale = Vector3.zero;
        Circle.gameObject.SetActive(true);

        SpriteRenderer renderer = Circle.GetComponent<SpriteRenderer>();
        renderer.color = new Color(0, 0, 0, 0);

        float elapsedTime = 0f;
        float appearDuration = 1.0f;

        while (elapsedTime < appearDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / appearDuration;
            float alpha = Mathf.Lerp(0f, 0.5f, t);
            float scale = Mathf.Lerp(0f, 1f, t);
            renderer.color = new Color(0, 0, 0, alpha);
            Circle.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        Expolosion_Particle.Play();
        Camera_Manager.instance.CameraShake();

        for (int i = 0; i < Spawner.m_Monsters.Count; i++)
        {
            if (Spawner.m_Monsters[i] != null)
            {
                float distance = Vector3.Distance(transform.position, Spawner.m_Monsters[i].transform.position);
                if (distance <= 1.5f)
                {
                    Spawner.m_Monsters[i].GetDamage(dmg);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        transform.parent = parentTransform;
        Stage_Manager.m_ReadyEvent -= DisableOBJ;
        this.gameObject.SetActive(false);

    }
}
