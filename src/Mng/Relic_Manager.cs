using UnityEngine;


public class Relic_Manager : MonoBehaviour
{
    public static Relic_Manager instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void Initialize()
    {
        if (Base_Manager.Item.SetItemCheck("Dice")) Delegate_Holder.M_Dead_Event += Dice;
        if (Base_Manager.Item.SetItemCheck("Axe")) Delegate_Holder.P_Attack_Event += Axe;
        if (Base_Manager.Item.SetItemCheck("Mana")) Delegate_Holder.P_Hit_Event += Mana;

    }
    public void Axe(Player player, Monster monster)
    {
        if (!RandomCount(50.0f)) return;
        Vector3 RealPos = monster.transform.position;
        GameObject go = Instantiate(Resources.Load<GameObject>("Prefabs/Axe"));
        go.transform.position = RealPos;

        for (int i = 0; i < Spawner.m_Monsters.Count; i++)
        {
            if (Vector3.Distance(Spawner.m_Monsters[i].transform.position, RealPos) <= 1.5f)
            {
                Spawner.m_Monsters[i].GetDamage(player.ATK * 0.7f);
            }
        }

    }

    public void Mana(Player player)
    {
        if (!RandomCount(50.0f))
        {
            return;
        }
        player.Get_MP(2);
    }

    public void Dice(Monster monster)
    {
        if (!RandomCount(30.0f))
        {
            return;
        }

        Vector3 RealPos = monster.transform.position;

        Base_Manager.Pool.Pooling_OBJ("Dice").Get((value) =>
        {
            value.GetComponent<Dice>().Init(RealPos);
        });
    }

    private bool RandomCount(float RandomValue)
    {
        //int level = 0;
        //for (int i = 0; i < Base_Manager.Data.m_Set_Item.Length; i++)
        //{
        //    if (Base_Manager.Data.m_Set_Item[i].name == "Dice") level = Base_Manager.Data.Item_Holder[Base_Manager.Data.m_Set_Item[i].name].Level;
        //}
        float RandomCount = Random.Range(0.0f, 100.0f);// + (((float)level - 1.0f) * 3.0f);
        if(RandomCount <= RandomValue)
        {
            return true;
        }
        return false;
    }
}
