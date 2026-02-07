using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 0f; // 2D
        audioSource.playOnAwake = false;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        audioSource.pitch = pitch;
        audioSource.PlayOneShot(clip, volume);
    }

    public void PlaySFXSequence(AudioClip[] clips, float[] delays, float volume = 1f)
    {
        StartCoroutine(PlaySequenceCoroutine(clips, delays, volume));
    }

    private IEnumerator PlaySequenceCoroutine(AudioClip[] clips, float[] delays, float volume)
    {
        for (int i = 0; i < clips.Length; i++)
        {
            PlaySFX(clips[i], volume);
            if (delays != null && i < delays.Length)
                yield return new WaitForSeconds(delays[i]);
        }
    }
}
