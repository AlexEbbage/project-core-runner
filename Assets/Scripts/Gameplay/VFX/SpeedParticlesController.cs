using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SpeedParticlesController : MonoBehaviour
{
    [Header("Speed Normalization")]
    [SerializeField] private float minSpeed = 10f;
    [SerializeField] private float maxSpeed = 40f;

    [Header("Particles")]
    [SerializeField] private float minParticleSpeed = 8f;
    [SerializeField] private float maxParticleSpeed = 25f;

    [SerializeField] private float minEmission = 10f;
    [SerializeField] private float maxEmission = 60f;

    private ParticleSystem _ps;
    private ParticleSystem.MainModule _main;
    private ParticleSystem.EmissionModule _emission;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
        _main = _ps.main;
        _emission = _ps.emission;
    }

    public void SetSpeedRange(float zoneMinSpeed, float zoneMaxSpeed)
    {
        minSpeed = Mathf.Max(0f, zoneMinSpeed);
        maxSpeed = Mathf.Max(minSpeed + 0.01f, zoneMaxSpeed);
    }

    public void SetRunSpeed(float speed)
    {
        float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
        float particleSpeed = Mathf.Lerp(minParticleSpeed, maxParticleSpeed, t);
        float emissionRate = Mathf.Lerp(minEmission, maxEmission, t);

        _main.startSpeed = particleSpeed;
        _emission.rateOverTime = emissionRate;
    }
}
