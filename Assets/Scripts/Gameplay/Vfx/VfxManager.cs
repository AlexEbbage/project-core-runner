using System.Collections.Generic;
using UnityEngine;

public class VfxManager : MonoBehaviour
{
    private static VfxManager _instance;

    [Header("Pooling")]
    [SerializeField] private float defaultLifetime = 2f;

    private readonly Dictionary<GameObject, Queue<GameObject>> _pool = new Dictionary<GameObject, Queue<GameObject>>();

    public static VfxManager Instance
    {
        get
        {
            if (_instance != null)
                return _instance;

            _instance = FindFirstObjectByType<VfxManager>();
            if (_instance == null)
            {
                var host = new GameObject("VfxManager");
                _instance = host.AddComponent<VfxManager>();
            }

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        GameObject instance = GetInstance(prefab);
        Transform instanceTransform = instance.transform;
        instanceTransform.SetPositionAndRotation(position, rotation);

        instance.SetActive(true);
        PlayParticleSystems(instance);

        float lifetime = GetMaxLifetime(instance);
        var pooled = instance.GetComponent<VfxPooledInstance>();
        if (pooled == null)
        {
            pooled = instance.AddComponent<VfxPooledInstance>();
        }

        pooled.Init(this, prefab);
        pooled.ScheduleReturn(lifetime);

        return instance;
    }

    public void ReturnToPool(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null)
            return;

        instance.SetActive(false);
        instance.transform.SetParent(transform);

        if (!_pool.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pool[prefab] = queue;
        }

        queue.Enqueue(instance);
    }

    private GameObject GetInstance(GameObject prefab)
    {
        if (_pool.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            while (queue.Count > 0)
            {
                GameObject instance = queue.Dequeue();
                if (instance != null)
                {
                    instance.transform.SetParent(null);
                    return instance;
                }
            }
        }

        return Instantiate(prefab);
    }

    private float GetMaxLifetime(GameObject instance)
    {
        ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>(true);
        float maxLifetime = 0f;

        foreach (ParticleSystem system in systems)
        {
            var main = system.main;
            float startLifetime = GetMaxCurveValue(main.startLifetime);
            maxLifetime = Mathf.Max(maxLifetime, main.duration + startLifetime);
        }

        if (maxLifetime <= 0f)
        {
            maxLifetime = defaultLifetime;
        }

        return maxLifetime;
    }

    private static float GetMaxCurveValue(ParticleSystem.MinMaxCurve curve)
    {
        switch (curve.mode)
        {
            case ParticleSystemCurveMode.TwoConstants:
                return curve.constantMax;
            case ParticleSystemCurveMode.TwoCurves:
                return curve.curveMultiplier * curve.curveMax.Evaluate(1f);
            case ParticleSystemCurveMode.Curve:
                return curve.curveMultiplier * curve.curve.Evaluate(1f);
            case ParticleSystemCurveMode.Constant:
            default:
                return curve.constant;
        }
    }

    private static void PlayParticleSystems(GameObject instance)
    {
        ParticleSystem[] systems = instance.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem system in systems)
        {
            system.Clear(true);
            system.Play(true);
        }
    }
}
