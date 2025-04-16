using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    public AudioClip clickSound;
    private AudioSource audioSource;

    void Awake()
    {
        // Tạo AudioSource nếu chưa có
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(PlayClickSound);
    }

    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
