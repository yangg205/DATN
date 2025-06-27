using ND;
using UnityEngine;

public class Hit : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to an enemy
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("dang gay dame cho player");
        }
    }
}
