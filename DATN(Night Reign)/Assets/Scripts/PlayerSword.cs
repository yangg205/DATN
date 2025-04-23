using Fusion;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PlayerSword : NetworkBehaviour
{
    [Header("Chém kiếm")]
    public float attackRange = 2f;
    public float attackDamage = 25f;

    [Header("Hiệu ứng")]
    public GameObject slashEffectPrefab;
    public AudioClip slashSFX;
    private AudioSource audioSource;

    public Transform attackPoint; // Điểm chém (thường là trước mặt player)
    public Animator animator; // Animator của nhân vật
    public NetworkRunner networkRunner;

    private void Start()
    {
        if (networkRunner == null)
            networkRunner = Runner;

        // Tạo AudioSource nếu chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (!Object.HasInputAuthority) return;

        // Kiểm tra phím tấn công (chém)
        if (Input.GetKeyDown(KeyCode.F))
        {
            Slash();
        }

        // Kiểm tra nếu animation đang là SwordSlash và đã kết thúc
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Attack") &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            // Khi animation chém kết thúc, trở về trạng thái Idle hoặc Run
            animator.SetBool("isAttacking", false);
        }
    }

    private void Slash()
    {
        // Kích hoạt animation chém
        animator.SetTrigger("Slash");  // Trigger animation chém
        animator.SetBool("isAttacking", true); // Đặt IsAttacking thành true khi đang tấn công

        // Kiểm tra các đối tượng trong phạm vi chém
        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRange);
        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Enemy")) // Kiểm tra nếu là địch
            {
                EnemyAI enemy = collider.GetComponent<EnemyAI>();
                if (enemy != null)
                {
                    enemy.TakeDamage((int)attackDamage); // Gọi hàm nhận sát thương từ EnemyAI
                }
            }
        }

        // Gọi hiệu ứng & âm thanh
        PlaySlashEffect_RPC();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void PlaySlashEffect_RPC()
    {
        // Hiệu ứng chém
        if (slashEffectPrefab != null && attackPoint != null)
        {
            GameObject vfx = Instantiate(slashEffectPrefab, attackPoint.position, attackPoint.rotation);
            Destroy(vfx, 2f);
        }

        // Âm thanh chém
        if (slashSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(slashSFX);
        }
    }

    // Vẽ gizmo để dễ nhìn phạm vi chém trong editor
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    // Sửa lại phần này để gửi thông tin cho quái về trạng thái tấn công
    public void OnAttack()
    {
        foreach (var enemy in FindObjectsOfType<EnemyAI>())
        {
            enemy.OnAttackEnter(true);
        }
    }

    public void OnAttackEnd()
    {
        foreach (var enemy in FindObjectsOfType<EnemyAI>())
        {
            enemy.OnAttackEnter(false);
        }
    }
}
