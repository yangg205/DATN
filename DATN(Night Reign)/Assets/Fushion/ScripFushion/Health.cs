using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    private int hea;
    private int maxHea;
    private Slider healthSlider;
    private DuyPlayerMovement player;

    public override void Spawned()
    {
        // Tìm Slider có tên "Health" trong Hierarchy
        healthSlider = GameObject.Find("Health").GetComponent<Slider>();
    }

    private void Start()
    {
        // Thiết lập giá trị máu ban đầu
        maxHea = 100;
        hea = maxHea;
        healthSlider.maxValue = maxHea;
        healthSlider.value = hea;

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RpcHealth(int damage)
    {
        // Trừ máu và đảm bảo không dưới 0
        hea -= damage;
        hea = Mathf.Clamp(hea, 0, maxHea);

        // Cập nhật Slider
        UpdateHealthUI();


    }

    private void UpdateHealthUI()
    {
        // Cập nhật giá trị của slider
        if (healthSlider != null)
        {
            healthSlider.value = hea;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            // Gọi RPC để giảm máu
            RpcHealth(20);
            player.TakeDamage();
        }
    }
}
