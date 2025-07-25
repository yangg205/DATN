using ND;
using UnityEngine;

public class ArrowDamageCollider : MonoBehaviour
{
    public int damage = 25;
    public float speed = 20f;
    public float destroyAfterSeconds = 5f;

    private Vector3 direction;

    private void Start()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    public void SetDamage(int value)
    {
        damage = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyStats>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        else if (!other.CompareTag("Player")) // Không tự hủy khi va chạm với player
        {
            Destroy(gameObject);
        }
    }
}
