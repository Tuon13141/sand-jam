using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolingObject<T> where T : MonoBehaviour
{
    public ObjectPool<T> pool;

    public PoolingObject(T prefab, int maxSize)
    {
        pool = new ObjectPool<T>(() => CreateElement(prefab), OnGetPoolObject, OnReleasePoolObject, OnDestroyPoolObject, maxSize: maxSize);
    }

    public void Release(T element)
    {
        element.transform.SetParent(PoolingManager.instance.transform);
        pool.Release(element);
    }

    public T Get()
    {
        return pool.Get();
    }

    private T CreateElement(T prefab)
    {
        return Static.InstantiateUtility(prefab, null);
    }

    private void OnGetPoolObject(T poolObject)
    {
        poolObject.gameObject.SetActive(true);
    }

    private void OnReleasePoolObject(T poolObject)
    {
        poolObject.gameObject.SetActive(false);
    }

    private void OnDestroyPoolObject(T poolObject)
    {
        GameObject.Destroy(poolObject.gameObject);
    }
}

public class PoolingManager : Kit.Common.Singleton<PoolingManager>
{
    [SerializeField] PrefabManager m_PrefabManager;

    public Dictionary<System.Type, PoolingObject<MonoBehaviour>> dic = new();

    public MonoBehaviour Get(System.Type type)
    {
        if (dic.TryGetValue(type, out PoolingObject<MonoBehaviour> pool))
        {
            return pool.Get();
        }

        return null;
    }

    public void Release(MonoBehaviour poolingObject)
    {
        if (dic.TryGetValue(poolingObject.GetType(), out PoolingObject<MonoBehaviour> pool))
        {
            pool.Release(poolingObject);
        }
    }

    protected override void Awake()
    {
        base.Awake();

        var poolingObjectFields = m_PrefabManager.GetAllSerializeField();
        foreach (var field in poolingObjectFields)
        {
            //Debug.Log(field.GetType());
            dic.Add(field.GetType(), new PoolingObject<MonoBehaviour>((MonoBehaviour)field, 1000));
        }
    }
}
