using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class desertBoss : MonoBehaviour
{
    [Header("Fire attack")]
    [SerializeField] private ParticleSystem fireBreathVFX;
    [SerializeField] private Transform fireSpawnPoint;
    [SerializeField] private Collider fireBreathCollider;

    [SerializeField] private GameObject meleeAttackHitboxeR;
    [SerializeField] private GameObject meleeAttackHitboxeL;


    [Header("SFX")]
    public AudioSource sfxGrowl;
    public AudioSource sfxHitBox;
    public AudioSource sfxHitBox1;
    public AudioSource sfxHurt;
    public AudioSource sfxDead;

    [Header("UI")]
    public Slider healthSlider;
    public Slider easeHealthSlider;

    public TextMeshProUGUI hpText;
    private float lerpSpeed = 0.05f;


    public Animator animator;
    private Transform targetPlayer;

    public float detectRange = 15f;
    public float meleeRange = 5f;
    public Transform rangeOrigin;


    public int maxHP = 1000;
    public int currentHP;

    private bool isAttacking = false;
    private bool isDead = false;

    private bool isPhase2 => currentHP <= 500;

    private bool hasShoutedPhase2 = false;
    private bool isShouting = false;
    private float attackWindupTime = 0.3f;

    public static bool IsPaused = false;

    void Awake()
    {
        GameObject healthSliderObj = GameObject.Find("HealthBarSaMac");
        if (healthSliderObj != null)
            healthSlider = healthSliderObj.GetComponent<Slider>();

        GameObject easeSliderObj = GameObject.Find("EaseHealthBarSaMac");
        if (easeSliderObj != null)
            easeHealthSlider = easeSliderObj.GetComponent<Slider>();

        GameObject hpTextObj = GameObject.Find("HPTextSaMac");
        if (hpTextObj != null)
            hpText = hpTextObj.GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("ko tim thay taf Player");
            }
        }


        meleeAttackHitboxeR.SetActive(false);
        meleeAttackHitboxeL.SetActive(false);

        fireBreathVFX = fireSpawnPoint.GetComponentInChildren<ParticleSystem>();


        currentHP = maxHP;


        animator.SetTrigger("isEmerging");
        StartCoroutine(AIBehavior());
    }

    private void Update()
    {
        if(IsPaused) return;

        if (healthSlider.value != currentHP)
        {
            healthSlider.value = currentHP;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHP, lerpSpeed);
        }

        hpText.text = $"{currentHP}";

        if (!isDead && !isAttacking && !isShouting)
        {
            LookAtPlayer(); 
        }

        //if(Input.GetKeyDown(KeyCode.E))
        //{
        //    TakeDamage(450);
        //}
    }

    private IEnumerator AIBehavior()
    {
        yield return new WaitForSeconds(3f); // Emergence

        while (!isDead)
        {
            Vector3 origin = rangeOrigin != null ? rangeOrigin.position : transform.position;
            float distance = Vector3.Distance(origin, targetPlayer.position);
            bool inRange = distance <= detectRange;

            // ▶️ Phase 2: Shout
            if (isPhase2 && !hasShoutedPhase2)
            {
                isShouting = true;
                hasShoutedPhase2 = true;
                animator.SetTrigger("doShout");
                yield return new WaitForSeconds(2f); // thời gian anim Shout
                isShouting = false;
            }

            if (inRange && !isAttacking && !isShouting)
            {
                isAttacking = true;
                int attackType = DecideAttackType(distance);
                animator.SetInteger("doAttackType", attackType);

                float elapsed = 0f;
                while (elapsed < attackWindupTime)
                {
                    LookAtPlayer();
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                yield return AttackCooldown();
                animator.SetInteger("doAttackType", 0); // reset
                isAttacking = false;
            }

            yield return null;
        }
    }

    private int DecideAttackType(float distance)
    {
        if (isPhase2 && distance > meleeRange)
        {
            return Random.Range(3, 5);
        }

        // random melee attacks
        return Random.Range(1, 4); // 1 to 3
    }


    private IEnumerator AttackCooldown()
    {
        float cooldown = Random.Range(4f, 6f);
        yield return new WaitForSeconds(cooldown);
    }
    private void LookAtPlayer()
    {
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        direction.y = 0; // không xoay theo trục Y

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    // Gọi khi animation bắt đầu phun lửa (qua Animation Event)
    public void StartFireDesertBoss()
    {
        if (fireBreathVFX && !fireBreathVFX.isPlaying)
            fireBreathVFX.Play();

        if (fireBreathCollider)
            fireBreathCollider.enabled = true;
    }
    public void StopFireDesertBoss()
    {
        if (fireBreathVFX && fireBreathVFX.isPlaying)
            fireBreathVFX.Stop();

        if (fireBreathCollider)
            fireBreathCollider.enabled = false;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        animator.SetTrigger("takeHit");
        sfxHurt.PlayOneShot(sfxHurt.clip);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("isDead");
        // Disable collider, disable attack, etc.
    }


    //private void OnDrawGizmosSelected()
    //{
    //    Vector3 origin = rangeOrigin != null ? rangeOrigin.position : transform.position;

    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(origin, detectRange);

    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(origin, meleeRange);
    //}
    public void startHitBoxR()
    {
        meleeAttackHitboxeR.SetActive(true);
    }
    public void startHitBoxL()
    {
        meleeAttackHitboxeL.SetActive(true);
    }

    public void endHitBoxR()
    {
        meleeAttackHitboxeR.SetActive(false);
    }

    public void endHitBoxL()
    {
        meleeAttackHitboxeL.SetActive(false);
    }

    //====2hand
    public void startHitBoxRL()
    {
        meleeAttackHitboxeR.SetActive(true);
        meleeAttackHitboxeL.SetActive(true);
    }

    public void endHitBoxRL()
    {
        meleeAttackHitboxeR.SetActive(false);
        meleeAttackHitboxeL.SetActive(false);
    }

    public void PlayGrowlSFXSaMac()
    {
        if (sfxGrowl != null && !sfxGrowl.isPlaying)
        {
            sfxGrowl.PlayOneShot(sfxGrowl.clip);
        }
    }
    public void PlayHitBoxSFXSaMac()
    {
        if (sfxHitBox != null && !sfxHitBox.isPlaying)
        {
            sfxHitBox.PlayOneShot(sfxHitBox.clip);
        }
    }

    public void PlayHitBox1SFXSaMac()
    {
        if (sfxHitBox1 != null && !sfxHitBox1.isPlaying)
        {
            sfxHitBox1.PlayOneShot(sfxHitBox1.clip);
        }
    }
    public void PlayDeadSFXSaMac()
    {
        if (sfxDead != null && !sfxDead.isPlaying)
        {
            sfxDead.PlayOneShot(sfxDead.clip);
        }
    }
}
