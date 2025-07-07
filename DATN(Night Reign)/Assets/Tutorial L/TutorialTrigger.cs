using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public TutorialData data;
    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && other.CompareTag("Player") && data != null)
        {
            TutorialManager.Instance.ShowMessage(data, this);
            hasTriggered = true;
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public bool ShouldDeactivateAfterShown()
    {
        return data != null && data.onlyTriggerOnce;
    }
}
