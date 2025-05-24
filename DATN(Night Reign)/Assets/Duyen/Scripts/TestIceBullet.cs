using UnityEngine;

public class TestIceBullet : MonoBehaviour
{
   
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DragonNightMare"))
        {
            NightMare enemy = other.GetComponent<NightMare>();
            if (enemy != null)
            {
                enemy.TakeIceDamage(1);
            }

        }
    }
}
