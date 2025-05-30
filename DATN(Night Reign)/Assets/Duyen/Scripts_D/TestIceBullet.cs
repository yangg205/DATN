using UnityEngine;

public class TestIceBullet : MonoBehaviour
{
   
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DragonNightMare") || other.CompareTag("DragonSoulEater"))
        {
            NightMare enemy = other.GetComponent<NightMare>();
            if (enemy != null)
            {
                enemy.TakeIceDamage(1);
            }
            DragonSoulEater enemy1 = other.GetComponent<DragonSoulEater>();
            if (enemy1 != null)
                enemy1.TakeIceDamage(1);
        }
    }
}
