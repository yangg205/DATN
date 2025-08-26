using UnityEngine;

public class OrderedLightSound : MonoBehaviour
{
    public Light[] lights;              // Kéo 8 đèn theo thứ tự
    public AudioClip switchOnClip;      // Âm thanh bật chung

    private AudioSource audioSource;
    private int currentIndex = 0;       // Đèn nào tới lượt
    private bool[] lastStates;          // Trạng thái ban đầu

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Lưu trạng thái ban đầu của tất cả đèn
        lastStates = new bool[lights.Length];
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lastStates[i] = lights[i].enabled;
        }
    }

    void Update()
    {
        if (currentIndex < lights.Length)
        {
            Light l = lights[currentIndex];
            if (l == null) return;

            // Khi trạng thái từ tắt -> bật thì mới phát
            if (!lastStates[currentIndex] && l.enabled)
            {
                audioSource.PlayOneShot(switchOnClip);
                currentIndex++; // Chuyển sang đèn tiếp theo
            }

            // Cập nhật trạng thái hiện tại
            lastStates[currentIndex] = l.enabled;
        }
    }
}
