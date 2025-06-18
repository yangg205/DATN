using UnityEngine;

public class TriggerSandau : MonoBehaviour
{
    public Animator sandauAnimator; // Kéo Sandau vào trong Inspector
    public string animationTriggerName = "SandauAnimation"; // Tên trigger đặt trong Animator

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            sandauAnimator.SetTrigger(animationTriggerName);
        }
    }
}
