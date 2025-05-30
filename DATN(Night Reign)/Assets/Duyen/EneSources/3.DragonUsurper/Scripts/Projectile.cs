using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
    }
}
