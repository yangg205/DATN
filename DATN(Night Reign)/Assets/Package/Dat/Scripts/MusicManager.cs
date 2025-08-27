using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    public AudioClip CurrentClip => audioSource.clip;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        if (audioSource.clip == clip) return;

        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = 1f;
        audioSource.Play();
    }

    public void FadeOutMusic(float duration)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeOutRoutine(duration));
    }

    private IEnumerator FadeOutRoutine(float duration)
    {
        float startVolume = audioSource.volume;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = 1f;
    }
}
