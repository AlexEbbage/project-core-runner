using UnityEngine;

public class VfxPooledInstance : MonoBehaviour
{
    private VfxManager _manager;
    private GameObject _prefab;
    private Coroutine _returnCoroutine;

    public void Init(VfxManager manager, GameObject prefab)
    {
        _manager = manager;
        _prefab = prefab;
    }

    public void ScheduleReturn(float delay)
    {
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
        }

        _returnCoroutine = StartCoroutine(ReturnAfterDelay(delay));
    }

    private System.Collections.IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (_manager == null)
        {
            Destroy(gameObject);
            yield break;
        }

        _manager.ReturnToPool(_prefab, gameObject);
    }
}
