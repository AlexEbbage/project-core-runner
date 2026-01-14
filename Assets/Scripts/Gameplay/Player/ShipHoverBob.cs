using UnityEngine;

public class ShipHoverBob : MonoBehaviour
{
    [SerializeField] private Vector3 localAxis = Vector3.up;
    [SerializeField] private float amplitude = 0.12f;
    [SerializeField] private float frequency = 1.1f;
    [SerializeField] private bool useUnscaledTime;

    private Vector3 _startLocalPosition;

    private void Awake()
    {
        _startLocalPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        _startLocalPosition = transform.localPosition;
    }

    private void OnDisable()
    {
        transform.localPosition = _startLocalPosition;
    }

    private void Update()
    {
        Vector3 axis = localAxis.sqrMagnitude > 0.0001f ? localAxis.normalized : Vector3.up;
        float time = useUnscaledTime ? Time.unscaledTime : Time.time;
        float offset = Mathf.Sin(time * Mathf.PI * 2f * frequency) * amplitude;
        transform.localPosition = _startLocalPosition + axis * offset;
    }
}
