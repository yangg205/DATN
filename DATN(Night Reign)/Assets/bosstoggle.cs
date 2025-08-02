using UnityEngine;

public class bosstoggle : MonoBehaviour
{
    public GameObject boss;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boss.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("bossbox"))
        {
            boss.SetActive(true);
            Destroy(gameObject); // Xóa đối tượng này sau khi kích hoạt boss
        }
    }
}
