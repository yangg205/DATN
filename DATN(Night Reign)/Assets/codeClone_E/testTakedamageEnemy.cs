using UnityEngine;

public class testTakedamageEnemy : MonoBehaviour
{
    public int damageAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DragonNightMare"))
        {
            NightMare nightmare = other.GetComponent<NightMare>();
            if (nightmare != null)
            {
                nightmare.TakeDamage(damageAmount);
            }
            else
            {
                Debug.LogWarning("NightMare script not found on object with tag Enemy.");
            }
        }
    }
}
