using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{
    [Range(0.0f, 100.0f)]
    public float speed;

    public ParticleSystem Expolosion_Particle;
    public Transform Meteor_OBJ;
    public Transform Circle;

    Transform parentTransform;

    public void Init(double dmg)
    {
        if(parentTransform == null)
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
        Meteor_OBJ.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
        Stage_Manager.m_ReadyEvent -= DisableOBJ;
    }

    IEnumerator Meteor_Coroutine(double dmg)
    {
        Meteor_OBJ.localPosition = new Vector3(Random.Range(-10.0f, 10.0f), Random.Range(10.0f, 15.0f), Random.Range(5.0f, 10.0f));
        Meteor_OBJ.gameObject.SetActive(true);
        Meteor_OBJ.LookAt(transform.parent);

        Circle.localScale = Vector3.one;
        SpriteRenderer renderer = Circle.GetComponent<SpriteRenderer>();
        Player mainCharacter = Spawner.m_Players.Find(p => p.MainCharacter);

        while (true)
        {
            float distance = Vector3.Distance(Meteor_OBJ.localPosition, Vector3.zero);

            if(distance >= 0.1f)
            {
                Meteor_OBJ.localPosition = Vector3.MoveTowards(Meteor_OBJ.localPosition, Vector3.zero, Time.deltaTime * speed);
                float ScaleValue = distance / speed;
                // Mathf.Min((distance / speed), 0.5f) = 두 값중에 더욱 작은값
                renderer.color = new Color(0, 0, 0, Mathf.Min((distance / speed), 0.5f));
                Circle.localScale = new Vector3(ScaleValue, ScaleValue, ScaleValue);
                yield return null;
            }
            else
            {
                Expolosion_Particle.Play();
                Camera_Manager.instance.CameraShake();
                for(int i =0; i<Spawner.m_Monsters.Count; i++)
                {
                    if(Spawner.m_Monsters[i] != null)
                    {
                        if (Vector3.Distance(transform.position, Spawner.m_Monsters[i].transform.position) <= 1.5f)
                        {
                            Spawner.m_Monsters[i].GetDamage(dmg);
                        }
                    }
                }
                break;
            }
        }
        yield return new WaitForSeconds(0.5f);
        transform.parent = parentTransform;
        Meteor_OBJ.gameObject.SetActive(false);
        Stage_Manager.m_ReadyEvent -= DisableOBJ;
        this.gameObject.SetActive(false);

    }
}
