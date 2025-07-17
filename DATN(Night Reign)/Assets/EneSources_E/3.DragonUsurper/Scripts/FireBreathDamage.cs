using UnityEngine;

public class FireBreathDamage : MonoBehaviour
{
    [SerializeField] private float damagePerSecond = 20f;
    private Transform player;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {

       }
    }
}
