using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class SimpleEnemyAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    [Tooltip("Ambient sound clips the enemy will randomly play.")]
    public AudioClip[] ambientClips;

    [Header("Settings")]
    [Tooltip("Minimum time between sounds (seconds).")]
    public float minDelay = 1.5f;
    [Tooltip("Maximum time between sounds (seconds).")]
    public float maxDelay = 4.0f;

    [Range(0f, 1f)]
    public float volume = 1.0f;

    private AudioSource _source;
    private Coroutine _loopRoutine;

    void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.loop = false;
        _source.playOnAwake = false;
    }

    void OnEnable()
    {
        if (_loopRoutine == null)
            _loopRoutine = StartCoroutine(PlayRandomClips());
    }

    void OnDisable()
    {
        if (_loopRoutine != null)
            StopCoroutine(_loopRoutine);
    }

    IEnumerator PlayRandomClips()
    {
        if (ambientClips == null || ambientClips.Length == 0)
            yield break;

        while (true)
        {
            AudioClip clip = ambientClips[Random.Range(0, ambientClips.Length)];
            _source.PlayOneShot(clip, volume);

            // Wait for clip duration + small random delay
            float waitTime = clip.length + Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
