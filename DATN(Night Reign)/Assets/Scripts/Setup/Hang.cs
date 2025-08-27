using UnityEngine;
using UnityEngine.SceneManagement;

public class Hang : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene("SceneBoss");
        }
    }
}
