using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip defaultFootstepClip;
    public AudioClip sandFootstepClip; // thêm âm thanh bước chân trên cát
    public float stepRate = 0.5f;
    private float stepTimer;

    void Update()
    {
        if (IsWalking()) // Logic detect nhân vật đang di chuyển
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepRate;
            }
        }
    }

    public void PlayFootstep()
    {
        AudioClip clipToPlay = defaultFootstepClip;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            var mat = hit.collider.sharedMaterial;
            if (mat != null && mat.name == "SandMaterial") // đúng tên file PhysicMaterial
            {
                clipToPlay = sandFootstepClip;
            }
        }

        audioSource.PlayOneShot(clipToPlay);
        Debug.Log("PlayFootstep triggered at " + Time.time);
    }

    bool IsWalking()
    {
        // Kiểm tra input, velocity, grounded... tùy controller
        return Input.GetAxis("Vertical") != 0 || Input.GetAxis("Horizontal") != 0;
    }
}
