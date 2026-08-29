using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Render_Gacha : MonoBehaviour
{
    [SerializeField] private GameObject LootEffect;
    [SerializeField] private GameObject[] RarityEffect;

    [SerializeField] private Transform[] ElevenCount;
    [SerializeField] private Transform OneCount;

    List<GameObject> Garvage = new List<GameObject>();
    public void Init()
    {
        for (int i = 0; i < Garvage.Count; i++)
        {
            Destroy(Garvage[i]);
        }
        Garvage.Clear();
    }
    public void Get_Hero(int GachaValue, int value, Character_Scriptable data)
    {
        Transform parent = Character_Transform(GachaValue, value);
        Vector3 efPosition = parent.position + new Vector3(0f, 0.05f, 0f);

        var loot = Instantiate(LootEffect, efPosition, LootEffect.transform.rotation, parent);
        var go = Instantiate(Resources.Load<GameObject>("Character/" + data.name), parent);
        var effect = Instantiate(RarityEffect[(int)data.m_Rarity], efPosition, RarityEffect[(int)data.m_Rarity].transform.rotation, parent);

        go.GetComponent<Player>().enabled = false;


        StartCoroutine(ScaleCoroutine(go.transform));

        go.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        go.transform.localPosition = Vector3.zero;

        Garvage.Add(loot);
        Garvage.Add(go);
        Garvage.Add(effect);
    }

    IEnumerator ScaleCoroutine(Transform obj)
    {
        float current = 0.0f;
        float percent = 0.0f;
        float start = 0.0f;
        float end = 0.4f;
        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / 0.2f;
            float LerpScale = Mathf.Lerp(start, end, percent);
            obj.localScale = new Vector3(LerpScale, LerpScale, LerpScale);
            yield return null;
        }
    }

    private Transform Character_Transform(int GachaValue, int value)
    {
        switch (GachaValue)
        {
            case 11: return ElevenCount[value];
            case 1: return OneCount;
        }
        return null;
    }

}
