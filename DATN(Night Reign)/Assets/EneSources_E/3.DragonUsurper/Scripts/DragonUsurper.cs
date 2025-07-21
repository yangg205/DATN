using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DragonUsurper : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHP = 150f;
    public float HP;
    private bool isDead = false;

    [Header("UI")]
    public Slider healthSlider;
    public Slider easeHealthSlider;

    public TextMeshProUGUI hpText;
    private float lerpSpeed = 0.05f;


    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private WaypointHolder waypointHolder;
    [SerializeField] private Transform fireSpawnPoint;

    [Header("VFX")]
    [SerializeField] private ParticleSystem fireBreathVFX;
    [SerializeField] private ParticleSystem deathVFX;


    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float rotationSpeed = 7.5f;
    [SerializeField] private float circleDuration = 5f;
    [SerializeField] private float waypointDistanceThreshold = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 35f;
    [SerializeField] private float attackDuration = 5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;


    private Transform currentWaypointTarget;
    private Transform[] waypoints;
    [SerializeField] private Collider fireBreathCollider;


    private void Start()
    {
        if (deathVFX && deathVFX.isPlaying)
            deathVFX.Stop();

        HP = maxHP;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        //waypointHolder = FindObjectOfType<WaypointHolder>();
        waypointHolder = FindAnyObjectByType<WaypointHolder>();

        fireBreathVFX = fireSpawnPoint.GetComponentInChildren<ParticleSystem>();
        StopFireBreath();

        if (waypointHolder != null)
        {
            waypointHolder.RefreshWaypoints();
            waypoints = waypointHolder.Waypoints;
        }

        if (waypoints != null && waypoints.Length > 0)
            StartCoroutine(StateMachine());
    }

    private void Update()
    {
        if (healthSlider.value != HP)
        {
            healthSlider.value = HP;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, HP, lerpSpeed);
        }

        hpText.text = $"{HP}";

    }

    public void ActionFlame()
    {
        if (fireBreathVFX && !fireBreathVFX.isPlaying)
            fireBreathVFX.Play();
    }

    private void StartFireBreath()
    {
        /*if (fireBreathVFX && !fireBreathVFX.isPlaying)
            fireBreathVFX.Play();*/

        if (fireBreathCollider)
            fireBreathCollider.enabled = true;
    }

    private void StopFireBreath()
    {
        if (fireBreathVFX && fireBreathVFX.isPlaying)
            fireBreathVFX.Stop();

        if (fireBreathCollider)
            fireBreathCollider.enabled = false;
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void MoveTowardsTarget(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;
        FaceTarget(targetPos);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    private float DistanceToPlayer()
    {
        if (!player) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }

    private bool ReachedWaypoint()
    {
        if (!currentWaypointTarget) return false;
        return Vector3.Distance(transform.position, currentWaypointTarget.position) < waypointDistanceThreshold;
    }

    private void PickRandomWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        currentWaypointTarget = waypoints[Random.Range(0, waypoints.Length)];
    }


    private IEnumerator IdleState(float duration)
    {
        animator.Play("Fly Float 0");

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator CircleState(float duration)
    {
        float timer = 0f;
        PickRandomWaypoint();

        animator.SetBool("isPatrolling", true);
        animator.SetBool("isChasing", false);

        while (timer < duration)
        {
            timer += Time.deltaTime;

            if (currentWaypointTarget)
            {
                MoveTowardsTarget(currentWaypointTarget.position);
                if (ReachedWaypoint()) PickRandomWaypoint();
            }

            yield return null;
        }

        animator.SetBool("isPatrolling", false);
    }

    private IEnumerator AttackState(float duration)
    {
        animator.SetBool("isChasing", true);
        animator.SetTrigger("isAttacking");

        StartFireBreath();

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;

            FaceTarget(player.position);

            if (DistanceToPlayer() > attackRange)
                MoveTowardsTarget(player.position);

            yield return null;
        }

        StopFireBreath();
        animator.SetBool("isChasing", false);
    }

    private IEnumerator StateMachine()
    {
        while (true)
        {
            if (DistanceToPlayer() > attackRange + 20f)
            {
                yield return IdleState(3f);
            }
            else
            {
                yield return CircleState(circleDuration);
                yield return AttackState(attackDuration);
            }
        }
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        HP -= damage;

        HP = Mathf.Clamp(HP, 0, maxHP);

        if (HP <= 0f)
        {
            HP = 0f;
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        StopAllCoroutines(); 


        StopFireBreath(); 

        animator.SetTrigger("dead");


        StartCoroutine(PlayDeathVFXAfterDelay(1f));

        Destroy(gameObject, 3f);
    }

    private IEnumerator PlayDeathVFXAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (deathVFX)
            deathVFX.Play();
    }

}
