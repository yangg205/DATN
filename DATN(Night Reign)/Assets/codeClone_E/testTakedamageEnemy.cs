using UnityEngine;

public class testTakedamageEnemy : MonoBehaviour
{
    public int damageAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DragonSoulEater"))
        {
            DragonSoulEater ene = other.GetComponent<DragonSoulEater>();
            if (ene != null)
            {
                ene.TakeDamage(damageAmount);
            }
         
        }
    }
}
