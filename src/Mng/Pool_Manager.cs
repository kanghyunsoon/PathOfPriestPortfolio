using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPool
{
    Transform parentTransform { get; set; }
    Queue<GameObject> pool { get; set; } // Queue - 선입선출(먼저 들어온 오브젝트를 먼저 내보냄)

    GameObject Get(Action<GameObject> action = null);

    void Return(GameObject obj, Action<GameObject> action = null);
}
public class Object_Pool : IPool
{
    // Dequeue = 먼저 들어온 오브젝트를 내보낸다.
    // EnQueue = 오브젝트를 queue내부에 집어넣는다.
    public Queue<GameObject> pool { get; set; } = new Queue<GameObject>();

    public Transform parentTransform { get; set; }

    public GameObject Get(Action<GameObject> action = null)
    {
        GameObject obj = pool.Dequeue();
        obj.SetActive(true);

        if(action !=null)
        {
            action?.Invoke(obj);
        }

        return obj;
    }

    public void Return(GameObject obj, Action<GameObject> action = null)
    {
        // 같은 오브젝트를 두 번 반납하면 큐에 두 번 들어가 한 오브젝트가 동시에 두 곳에서 쓰인다.
        // (데미지 텍스트는 절전 모드 진입 때 한 번, 2초 타이머로 한 번 더 반납됐다)
        if (pool.Contains(obj))
        {
            return;
        }

        pool.Enqueue(obj);
        obj.transform.SetParent(parentTransform,false);
        obj.SetActive(false);

        if (action != null)
        {
            action?.Invoke(obj);
        }
    }
}

public class Pool_Manager
{
    // ContainsKey(string) =
    public Dictionary<string, IPool> m_pool_Dictionary = new Dictionary<string, IPool>();

    Transform base_obj = null;

    public void Initalize(Transform T)
    {
        base_obj = T;
    }

    public IPool Pooling_OBJ(string path)
    {
        if(m_pool_Dictionary.ContainsKey(path) == false)
        {
            Add_Pool(path);
        }

        if (m_pool_Dictionary[path].pool.Count <= 0)
        {
            Add_Queue(path);
        }

        return m_pool_Dictionary[path];

    }

    private GameObject Add_Pool(string path)
    {
        GameObject obj = new GameObject(path + "##POOL");

        obj.transform.SetParent(base_obj);

        Object_Pool T_Component = new Object_Pool();

        m_pool_Dictionary.Add(path, T_Component);

        T_Component.parentTransform = obj.transform;
        return obj;
    }

    private void Add_Queue(string path)
    {
        var go = Base_Manager.instance.Instantiate_Path("Pool_OBJ/" + path);
        go.transform.SetParent(m_pool_Dictionary[path].parentTransform, false);
        m_pool_Dictionary[path].Return(go);
    }
}
