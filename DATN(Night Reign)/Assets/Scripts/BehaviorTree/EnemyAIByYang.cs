using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding; // Đảm bảo bạn đã import A* Pathfinding

public class EnemyAI : MonoBehaviour
{
    [Header("Target & Detection")]
    public Transform targetPlayer;
    public float detectionRange = 15f;
    public float engageRange = 10f; // Khoảng cách Boss sẽ dừng di chuyển để bắt đầu tấn công (Đã tăng lên 10f)
    public float approachDistance = 10f; // Khoảng cách mà Boss sẽ áp sát người chơi để tấn công (Giữ bằng engageRange để có vùng stop rõ ràng)
    public float meleeAttackRange = 6f; // Phạm vi tấn công cận chiến

    [Header("Health & Phases")]
    public float currentHealth = 1000f;
    public float maxHealth = 1000f;
    public float phase2HealthThreshold = 500f;
    public float phase3HealthThreshold = 300f;

    [Header("Movement")]
    public float movementSpeed = 4f;
    public float enragedMovementSpeedMultiplier = 1.5f;
    public float dodgeSpeed = 8f;
    public float rotationSpeed = 10f;
    public float _fleeRange = 8f;

    [Header("Attack & Cooldowns")]
    public float attackCooldown = 1.5f;
    private float _lastAttackTime;
    public float dodgeCooldown = 3f;
    private float _lastDodgeTime;

    [Header("Damage & Hitboxes")]
    public GameObject meleeAttackHitbox;
    public float meleeDamage = 50f;

    // Tham chiếu đến các Component cần thiết
    private Node _rootNode;
    private Animator _animator;
    private AIPath _aiPath;
    private Seeker _seeker;
    private CharacterController _characterController;

    public enum BossPhase
    {
        Phase1,
        Phase2,
        Phase3
    }
    public BossPhase currentPhase = BossPhase.Phase1;

    private bool _isAttacking = false;
    private bool _isDodging = false;
    private bool _isStaggered = false;
    private bool _isParrying = false;
    private bool _isEnraged = false;
    private bool _isCurrentlyFleeing = false;
    private bool _playerDodgedBossSkill = false;


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

        // Điều chỉnh endReachedDistance và slowdownDistance để phù hợp với engageRange và meleeAttackRange
        // Boss sẽ dừng lại khi cách player một khoảng bằng engageRange để bắt đầu hành động.
        _aiPath.endReachedDistance = engageRange * 0.5f; // Dừng lại ở giữa engageRange và meleeAttackRange
        _aiPath.slowdownDistance = engageRange; // Bắt đầu giảm tốc khi cách engageRange

        if (meleeAttackHitbox != null)
        {
            meleeAttackHitbox.SetActive(false);
        }

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

        _lastAttackTime = -attackCooldown; // Đảm bảo Boss có thể tấn công ngay từ đầu
        _lastDodgeTime = -dodgeCooldown; // Đảm bảo Boss có thể né ngay từ đầu
    }

    public void Tick()
    {
        // Debugging cho các điều kiện tấn công và bận rộn
        float distToPlayer = targetPlayer != null ? Vector3.Distance(transform.position, targetPlayer.position) : -1f;
        Debug.Log($"[Tick] Dist: {distToPlayer:F2}f, MeleeRange: {meleeAttackRange}f, EngageRange: {engageRange}f, IsPlayerInMeleeAttackRange: {IsPlayerInMeleeAttackRange()}, CanAttack: {CanAttack()}, IsBossBusy: {IsBossBusy()} (Attacking:{_isAttacking}, Dodging:{_isDodging}, Staggered:{_isStaggered}, Fleeing:{_isCurrentlyFleeing})");


        if (_rootNode != null)
        {
            _rootNode.Evaluate();
        }

        // Chỉ xoay khi Boss không bận và có mục tiêu
        if (targetPlayer != null && !IsBossBusy())
        {
            LookAtTarget(targetPlayer.position);
        }

        if (_isEnraged)
        {
            _aiPath.maxSpeed = movementSpeed * enragedMovementSpeedMultiplier;
        }
        else
        {
            _aiPath.maxSpeed = movementSpeed;
        }

        // Cập nhật animation di chuyển
        if (_aiPath.desiredVelocity.magnitude > 0.1f && !_isAttacking && !_isDodging && !_isStaggered && !_isCurrentlyFleeing)
        {
            _animator?.SetBool("isMoving", true);
            _animator?.SetFloat("Speed", _aiPath.desiredVelocity.magnitude);
        }
        else
        {
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Boss took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (currentHealth <= phase3HealthThreshold && currentPhase != BossPhase.Phase3)
        {
            TransitionToPhase(BossPhase.Phase3);
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
        enabled = false; // Tắt script EnemyAI

        if (EnemyAIManager.Instance != null)
        {
            EnemyAIManager.Instance.UnregisterEnemy(this);
        }
    }

    private void TransitionToPhase(BossPhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log($"Boss transitioning to {newPhase} at {currentHealth} HP.");
        _animator?.SetTrigger("ChangePhase");

        if (newPhase == BossPhase.Phase3)
        {
            _isEnraged = true;
            Debug.Log("Boss is ENRAGED!");
        }
        SetupBehaviorTree(); // Cập nhật cây hành vi cho pha mới
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Giữ Boss không bị nghiêng

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    // --- CÁC HÀM ĐIỀU KIỆN ---
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

    // Điều kiện này sẽ kiểm tra xem player có nằm trong khoảng từ meleeAttackRange đến engageRange không
    private bool IsPlayerAtEngageDistance()
    {
        if (targetPlayer == null) return false;
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        return distance > meleeAttackRange && distance <= engageRange;
    }

    private bool IsLowHP()
    {
        return currentHealth <= phase3HealthThreshold;
    }

    private bool CanDodge()
    {
        return Time.time >= _lastDodgeTime + dodgeCooldown;
    }

    private bool CanAttack()
    {
        bool canAttack = Time.time >= _lastAttackTime + attackCooldown;
        return canAttack;
    }

    public bool IsBossBusy()
    {
        bool busy = _isAttacking || _isDodging || _isStaggered || _isParrying || _isCurrentlyFleeing;
        return busy;
    }

    private bool IsPlayerCurrentlyAttacking()
    {
        // TODO: Triển khai logic này dựa trên trạng thái của Player (ví dụ: Player có đang animation tấn công không?)
        // Hiện tại luôn trả về false để không chặn Boss.
        return false;
    }

    public void PlayerDodgedBossSkill()
    {
        _playerDodgedBossSkill = true;
        Debug.Log("Player successfully dodged Boss's skill!");
    }

    private bool HasPlayerDodgedBossSkill()
    {
        bool dodged = _playerDodgedBossSkill;
        if (dodged) _playerDodgedBossSkill = false; // Reset cờ sau khi kiểm tra
        return dodged;
    }

    private bool IsPlayerTooCloseToFlee()
    {
        if (targetPlayer == null) return false;
        return Vector3.Distance(transform.position, targetPlayer.position) <= _fleeRange;
    }

    private bool IsFleeing()
    {
        return _isCurrentlyFleeing;
    }

    // --- CÁC HÀM HÀNH ĐỘNG ---
    private NodeState MoveTowardsPlayer()
    {
        if (targetPlayer == null)
        {
            _aiPath.isStopped = true;
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
            return NodeState.FAILURE;
        }

        // Nếu Boss bận (trừ khi đang bỏ chạy), không di chuyển
        if (IsBossBusy() && !_isCurrentlyFleeing)
        {
            _aiPath.isStopped = true;
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
            return NodeState.FAILURE;
        }

        float currentDistance = Vector3.Distance(transform.position, targetPlayer.position);

        // Nếu đã đủ gần để tấn công hoặc dừng, dừng di chuyển và báo thành công
        // Boss sẽ dừng di chuyển nếu trong khoảng cách engageRange.
        if (currentDistance <= engageRange)
        {
            _aiPath.isStopped = true;
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
            Debug.Log("[MoveTowardsPlayer] Reached engage range or closer. Stopping movement. SUCCESS.");
            return NodeState.SUCCESS;
        }

        // Nếu chưa đủ gần, đặt đích và cho phép di chuyển
        _aiPath.destination = targetPlayer.position;
        _aiPath.isStopped = false; // Bật lại AIPath để di chuyển
        _aiPath.maxSpeed = movementSpeed; // Đảm bảo tốc độ đúng

        // Kích hoạt animation di chuyển
        _animator?.SetBool("isMoving", true);
        // AIPath.desiredVelocity.magnitude sẽ cho biết tốc độ mong muốn của AIPath
        _animator?.SetFloat("Speed", _aiPath.desiredVelocity.magnitude);

        if (_aiPath.pathPending)
        {
            Debug.Log("[MoveTowardsPlayer] Path pending... RUNNING.");
            return NodeState.RUNNING;
        }

        if (_aiPath.hasPath && !_aiPath.isStopped)
        {
            // Vẫn đang di chuyển hoặc đã đến gần engageRange
            Debug.Log("[MoveTowardsPlayer] Moving towards player... RUNNING.");
            return NodeState.RUNNING;
        }

        // Nếu không có đường đi hoặc bị chặn, thất bại
        _aiPath.isStopped = true;
        _animator?.SetBool("isMoving", false);
        _animator?.SetFloat("Speed", 0f);
        Debug.LogWarning("[MoveTowardsPlayer] Failed to find path or is stopped unexpectedly. FAILURE.");
        return NodeState.FAILURE;
    }

    private NodeState PerformMeleeAttack()
    {
        // Điều kiện để bắt đầu tấn công
        if (targetPlayer == null) return NodeState.FAILURE;
        if (!IsPlayerInMeleeAttackRange()) return NodeState.FAILURE; // Phải trong tầm cận chiến
        if (IsBossBusy()) return NodeState.FAILURE; // Quan trọng: Đảm bảo Boss không bận!
        if (!CanAttack()) return NodeState.FAILURE; // Cooldown đã hết chưa

        Debug.Log("Boss: Performing Melee Attack!");
        _isAttacking = true;
        _lastAttackTime = Time.time;
        _aiPath.isStopped = true; // Dừng di chuyển ngay lập tức
        _animator?.SetBool("isMoving", false); // Dừng animation di chuyển khi tấn công
        _animator?.SetFloat("Speed", 0f);

        // Chọn ngẫu nhiên animation tấn công
        int attackChoice = UnityEngine.Random.Range(1, 4); // Chọn giữa 1, 2, 3 (vì Range(minInclusive, maxExclusive))
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

        // Vì đây là hành động không đồng bộ (animation event sẽ reset cờ), chúng ta trả về RUNNING
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
        _animator.applyRootMotion = true; // Kích hoạt Root Motion nếu animation né tránh sử dụng nó

        // Chọn ngẫu nhiên animation né tránh theo hướng
        int dodgeChoice = UnityEngine.Random.Range(0, 4); // 0:Left, 1:Right, 2:LeftBack, 3:RightBack
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
        _aiPath.isStopped = true; // Đảm bảo Boss dừng khi đang choáng
        // StartCoroutine(StaggerRecoveryCoroutine(1.0f)); // Nếu bạn muốn dùng Coroutine để phục hồi
        // Thay vì Coroutine, Animation Event OnStaggerAnimationEnd() hoặc OnRecoverStaggerAnimationEnd() sẽ được gọi
        // để thực sự chuyển trạng thái. Behavior Tree sẽ tiếp tục trả về RUNNING cho đến khi _isStaggered = false.
        return NodeState.RUNNING;
    }

    private IEnumerator StaggerRecoveryCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        OnRecoverStaggerAnimationEnd();
    }

    public void StaggerBoss(float duration)
    {
        if (_isStaggered) return; // Tránh choáng lặp lại nếu đã choáng
        _isStaggered = true;
        _aiPath.isStopped = true;
        _animator?.SetBool("isMoving", false); // Dừng di chuyển khi choáng
        _animator?.SetFloat("Speed", 0f);
        Debug.Log($"Boss STAGGERED for {duration} seconds!");
        _animator?.SetTrigger("GetHit"); // Kích hoạt animation trúng đòn
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
        // Kích hoạt animation Parry nếu có
        // _animator?.SetTrigger("Parry"); 

        return NodeState.RUNNING;
    }

    private NodeState PerformFlee()
    {
        if (targetPlayer == null) return NodeState.FAILURE;

        // Điều kiện để thoát khỏi trạng thái bỏ chạy:
        // Đang bỏ chạy VÀ đã đến đích VÀ khoảng cách với Player ĐÃ LỚN HƠN tầm bỏ chạy
        if (_isCurrentlyFleeing && _aiPath.reachedDestination && Vector3.Distance(transform.position, targetPlayer.position) > _fleeRange)
        {
            _isCurrentlyFleeing = false;
            _aiPath.isStopped = true;
            _animator?.SetBool("isMoving", false);
            _animator?.SetFloat("Speed", 0f);
            Debug.Log("Boss: Successfully fled to safe distance. SUCCESS.");
            return NodeState.SUCCESS;
        }

        // Điều kiện để bắt đầu hoặc tiếp tục bỏ chạy:
        // Đang quá gần để bỏ chạy HOẶC đang trong quá trình bỏ chạy
        if (!IsPlayerTooCloseToFlee() && !_isCurrentlyFleeing)
        {
            // Nếu không quá gần và không đang bỏ chạy, thì không cần bỏ chạy nữa
            return NodeState.FAILURE;
        }


        // Nếu chưa bỏ chạy hoặc chưa đến đích an toàn, bắt đầu/tiếp tục bỏ chạy
        _isCurrentlyFleeing = true;
        _aiPath.isStopped = false;
        _aiPath.maxSpeed = movementSpeed * 1.2f; // Chạy nhanh hơn khi bỏ chạy
        _animator?.SetBool("isMoving", true);
        _animator?.SetFloat("Speed", _aiPath.desiredVelocity.magnitude);

        Vector3 fleeDirection = (transform.position - targetPlayer.position).normalized;
        // Đặt đích cách xa người chơi một khoảng _fleeRange + 5f
        _aiPath.destination = transform.position + fleeDirection * (_fleeRange + 5f);

        if (_aiPath.pathPending || (_aiPath.hasPath && !_aiPath.isStopped))
        {
            Debug.Log("Boss: Fleeing. RUNNING.");
            return NodeState.RUNNING;
        }
        // Nếu không có đường đi hoặc bị dừng, coi như thất bại
        _isCurrentlyFleeing = false;
        _aiPath.isStopped = true;
        _animator?.SetBool("isMoving", false);
        _animator?.SetFloat("Speed", 0f);
        Debug.LogWarning("Boss: Fleeing failed or path not found. FAILURE.");
        return NodeState.FAILURE;
    }

    // --- CÁC HÀM GỌI TỪ ANIMATION EVENTS (QUAN TRỌNG!) ---
    public void OnMeleeAttackAnimationEnd()
    {
        _isAttacking = false;
        _aiPath.isStopped = false; // <<< THAY ĐỔI QUAN TRỌNG: CHO PHÉP BOSS DI CHUYỂN LẠI SAU KHI TẤN CÔNG
        Debug.Log("Animation Event: Melee Attack animation ended. _isAttacking = false. AIPath resumed. Boss should now move/attack.");
    }

    public void OnDodgeAnimationEnd()
    {
        _isDodging = false;
        _aiPath.isStopped = false; // Cho phép Boss di chuyển lại
        _animator.applyRootMotion = false; // Tắt Root Motion sau khi né tránh
        Debug.Log("Animation Event: Dodge animation ended. _isDodging = false.");
    }

    public void OnStaggerAnimationEnd()
    {
        Debug.Log("Animation Event: Stagger animation ended. Boss now ready to recover.");
        // Bạn có thể không cần _isStaggered = false ở đây nếu RecoverFromStaggerCoroutine xử lý.
        // Hoặc nếu animation RecoverStagger là riêng biệt, hàm này chỉ thông báo kết thúc stagger animation.
    }

    public void OnRecoverStaggerAnimationEnd()
    {
        _isStaggered = false;
        _aiPath.isStopped = false; // Cho phép Boss di chuyển lại
        Debug.Log("Animation Event: Recover Stagger animation ended. _isStaggered = false.");
    }

    public void OnParryAnimationEnd()
    {
        _isParrying = false;
        _aiPath.isStopped = false; // Cho phép Boss di chuyển lại
        Debug.Log("Animation Event: Parry animation ended. _isParrying = false.");
    }

    public void OnDieAnimationEnd()
    {
        Debug.Log("Animation Event: Die animation ended. Boss fully dead.");
    }

    public void ActivateMeleeAttackHitbox()
    {
        if (meleeAttackHitbox != null)
        {
            meleeAttackHitbox.SetActive(true);
            Debug.Log("Animation Event: Melee Attack Hitbox Activated!");
        }
    }

    public void DeactivateMeleeAttackHitbox()
    {
        if (meleeAttackHitbox != null)
        {
            meleeAttackHitbox.SetActive(false);
            Debug.Log("Animation Event: Melee Attack Hitbox Deactivated!");
        }
    }

    // --- KHỞI TẠO CÂY HÀNH VI ---
    private void SetupBehaviorTree()
    {
        // Các Node điều kiện
        var isPlayerDetected = new ConditionNode(IsPlayerInDetectionRange);
        var isPlayerMeleeRange = new ConditionNode(IsPlayerInMeleeAttackRange);
        var isPlayerAtEngageDistance = new ConditionNode(IsPlayerAtEngageDistance); // Sử dụng engageDistance mới
        var isBossBusyCondition = new ConditionNode(IsBossBusy);
        var canDodge = new ConditionNode(CanDodge);
        var canAttack = new ConditionNode(CanAttack);
        var isPlayerAttacking = new ConditionNode(IsPlayerCurrentlyAttacking);
        var isLowHP = new ConditionNode(IsLowHP);
        var isFleeing = new ConditionNode(IsFleeing);
        var isPlayerTooCloseToFlee = new ConditionNode(IsPlayerTooCloseToFlee);

        // Các Node hành động
        var moveAction = new ActionNode(MoveTowardsPlayer);
        var meleeAttackAction = new ActionNode(PerformMeleeAttack);
        var dodgeAction = new ActionNode(Dodge);
        var recoverStaggerAction = new ActionNode(RecoverFromStagger);
        var blockOrParryAction = new ActionNode(BlockOrParry);
        var fleeAction = new ActionNode(PerformFlee);

        // Sub-tree cho các trạng thái Boss đang bận (không thể làm gì khác)
        var busyBehavior = new SelectorNode(
            new SequenceNode(new ConditionNode(() => _isStaggered), recoverStaggerAction), // Hồi phục choáng
            new SequenceNode(new ConditionNode(() => _isParrying), blockOrParryAction),   // Đang parry
            new SequenceNode(new ConditionNode(() => _isDodging), dodgeAction),         // Đang né tránh
            new SequenceNode(isFleeing, fleeAction),                                     // Đang bỏ chạy
            new ActionNode(() => NodeState.RUNNING) // Nếu Boss bận nhưng không rơi vào các trường hợp trên, vẫn coi là bận.
        );

        // Sub-tree cho các hành vi chiến đấu cốt lõi (né tránh, block/parry)
        var coreCombatBehavior = new SelectorNode(
            // Ưu tiên Né tránh nếu có thể và điều kiện phù hợp
            new SequenceNode(
                canDodge,
                new SelectorNode( // Điều kiện để né tránh
                    isLowHP,
                    isPlayerAttacking,
                    new ConditionNode(HasPlayerDodgedBossSkill)
                ),
                dodgeAction
            ),
            // Ưu tiên Block/Parry nếu Player đang tấn công
            new SequenceNode(
                isPlayerAttacking,
                new Inverter(isBossBusyCondition), // Đảm bảo Boss không bận khi Block/Parry
                blockOrParryAction
            )
        );

        // Sub-tree cho hành vi áp sát và tấn công
        var approachAndAttackBehavior = new SelectorNode(
            // 1. Ưu tiên: Tấn công cận chiến nếu trong tầm và có thể tấn công
            new SequenceNode(
                isPlayerMeleeRange,         // Kiểm tra khoảng cách
                canAttack,                  // Kiểm tra cooldown
                new Inverter(isBossBusyCondition), // Kiểm tra trạng thái bận
                meleeAttackAction           // Thực hiện tấn công
            ),
            // 2. Nếu không trong tầm cận chiến nhưng trong tầm engage, dừng di chuyển và chờ hoặc tấn công
            new SequenceNode(
                isPlayerAtEngageDistance,   // Player đang ở khoảng cách engage (giữa melee và engage range)
                new Inverter(isBossBusyCondition), // Boss không bận
                new ActionNode(() => { // Hành động dừng lại và chờ. Nếu cooldown tấn công xong, có thể tấn công ngay.
                    _aiPath.isStopped = true;
                    _animator?.SetBool("isMoving", false);
                    _animator?.SetFloat("Speed", 0f);
                    Debug.Log("Boss: Within engage range, stopping to prepare.");
                    return NodeState.SUCCESS; // Trả về SUCCESS để selector tiếp tục kiểm tra các hành vi khác (ví dụ: tấn công)
                })
            ),
            // 3. Nếu ngoài tầm engage, di chuyển tới người chơi (chase)
            new SequenceNode(
                new Inverter(isPlayerMeleeRange),       // Player KHÔNG trong tầm cận chiến
                new Inverter(isPlayerAtEngageDistance), // Player KHÔNG trong tầm engage (tức là ngoài 10f)
                new Inverter(isBossBusyCondition),       // Boss không bận
                moveAction                               // Di chuyển tới Player
            )
        );

        // Cây hành vi gốc: Ưu tiên các trạng thái bận, sau đó là các hành vi chiến đấu theo pha, và cuối cùng là di chuyển/tấn công
        _rootNode = new SelectorNode(
            new SequenceNode(isBossBusyCondition, busyBehavior), // 1. Ưu tiên xử lý các trạng thái bận của Boss (stagger, dodge, parry, flee)
            new SequenceNode(isLowHP, // 2. Nếu HP thấp, ưu tiên các hành vi phòng thủ/bỏ chạy
                new SelectorNode(
                    new SequenceNode(isPlayerTooCloseToFlee, fleeAction), // Bỏ chạy nếu quá gần và HP thấp
                    coreCombatBehavior // Hoặc tiếp tục chiến đấu cốt lõi
                )
            ),
            coreCombatBehavior, // 3. Hành vi chiến đấu cốt lõi (né tránh, block/parry) - nếu không bận và không HP thấp cần bỏ chạy
            approachAndAttackBehavior // 4. Hành vi áp sát và tấn công - ưu tiên cuối cùng nếu không có gì đặc biệt xảy ra
        );
    }
}