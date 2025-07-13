/*using System.Collections;
using UnityEngine;

public class DragonUsurper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private WaypointHolder waypointHolder;
    //[SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    [Header("VFX")]
    [SerializeField] private ParticleSystem fireBreathVFX;


    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float rotationSpeed = 7.5f;
    [SerializeField] private float circleDuration = 5f;
    [SerializeField] private float waypointDistanceThreshold = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 35f;
    [SerializeField] private float attackDuration = 5f;
    [SerializeField] private float shootInterval = 1f;

    //[Header("Projectile Settings")]
    //[SerializeField] private float projectileSpeed = 60f;
    //[SerializeField] private float angleToShootAtPlayer = 0.1f;

    [SerializeField] private Animator animator;


    private Transform currentWaypointTarget;
    private Transform[] waypoints;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        fireBreathVFX = projectileSpawnPoint.GetComponentInChildren<ParticleSystem>();

        StopFireBreath();

        if (waypointHolder != null)
        {
            waypointHolder.RefreshWaypoints();
            waypoints = waypointHolder.Waypoints;
        }

        if (waypoints == null || waypoints.Length == 0) return;

        StartCoroutine(StateMachine());
    }
    private void StartFireBreath()
    {
        if (fireBreathVFX && !fireBreathVFX.isPlaying)
            fireBreathVFX.Play();
    }

    private void StopFireBreath()
    {
        if (fireBreathVFX && fireBreathVFX.isPlaying)
            fireBreathVFX.Stop();
    }

    private void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        dir.Normalize();
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
    }

    private bool IsFacingPlayer(float angleThreshold)
    {
        if (!player) return true;
        Vector3 toPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        return angle <= angleThreshold;
    }

    private IEnumerator RotateUntilFacingPlayer(float angleThreshold)
    {
        while (!IsFacingPlayer(angleThreshold))
        {
            FaceTarget(player.position);
            yield return null;
        }
    }

    private void PickRandomWaypoint()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            currentWaypointTarget = waypoints[Random.Range(0, waypoints.Length)];
        }
    }

    private bool ReachedWaypoint()
    {
        if (!currentWaypointTarget) return false;
        return Vector3.Distance(transform.position, currentWaypointTarget.position) < waypointDistanceThreshold;
    }

    private void MoveTowardsTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        dir.Normalize();
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * rotationSpeed
        );
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    private float DistanceToPlayer()
    {
        if (!player) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }

    //private void FireProjectile()
    //{
    //    if (!projectilePrefab) return;
    //    var spawn = projectileSpawnPoint ? projectileSpawnPoint : transform;

    //    var proj = Instantiate(projectilePrefab, spawn.position, spawn.rotation);
    //    var rb = proj.GetComponent<Rigidbody>();

    //    if (rb)
    //        rb.linearVelocity = spawn.forward * projectileSpeed;
    //}

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
                MoveTowardsTarget(currentWaypointTarget.position);

            if (ReachedWaypoint()) PickRandomWaypoint();

            yield return null;
        }
        animator.SetBool("isPatrolling", false);

    }

    *//*private IEnumerator AttackState(float duration)
    {
        yield return StartCoroutine(RotateUntilFacingPlayer(angleToShootAtPlayer));
        FireProjectile();

        float timer = 0f;
        float shootTimer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            shootTimer += Time.deltaTime;

            FaceTarget(player.position);

            if (DistanceToPlayer() > attackRange)
            {
                MoveTowardsTarget(player.position);
            }

            if (shootTimer >= shootInterval)
            {
                shootTimer = 0f;
                FireProjectile();
            }
            yield return null;
        }
    }*//*
    private IEnumerator AttackState(float duration)
    {
        animator.SetBool("isChasing", true);

        //yield return StartCoroutine(RotateUntilFacingPlayer(angleToShootAtPlayer));
        animator.SetTrigger("isAttacking");

        StartFireBreath(); // 🔥 Bắt đầu phun lửa

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            FaceTarget(player.position);

            if (DistanceToPlayer() > attackRange)
            {
                MoveTowardsTarget(player.position);
            }

            yield return null;
        }

        StopFireBreath(); // ❌ Dừng phun lửa
        animator.SetBool("isChasing", false);
    }

    private IEnumerator StateMachine()
    {
        while (true)
        {
            if (DistanceToPlayer() > attackRange + 20f)
            {
                yield return StartCoroutine(IdleState(3f));
            }
            else
            {
                yield return StartCoroutine(CircleState(circleDuration));
                yield return StartCoroutine(AttackState(attackDuration));
            }
        }
    }

    private IEnumerator IdleState(float duration)
    {
        //animator.Play("Fly_Idle");
        animator.Play("Fly Float 0");

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }

}
*/

using Pathfinding;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DragonUsurper : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHP = 150f;
    public float HP;
    private bool isDead = false;

    [Header("UI")]
    public Image healthFill;

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

        waypointHolder = FindObjectOfType<WaypointHolder>();

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
        if (healthFill != null)
        {
            healthFill.fillAmount = HP / maxHP;
        }
        
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
