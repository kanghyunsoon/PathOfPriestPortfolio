using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class COIN_PARENT : MonoBehaviour
{
    Vector3 target;
    Camera cam;

    RectTransform[] childs = new RectTransform[5];

    [Range(0.0f, 500.0f)]
    [SerializeField] private float m_DistanceRange, speed;

    private void Awake()
    {
        cam = Camera.main;
        for (int i = 0; i < childs.Length; i++) childs[i] = transform.GetChild(i).GetComponent<RectTransform>();


    }

    private void OnSave()
    {
        Data_Manager.m_Data.Money += Utils.Data.stageData.MONEY();
        if (Distance_Boolean_World(0.5f))
        {
            Base_Manager.Pool.m_pool_Dictionary["COIN_PARENT"].Return(this.gameObject);
        }
    }

    private void OnDisable()
    {
        UI_SavingMode.m_OnSaving -= OnSave;
    }

    public void Init(Vector3 pos, bool offline, Coin_type type = Coin_type.Gold, int value = 0)
    {
        UI_SavingMode.m_OnSaving += OnSave;

        if (Base_Canvas.isSave) return;

        target = pos;

        transform.localScale = Vector3.one;

        for (int i = 0; i < 5; i++)
        {
            childs[i].GetComponent<Image>().sprite = Utils.Get_Atlas(type.ToString());
            childs[i].anchoredPosition = Vector2.zero;
        }
        transform.SetParent(Base_Canvas.instance.HOLDER_LAYER(0));

        // 수정: World → Screen 좌표 변환 후, UI 위치로 설정
        Vector3 screenPosition = cam.WorldToScreenPoint(pos);
        transform.position = screenPosition;

        if (!offline)
        {
            switch(type)
            {
                case Coin_type.Gold:
                    double baseMoney = Utils.Data.stageData.MONEY();

                    float dropChance = Base_Manager.Player.GoldDropPercentage();
                    float roll = Random.Range(0f, 100f);

                    double finalGold = (roll <= dropChance)
                        ? baseMoney * 1.5f // 보너스 드랍
                        : baseMoney;       // 일반 드랍

                    Data_Manager.m_Data.Money += finalGold;
                    break;
                case Coin_type.Dia: Data_Manager.m_Data.Dia +=value; break;
            }
        }
        StartCoroutine(Coin_Effect());
    }

    IEnumerator Coin_Effect()
    {
        Vector2[] RandomPos = new Vector2[childs.Length];
        for(int i =0; i<childs.Length; i++)
        {
            RandomPos[i] = new Vector2(target.x, target.y) + Random.insideUnitCircle * Random.Range(-m_DistanceRange, m_DistanceRange);
        }

        while(true)
        {
            for(int i =0; i<childs.Length; i++)
            {
                RectTransform rect = childs[i];

                rect.anchoredPosition = Vector2.MoveTowards(rect.anchoredPosition, RandomPos[i], Time.deltaTime * speed);
            }

            if (Distance_Boolean(RandomPos, 0.5f)) break;

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        while(true)
        {
            for (int i = 0; i < childs.Length; i++)
            {
                RectTransform rect = childs[i];

                rect.position = Vector2.MoveTowards(rect.position, Base_Canvas.instance.COIN.position, Time.deltaTime * speed * 20);
            }

            if (Distance_Boolean_World(0.5f))
            {
                Base_Manager.Pool.m_pool_Dictionary["COIN_PARENT"].Return(this.gameObject);
                break;
            }
            yield return null;
        }

        Main_UI.instance.TextCheck();
    }

    private bool Distance_Boolean(Vector2[] end, float range)
    {
        for(int i =0; i<childs.Length; i++)
        {
            float distance = Vector2.Distance(childs[i].anchoredPosition, end[i]);
            if(distance > range)
            {
                return false;
            }
        }
        return true;
    }

    private bool Distance_Boolean_World(float range)
    {
        for (int i = 0; i < childs.Length; i++)
        {
            float distance = Vector2.Distance(childs[i].position, Base_Canvas.instance.COIN.position);
            if (distance > range)
            {
                return false;
            }
        }
        return true;
    }
}
