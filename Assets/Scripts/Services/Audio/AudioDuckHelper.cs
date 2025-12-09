using UnityEngine;
using UnityEngine.Audio;

public class AudioDuckHelper : MonoBehaviour
{
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private float duckVolumeDb = -12f;
    [SerializeField] private float normalVolumeDb = -6f;
    [SerializeField] private float duckDuration = 0.15f;

    private Coroutine _routine;

    public void Duck()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(DuckRoutine());
    }

    private System.Collections.IEnumerator DuckRoutine()
    {
        mixer.SetFloat(musicVolumeParam, duckVolumeDb);
        yield return new WaitForSeconds(duckDuration);
        mixer.SetFloat(musicVolumeParam, normalVolumeDb);
        _routine = null;
    }
}
