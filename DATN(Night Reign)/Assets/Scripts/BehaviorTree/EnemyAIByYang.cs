using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAIByYang : MonoBehaviour
{
    [Header("Target & Detection")]
    public Transform targetPlayer;
    public float detectionRange = 15f;
    [SerializeField] private float endReachedDistance = 7f;
    [SerializeField] private float engageRange = 14f;
    public float approachDistance = 10f;
    public float meleeAttackRange = 6f;

    [Header("Health & Phases")]
    public float maxHealth = 1000f;
    public float currentHealth = 1000f;
    public float phase2HealthThreshold = 500f;

    [Header("SFX")]
    public AudioSource sfxCrawl;
    public AudioSource sfxGrowl;
    public AudioSource sfxAttack;
    public AudioSource sfx2Attack;
    public AudioSource sfxDead;

    [Header("Movement")]
    public float movementSpeed = 4f; // Tốc độ di chuyển giữ nguyên
    private float baseMovementSpeed; // lưu speed gốc để tính cộng thêm
    public float dodgeSpeed = 8f;
    public float rotationSpeed = 10f;
    public float _fleeRange = 8f;

    [Header("Attack & Cooldowns")]
    public float attackCooldown = 1.5f;
    private float phase2AttackCooldown = 0.8f;
    private float _lastAttackTime;
    public float dodgeCooldown = 3f;
    private float _lastDodgeTime;

    [Header("Damage & Hitboxes")]
    //public GameObject meleeAttackHitbox;
    [SerializeField] private List<GameObject> meleeAttackHitboxes = new List<GameObject>();

    public float meleeDamage = 50f;

    private Node _rootNode;
    private Animator _animator;
    private AIPath _aiPath;
    private Seeker _seeker;
    private CharacterController _characterController;

    private float delayBeforeStart = 10f;
    private float spawnTime;
    private bool isAIActivated = false;

    [Header("VFX")]
    public ParticleSystem vfxHitEffect;
    public enum BossPhase
    {
        Phase1,
        Phase2
    }
    public BossPhase currentPhase = BossPhase.Phase1;

    private bool _isAttacking = false;
    private bool _isDodging = false;
    private bool _isStaggered = false;
    private bool _isParrying = false;
    private bool _isEnraged = false;
    private bool _isCurrentlyFleeing = false;
    private bool _playerDodgedBossSkill = false;
    public bool IsEnraged => _isEnraged;


    void Awake()
    {
        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
                Debug.Log("Found Player with tag 'Player'.");
            }
            else
            {
                Debug.LogWarning("Player object with 'Player' tag not found. Please assign targetPlayer manually or ensure Player has the correct tag.");
            }
        }
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        _aiPath = GetComponent<AIPath>();
        _seeker = GetComponent<Seeker>();
        _characterController = GetComponent<CharacterController>();

        if (_animator == null) { Debug.LogError("Animator component not found."); enabled = false; return; }
        if (_aiPath == null) { Debug.LogError("AIPath component not found."); enabled = false; return; }
        if (_seeker == null) { Debug.LogError("Seeker component not found."); enabled = false; return; }
        if (_characterController == null) { Debug.LogError("CharacterController component not found."); enabled = false; return; }
        
        _aiPath.maxSpeed = movementSpeed;
        _aiPath.enableRotation = false;

        baseMovementSpeed = movementSpeed;

        if (meleeAttackHitboxes != null)
        {
            foreach (var hitbox in meleeAttackHitboxes)
                hitbox.SetActive(false);
        }

        spawnTime = Time.time;

        _aiPath.endReachedDistance = endReachedDistance;
        _aiPath.slowdownDistance = engageRange;


        SetupBehaviorTree();

        if (EnemyAIManager.Instance != null)
        {
            EnemyAIManager.Instance.RegisterEnemy(this);
            Debug.Log("Boss registered with EnemyAIManager.");
        }
        else
        {
            Debug.LogWarning("EnemyAIManager instance not found.");
        }

        _lastAttackTime = -attackCooldown;
        _lastDodgeTime = -dodgeCooldown;
    }

    private void Update()
    {
        Tick();

        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(100);
        }
    }

    public void Tick()
    {
        if (!isAIActivated)
        {
            if (Time.time - spawnTime >= delayBeforeStart)
            {
                isAIActivated = true;
                Debug.Log("Boss AI Activated after delay!");
            }
            else
            {
                _aiPath.isStopped = true;
                _animator?.SetBool("isMoving", false);
                _animator?.SetFloat("Speed", 0f);
                return;
            }
        }

        float distToPlayer = targetPlayer != null ? Vector3.Distance(transform.position, targetPlayer.position) : -1f;
        //Debug.Log($"Phase: {currentPhase}, IsEnraged: {_isEnraged}, IsFleeing: {_isCurrentlyFleeing}, DistanceToPlayer: {distToPlayer}, AIPathStopped: {_aiPath.isStopped}");

        if (_rootNode != null)
        {
            _rootNode.Evaluate();
        }

        if (targetPlayer != null && !IsBossBusy())
        {
            LookAtTarget(targetPlayer.position);
        }

        _aiPath.maxSpeed = movementSpeed;

        if (_aiPath.desiredVelocity.magnitude > 0.1f && !_isAttacking && !_isDodging && !_isStaggered && !_isCurrentlyFleeing)
        {
            _animator?.SetBool("isMoving", true);
            _animator?.SetFloat("Speed", _isEnraged ? 2f : 1f); // Tốc độ animation
        }
        else
        {
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
        }

        if (_aiPath.desiredVelocity.magnitude > 0.1f && !_isAttacking && !_isDodging)
        {
            _animator?.SetBool("isMoving", true);
            _animator?.SetFloat("Speed", _isEnraged ? 2f : 1f); // Tốc độ animation

            if (!sfxCrawl.isPlaying)
                sfxCrawl.Play();
        }
        else
        {
            if (sfxCrawl.isPlaying)
                sfxCrawl.Stop();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (vfxHitEffect != null)
        {
            vfxHitEffect.Play();
        }

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (currentHealth <= phase2HealthThreshold && currentPhase == BossPhase.Phase1)
        {
            TransitionToPhase(BossPhase.Phase2);
        }
    }

    private void Die()
    {
        Debug.Log("Boss has been defeated!");
        _animator?.SetTrigger("Die");
        _aiPath.isStopped = true;
        enabled = false;

        if (EnemyAIManager.Instance != null)
        {
            EnemyAIManager.Instance.UnregisterEnemy(this);
        }
    }

    private void TransitionToPhase(BossPhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log($"Boss transitioning to {newPhase} at {currentHealth} HP.");

        if (newPhase == BossPhase.Phase2)
        {
            _isEnraged = true;
            attackCooldown = phase2AttackCooldown;

            movementSpeed = baseMovementSpeed + 1.1f;
            _aiPath.maxSpeed = movementSpeed;


            StartCoroutine(PerformPhase2AnimationSequence()); // Bắt đầu chuỗi animation ở Phase 2
            Debug.Log("Boss is ENRAGED! Faster animation and new animation sequence!");
        }
        SetupBehaviorTree();
    }

    private IEnumerator PerformPhase2AnimationSequence()
    {
        while (currentPhase == BossPhase.Phase2 && currentHealth > 0)
        {
            if (!IsBossBusy() && IsPlayerInMeleeAttackRange())
            {
                PerformPhase2Animation();
                yield return new WaitForSeconds(phase2AttackCooldown);
            }
            else
            {
                yield return null; // Chờ frame tiếp theo nếu bận hoặc không trong tầm
            }
        }
    }

    private void PerformPhase2Animation()
    {
        if (_animator == null || targetPlayer == null || !IsPlayerInMeleeAttackRange()) return;

        _isAttacking = true;
        _lastAttackTime = Time.time;
        _aiPath.isStopped = true;
        _animator?.SetBool("isMoving", false);
        _animator?.SetFloat("Speed", 0f);

        int animationChoice = UnityEngine.Random.Range(1, 5);
        switch (animationChoice)
        {
            case 1:
                _animator?.SetTrigger("MeleeAttack1Speed");
                break;
            case 2:
                _animator?.SetTrigger("MeleeAttack3Speed");
                break;
            case 3:
                _animator?.SetTrigger("Jump");
                break;
            case 4:
                _animator?.SetTrigger("MeleeAttack4Speed");
                break;
        }

        Debug.Log($"Boss: Performing Phase 2 Animation {animationChoice}!");
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private bool IsPlayerInDetectionRange()
    {
        if (targetPlayer == null) return false;
        return Vector3.Distance(transform.position, targetPlayer.position) <= detectionRange;
    }

    private bool IsPlayerInMeleeAttackRange()
    {
        if (targetPlayer == null) return false;
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        return distance <= meleeAttackRange;
    }

    private bool IsPlayerAtEngageDistance()
    {
        if (targetPlayer == null) return false;
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        return distance > meleeAttackRange && distance <= engageRange;
    }

    private bool IsLowHP()
    {
        return currentHealth <= phase2HealthThreshold;
    }

    private bool CanDodge()
    {
        return (Time.time >= _lastDodgeTime + dodgeCooldown) && (IsPlayerCurrentlyAttacking() || HasPlayerDodgedBossSkill());
    }

    private bool CanAttack()
    {
        return Time.time >= _lastAttackTime + attackCooldown;
    }

    public bool IsBossBusy()
    {
        return _isAttacking || _isDodging || _isStaggered || _isParrying || _isCurrentlyFleeing;
    }

    private bool IsPlayerCurrentlyAttacking()
    {
        return false; // TODO: Triển khai logic này dựa trên trạng thái của Player
    }

    public void PlayerDodgedBossSkill()
    {
        _playerDodgedBossSkill = true;
        Debug.Log("Player successfully dodged Boss's skill!");
    }

    private bool HasPlayerDodgedBossSkill()
    {
        bool dodged = _playerDodgedBossSkill;
        if (dodged) _playerDodgedBossSkill = false;
        return dodged;
    }

    private bool IsPlayerTooCloseToFlee()
    {
        if (targetPlayer == null) return false;
        if (_isEnraged) return false; // Không bỏ chạy ở Phase 2
        return Vector3.Distance(transform.position, targetPlayer.position) <= _fleeRange;
    }

    private bool IsFleeing()
    {
        return _isCurrentlyFleeing;
    }

    private NodeState MoveTowardsPlayer()
    {
        if (targetPlayer == null)
        {
            _aiPath.isStopped = true;
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
            return NodeState.FAILURE;
        }

        if (IsBossBusy() && !_isCurrentlyFleeing)
        {
            _aiPath.isStopped = true;
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
            return NodeState.FAILURE;
        }

        float currentDistance = Vector3.Distance(transform.position, targetPlayer.position);

        if (currentDistance <= engageRange)
        {
            _aiPath.isStopped = true;
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
            Debug.Log("[MoveTowardsPlayer] Reached engage range or closer. Stopping movement. SUCCESS.");
            return NodeState.SUCCESS;
        }

        _aiPath.destination = targetPlayer.position;
        if (_aiPath.isStopped) _aiPath.isStopped = false;
        _aiPath.maxSpeed = movementSpeed;

        _animator?.SetBool("isMoving", true);
        _animator?.SetFloat("Speed", _isEnraged ? 2f : 1f); // Tốc độ animation

        if (_aiPath.pathPending)
        {
            //Debug.Log("[MoveTowardsPlayer] Path pending... RUNNING.");
            return NodeState.RUNNING;
        }

        if (_aiPath.hasPath && !_aiPath.isStopped)
        {
            //Debug.Log("[MoveTowardsPlayer] Moving towards player... RUNNING.");
            return NodeState.RUNNING;
        }

        _seeker.StartPath(_aiPath.position, targetPlayer.position, null);
        //Debug.LogWarning("[MoveTowardsPlayer] Failed to find path, requesting new path. RUNNING.");
        return NodeState.RUNNING;
    }

    private NodeState PerformMeleeAttack()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (!IsPlayerInMeleeAttackRange()) return NodeState.FAILURE;
        if (IsBossBusy()) return NodeState.FAILURE;
        if (!CanAttack()) return NodeState.FAILURE;
        if (currentPhase == BossPhase.Phase2) return NodeState.FAILURE; // Không dùng ở Phase 2

        Debug.Log("Boss: Performing Phase 1 Melee Attack!");
        _isAttacking = true;
        _lastAttackTime = Time.time;
        _aiPath.isStopped = true;
        _animator?.SetBool("isMoving", false);
        _animator?.SetFloat("Speed", 0f);

        int attackChoice = UnityEngine.Random.Range(1, 4);
        switch (attackChoice)
        {
            case 1:
                _animator?.SetTrigger("MeleeAttack1");
                break;
            case 2:
                _animator?.SetTrigger("MeleeAttack3");
                break;
            case 3:
                _animator?.SetTrigger("MeleeAttack4");
                break;
        }

        return NodeState.RUNNING;
    }

    private NodeState Dodge()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (!CanDodge() || IsBossBusy()) return NodeState.FAILURE;

        Debug.Log("Boss: Dodging!");
        _isDodging = true;
        _lastDodgeTime = Time.time;
        _aiPath.isStopped = true;
        _animator?.SetBool("isMoving", false);
        _animator?.SetFloat("Speed", 0f);
        _animator.applyRootMotion = true;

        int dodgeChoice = UnityEngine.Random.Range(0, 4);
        switch (dodgeChoice)
        {
            case 0:
                _animator?.SetTrigger("Rotate90Left");
                break;
            case 1:
                _animator?.SetTrigger("Rotate90Right");
                break;
            case 2:
                _animator?.SetTrigger("Rotate90LeftBack");
                break;
            case 3:
                _animator?.SetTrigger("Rotate90RightBack");
                break;
        }

        return NodeState.RUNNING;
    }

    private NodeState RecoverFromStagger()
    {
        if (!_isStaggered) return NodeState.FAILURE;

        Debug.Log("Boss: Recovering from Stagger.");
        _aiPath.isStopped = true;
        return NodeState.RUNNING;
    }

    private IEnumerator StaggerRecoveryCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        OnRecoverStaggerAnimationEnd();
    }

    public void StaggerBoss(float duration)
    {
        if (_isStaggered) return;
        _isStaggered = true;
        _aiPath.isStopped = true;
        _animator?.SetBool("isMoving", false);
        _animator?.SetFloat("Speed", 0f);
        Debug.Log($"Boss STAGGERED for {duration} seconds!");
        _animator?.SetTrigger("GetHit");
    }

    private NodeState BlockOrParry()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (!IsPlayerCurrentlyAttacking() || IsBossBusy()) return NodeState.FAILURE;

        Debug.Log("Boss: Attempting Block/Parry!");
        _isParrying = true;
        _aiPath.isStopped = true;
        _animator?.SetBool("isMoving", false);
        _animator?.SetFloat("Speed", 0f);
        return NodeState.RUNNING;
    }

    private NodeState PerformFlee()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (_isEnraged)
        {
            _isCurrentlyFleeing = false;
            _aiPath.isStopped = true;
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
            Debug.Log("Boss: Flee disabled in Phase 2. FAILURE.");
            return NodeState.FAILURE;
        }

        if (_isCurrentlyFleeing && _aiPath.reachedDestination && Vector3.Distance(transform.position, targetPlayer.position) > _fleeRange)
        {
            _isCurrentlyFleeing = false;
            _aiPath.isStopped = true;
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
            Debug.Log("Boss: Successfully fled to safe distance. SUCCESS.");
            return NodeState.SUCCESS;
        }

        if (!IsPlayerTooCloseToFlee())
        {
            _isCurrentlyFleeing = false;
            return NodeState.FAILURE;
        }

        _isCurrentlyFleeing = true;
        _aiPath.isStopped = false;
        _aiPath.maxSpeed = movementSpeed * 1.2f;
        _animator?.SetBool("isMoving", true);
        _animator?.SetFloat("Speed", _isEnraged ? 2f : 1f);

        Vector3 fleeDirection = (transform.position - targetPlayer.position).normalized;
        _aiPath.destination = transform.position + fleeDirection * (_fleeRange + 5f);

        if (_aiPath.pathPending || (_aiPath.hasPath && !_aiPath.isStopped))
        {
            Debug.Log("Boss: Fleeing. RUNNING.");
            return NodeState.RUNNING;
        }

        _isCurrentlyFleeing = false;
        _aiPath.isStopped = true;
        _animator?.SetBool("isMoving", false);
        _animator?.SetFloat("Speed", 0f);
        Debug.LogWarning("Boss: Fleeing failed or path not found. FAILURE.");
        return NodeState.FAILURE;
    }

    public void OnMeleeAttackAnimationEnd()
    {
        _isAttacking = false;
        _aiPath.isStopped = false;
        Debug.Log("Animation Event: Melee Attack animation ended. _isAttacking = false. AIPath resumed.");
    }

    public void OnDodgeAnimationEnd()
    {
        _isDodging = false;
        _aiPath.isStopped = false;
        _animator.applyRootMotion = false;
        Debug.Log("Animation Event: Dodge animation ended. _isDodging = false.");
    }

    public void OnStaggerAnimationEnd()
    {
        Debug.Log("Animation Event: Stagger animation ended. Boss now ready to recover.");
    }

    public void OnRecoverStaggerAnimationEnd()
    {
        _isStaggered = false;
        _aiPath.isStopped = false;
        Debug.Log("Animation Event: Recover Stagger animation ended. _isStaggered = false.");
    }

    public void OnParryAnimationEnd()
    {
        _isParrying = false;
        _aiPath.isStopped = false;
        Debug.Log("Animation Event: Parry animation ended. _isParrying = false.");
    }

    public void OnDieAnimationEnd()
    {
        Debug.Log("Animation Event: Die animation ended. Boss fully dead.");
    }

    public void ActivateMeleeAttackHitbox(int index)
    {
        if (meleeAttackHitboxes != null)
        {
            if (index >= 0 && index < meleeAttackHitboxes.Count)
                meleeAttackHitboxes[index].SetActive(true);
        }
    }

    public void DeactivateMeleeAttackHitbox()
    {
        foreach (var hitbox in meleeAttackHitboxes)
            hitbox.SetActive(false);
    }

    private void SetupBehaviorTree()
    {
        var isPlayerDetected = new ConditionNode(IsPlayerInDetectionRange);
        var isPlayerMeleeRange = new ConditionNode(IsPlayerInMeleeAttackRange);
        var isPlayerAtEngageDistance = new ConditionNode(IsPlayerAtEngageDistance);
        var isBossBusyCondition = new ConditionNode(IsBossBusy);
        var canDodge = new ConditionNode(CanDodge);
        var canAttack = new ConditionNode(CanAttack);
        var isPlayerAttacking = new ConditionNode(IsPlayerCurrentlyAttacking);
        var isLowHP = new ConditionNode(IsLowHP);
        var isFleeing = new ConditionNode(IsFleeing);
        var isPlayerTooCloseToFlee = new ConditionNode(IsPlayerTooCloseToFlee);

        var moveAction = new ActionNode(MoveTowardsPlayer);
        var meleeAttackAction = new ActionNode(PerformMeleeAttack);
        var dodgeAction = new ActionNode(Dodge);
        var recoverStaggerAction = new ActionNode(RecoverFromStagger);
        var blockOrParryAction = new ActionNode(BlockOrParry);
        var fleeAction = new ActionNode(PerformFlee);

        var busyBehavior = new SelectorNode(
            new SequenceNode(new ConditionNode(() => _isStaggered), recoverStaggerAction),
            new SequenceNode(new ConditionNode(() => _isParrying), blockOrParryAction),
            new SequenceNode(new ConditionNode(() => _isDodging), dodgeAction),
            new SequenceNode(isFleeing, fleeAction),
            new ActionNode(() => NodeState.RUNNING)
        );

        var coreCombatBehavior = new SelectorNode(
            new SequenceNode(
                canDodge,
                new SelectorNode(
                    isPlayerAttacking,
                    new ConditionNode(HasPlayerDodgedBossSkill)
                ),
                dodgeAction
            ),
            new SequenceNode(
                isPlayerAttacking,
                new Inverter(isBossBusyCondition),
                blockOrParryAction
            )
        );

        var approachAndAttackBehavior = new SelectorNode(
            new SequenceNode(
                isPlayerMeleeRange,
                canAttack,
                new Inverter(isBossBusyCondition),
                meleeAttackAction
            ),
            new SequenceNode(
                isPlayerAtEngageDistance,
                new Inverter(isBossBusyCondition),
                new ActionNode(() => {
                    _aiPath.isStopped = true;
                    _animator?.SetBool("isMoving", false);
                    _animator?.SetFloat("Speed", 0f);
                    Debug.Log("Boss: Within engage range, stopping to prepare.");
                    return NodeState.SUCCESS;
                })
            ),
            new SequenceNode(
                new Inverter(isPlayerMeleeRange),
                new Inverter(isPlayerAtEngageDistance),
                new Inverter(isBossBusyCondition),
                moveAction
            )
        );

        _rootNode = new SelectorNode(
            new SequenceNode(isBossBusyCondition, busyBehavior),
            new SequenceNode(isLowHP,
                new SelectorNode(
                    new SequenceNode(new ConditionNode(() => !_isEnraged), isPlayerTooCloseToFlee, fleeAction),
                    coreCombatBehavior,
                    approachAndAttackBehavior
                )
            ),
            coreCombatBehavior,
            approachAndAttackBehavior
        );
    }

    public void PlayGrowlSFX()
    {
        if (sfxGrowl != null && !sfxGrowl.isPlaying)
        {
            sfxGrowl.PlayOneShot(sfxGrowl.clip);
        }
    }

    public void PlayAttacklSFX()
    {
        if (sfxAttack != null && !sfxAttack.isPlaying)
        {
            sfxAttack.PlayOneShot(sfxAttack.clip);
        }
    }

    public void Play2AttacklSFX()
    {
        if (sfx2Attack != null && !sfx2Attack.isPlaying)
        {
            sfx2Attack.PlayOneShot(sfx2Attack.clip);
        }
    }

    public void PlayDeadSFX()
    {
        if (sfxDead != null && !sfxDead.isPlaying)
        {
            sfxDead.PlayOneShot(sfxDead.clip);
        }
    }


    /*void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, engageRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, approachDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }*/


}