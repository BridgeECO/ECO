using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

#region Pool
internal class Pool
{
    private GameObject _prefab;
    private IObjectPool<GameObject> _pool;

    private Transform _root;
    private Transform Root
    {
        get
        {
            if (_root == null)
            {
                GameObject go = new GameObject() { name = $"{_prefab.name}Pool" };
                _root = go.transform;
            }
            return _root;
        }
    }

    public Pool(GameObject prefab)
    {
        _prefab = prefab;
        _pool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
    }

    public void Push(GameObject go)
    {
        if (go.activeSelf)
        {
            _pool.Release(go);
        }
    }

    public GameObject Pop()
    {
        return _pool.Get();
    }

    #region Funcs
    private GameObject OnCreate()
    {
        GameObject go = GameObject.Instantiate(_prefab);
        go.transform.SetParent(Root);
        go.name = _prefab.name;
        return go;
    }

    private void OnGet(GameObject go)
    {
        go.SetActive(true);
    }

    private void OnRelease(GameObject go)
    {
        go.SetActive(false);
    }

    private void OnDestroy(GameObject go)
    {
        GameObject.Destroy(go);
    }
    #endregion
}
#endregion

public class PoolManager : MonoBehaviourSingleton<PoolManager>
{
    [SerializeField]
    private List<PoolInfo> _poolInfos = new();

    private Dictionary<EPoolable, Pool> _pools = new();

    public bool Push(EPoolable type, GameObject go)
    {
        if (!_pools.ContainsKey(type))
        {
            return false;
        }
        _pools[type].Push(go);
        return true;
    }

    public GameObject Pop(EPoolable type)
    {
        if (_pools.TryGetValue(type, out Pool pool))
        {
            return pool.Pop();
        }

        GameObject prefab = GetPrefabOnType(type);
        if (prefab == null)
        {
            return null;
        }

        CreatePool(type, prefab);
        return _pools[type].Pop();
    }

    private GameObject GetPrefabOnType(EPoolable type)
    {
        foreach (var poolInfo in _poolInfos)
        {
            if (type == poolInfo.Type)
            {
                return poolInfo.Prefab;
            }
        }
        Debug.LogError($"[ObjectPool] '{type}' 타입에 대한 프리팹 설정이 _poolInfos에 누락되었습니다!");
        return null;
    }

    private void CreatePool(EPoolable type, GameObject go)
    {
        _pools.Add(type, new Pool(go));
    }

    public void Clear()
    {
        _pools.Clear();
    }
}