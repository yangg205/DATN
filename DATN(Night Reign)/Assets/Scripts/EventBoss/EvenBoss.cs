using Unity.VisualScripting;
using UnityEngine;

public class EvenBoss : MonoBehaviour
{
    public GameObject bossPrefab;
    public GameObject Box;
    void Start()
    {
        bossPrefab.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BoxEvent"))
        {
            bossPrefab.gameObject.SetActive(true);
            Box.gameObject.SetActive(false);
            Debug.Log("BoxEvent collision detected. Boss activated and box deactivated.");
            Destroy(gameObject, 1f); // Destroy the box after 1 second
        }
    }
}
