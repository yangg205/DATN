using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding; // Quan trọng: Thêm namespace này để sử dụng AIPath và Seeker

public class EnemyAI : MonoBehaviour
{
    [Header("Target & Detection")]
    public Transform targetPlayer; // Đối tượng người chơi mà Boss nhắm tới
    public float detectionRange = 15f; // Phạm vi Boss có thể phát hiện người chơi
    public float engageRange = 5f; // Khoảng cách Boss sẽ dừng di chuyển để bắt đầu tấn công
    public float meleeAttackRange = 2f; // Phạm vi tấn công cận chiến (vẫn dùng cho logic BT, nhưng đòn đánh thường sẽ dùng collider)
    public float rangedAttackRange = 10f; // Phạm vi tấn công tầm xa

    [Header("Health & Phases")]
    public float currentHealth = 1000f; // Máu hiện tại của Boss
    public float maxHealth = 1000f; // Máu tối đa của Boss
    public float phase2HealthThreshold = 500f; // Ngưỡng HP để chuyển sang Phase 2 (ví dụ: 50% HP)
    public float phase3HealthThreshold = 300f; // Ngưỡng HP để chuyển sang Phase 3 (30% HP cho "Nổi loạn")

    [Header("Movement")]
    public float movementSpeed = 3f; // Tốc độ di chuyển bình thường
    public float enragedMovementSpeedMultiplier = 1.5f; // Tốc độ di chuyển khi nổi loạn
    public float dodgeSpeed = 8f; // Tốc độ né tránh
    public float rotationSpeed = 10f; // Tốc độ xoay của Boss để nhìn Player (chúng ta sẽ tự xoay, không dùng AIPath mặc định)

    [Header("Attack & Cooldowns")]
    public float recoveryTimeAfterAttack = 1.0f; // Thời gian hồi phục sau một đòn tấn công
    public float dodgeCooldown = 2f; // Cooldown cho kỹ năng né tránh
    private float _lastDodgeTime; // Thời điểm cuối cùng thực hiện né tránh
    public float specialAbilityCooldown = 5f; // Cooldown cho kỹ năng đặc biệt
    private float _lastSpecialAbilityTime; // Thời điểm cuối cùng sử dụng kỹ năng đặc biệt
    public float parryWindowDuration = 0.5f; // Thời gian cửa sổ parry

    // Tham chiếu đến các Component cần thiết
    private Node _rootNode; // Cây hành vi gốc của Boss
    private Animator _animator; // Animator để điều khiển hoạt ảnh của Boss
    private AIPath _aiPath; // Tham chiếu đến AIPath component từ A* Pathfinding Project
    private Seeker _seeker; // Tham chiếu đến Seeker component từ A* Pathfinding Project (dùng bởi AIPath)
    private CharacterController _characterController; // Tham chiếu đến CharacterController (hoặc Rigidbody nếu dùng)

    // Enum định nghĩa các giai đoạn của Boss trong trận chiến
    public enum BossPhase
    {
        Phase1,
        Phase2,
        Phase3 // Đây sẽ là trạng thái "Nổi loạn"
    }
    public BossPhase currentPhase = BossPhase.Phase1; // Giai đoạn hiện tại của Boss, mặc định là Phase 1

    // Biến trạng thái nội bộ của Boss (để Behavior Tree biết Boss đang làm gì)
    private bool _isAttacking = false;
    private bool _isDodging = false;
    private bool _isPerformingSpecialAbility = false;
    private bool _isStaggered = false; // Trạng thái choáng (Boss không thể hành động)
    private bool _isParrying = false; // Trạng thái Boss đang cố gắng đỡ đòn/parry
    private bool _isEnraged = false; // Trạng thái nổi loạn

    // Biến cho logic mới
    private int _playerNormalAttackComboCount = 0; // Đếm số đòn đánh thường liên tiếp của người chơi
    private bool _playerDodgedBossSkill = false; // Cờ hiệu khi người chơi né được skill của Boss
    private bool _canCastSkill1 = false; // Cờ hiệu để biết có thể dùng Skill 1 không (khi player đánh thường đòn 2)

    void Awake()
    {
        // Cố gắng tìm GameObject của người chơi nếu chưa được gán trong Inspector
        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player"); // Đảm bảo GameObject Player có tag "Player"
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("Player object with 'Player' tag not found. Please assign targetPlayer manually or ensure Player has the correct tag.");
            }
        }
    }

    void Start()
    {
        // Lấy các component từ GameObject này
        _animator = GetComponent<Animator>();
        _aiPath = GetComponent<AIPath>();
        _seeker = GetComponent<Seeker>();
        _characterController = GetComponent<CharacterController>(); // Lấy CharacterController

        // Kiểm tra xem các component cần thiết có tồn tại không
        if (_aiPath == null)
        {
            Debug.LogError("AIPath component not found on Boss object. Please add one in the Unity Editor.");
            enabled = false; // Vô hiệu hóa script nếu thiếu component quan trọng
            return;
        }
        if (_seeker == null)
        {
            Debug.LogError("Seeker component not found on Boss object. Please add one in the Unity Editor.");
            enabled = false;
            return;
        }
        if (_characterController == null)
        {
            Debug.LogError("CharacterController component not found on Boss object. Please add one in the Unity Editor.");
            enabled = false;
            return;
        }

        // Cấu hình các thuộc tính ban đầu của AIPath
        _aiPath.maxSpeed = movementSpeed; // Đặt tốc độ di chuyển tối đa cho AIPath
        _aiPath.enableRotation = false; // Tắt tính năng xoay tự động của AIPath vì chúng ta sẽ tự xoay Boss
        _aiPath.endReachedDistance = engageRange * 0.8f; // Khoảng cách Boss được coi là đã đến đích (gần Player)
        _aiPath.slowdownDistance = engageRange; // Khoảng cách mà AIPath bắt đầu giảm tốc độ

        // Khởi tạo cây hành vi của Boss
        SetupBehaviorTree();

        // Đăng ký Boss vào EnemyAIManager để được quản lý và tick
        if (EnemyAIManager.Instance != null)
        {
            EnemyAIManager.Instance.RegisterEnemy(this);
        }
        else
        {
            Debug.LogWarning("EnemyAIManager instance not found. Please ensure it is present in the scene.");
        }
    }

    // Hàm Tick này được gọi bởi EnemyAIManager mỗi khi đến lượt Boss được cập nhật
    public void Tick()
    {
        if (_rootNode != null)
        {
            _rootNode.Evaluate(); // Đánh giá cây hành vi để quyết định hành động tiếp theo
        }

        // Luôn xoay Boss nhìn về phía người chơi khi Boss không đang thực hiện các hành động khác
        if (targetPlayer != null && !_isDodging && !_isPerformingSpecialAbility && !_isStaggered && !_isAttacking && !_isParrying)
        {
            LookAtTarget(targetPlayer.position);
        }

        // Cập nhật tốc độ di chuyển nếu Boss đang ở trạng thái nổi loạn
        if (_isEnraged)
        {
            _aiPath.maxSpeed = movementSpeed * enragedMovementSpeedMultiplier;
        }
        else
        {
            _aiPath.maxSpeed = movementSpeed;
        }
    }

    // Hàm xử lý khi Boss nhận sát thương
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Boss took {damage} damage. Current health: {currentHealth}");

        // Kiểm tra chuyển đổi giai đoạn dựa trên lượng máu hiện tại
        if (currentHealth <= 0)
        {
            Die(); // Boss chết
        }
        else if (currentHealth <= phase3HealthThreshold && currentPhase != BossPhase.Phase3)
        {
            TransitionToPhase(BossPhase.Phase3); // Chuyển sang Phase 3 (Nổi loạn)
        }
        else if (currentHealth <= phase2HealthThreshold && currentPhase == BossPhase.Phase1)
        {
            TransitionToPhase(BossPhase.Phase2); // Chuyển sang Phase 2
        }
    }

    // Hàm xử lý cái chết của Boss
    private void Die()
    {
        Debug.Log("Boss has been defeated!");
        _animator?.SetTrigger("Die"); // Kích hoạt animation chết
        _aiPath.isStopped = true; // Dừng mọi di chuyển của AIPath
        enabled = false; // Vô hiệu hóa script này để Boss ngừng mọi hoạt động
        // TODO: Thêm logic thả vật phẩm, hiệu ứng, kết thúc màn chơi, v.v.
        if (EnemyAIManager.Instance != null)
        {
            EnemyAIManager.Instance.UnregisterEnemy(this); // Hủy đăng ký Boss khi chết
        }
    }

    // Hàm chuyển đổi giữa các giai đoạn chiến đấu của Boss
    private void TransitionToPhase(BossPhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log($"Boss transitioning to {newPhase} at {currentHealth} HP.");
        _animator?.SetTrigger("ChangePhase"); // Kích hoạt animation chuyển pha

        if (newPhase == BossPhase.Phase3)
        {
            _isEnraged = true;
            Debug.Log("Boss is ENRAGED!");
            // TODO: Thêm hiệu ứng nổi loạn đặc biệt (ví dụ: đổi màu, aura)
        }
        SetupBehaviorTree(); // Gọi lại SetupBehaviorTree để cập nhật logic AI cho giai đoạn mới
    }

    // Hàm xoay Boss nhìn về phía mục tiêu một cách mượt mà
    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Giữ Boss không nghiêng theo trục Y, chỉ xoay trên mặt phẳng ngang

        if (direction != Vector3.zero) // Chỉ xoay nếu có hướng xác định
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction); // Tạo Quaternion quay đến hướng mục tiêu
            // Slerp (Spherical Linear Interpolation) để xoay mượt mà theo thời gian
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    // --- CÁC HÀM ĐIỀU KIỆN (ConditionNodes) ---
    // Kiểm tra xem người chơi có trong phạm vi phát hiện của Boss không
    private bool IsPlayerInDetectionRange()
    {
        if (targetPlayer == null) return false;
        return Vector3.Distance(transform.position, targetPlayer.position) <= detectionRange;
    }

    // Kiểm tra xem người chơi có trong phạm vi tấn công cận chiến của Boss không (dùng cho logic BT)
    private bool IsPlayerInMeleeAttackRange()
    {
        if (targetPlayer == null) return false;
        return Vector3.Distance(transform.position, targetPlayer.position) <= meleeAttackRange;
    }

    // Kiểm tra xem người chơi có trong phạm vi tấn công tầm xa của Boss không
    private bool IsPlayerInRangedAttackRange()
    {
        if (targetPlayer == null) return false;
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        return distance > meleeAttackRange && distance <= rangedAttackRange; // Ở xa hơn cận chiến nhưng trong tầm xa
    }

    // Kiểm tra xem HP của Boss có thấp hơn ngưỡng quy định không (dùng cho hành vi "HP thấp")
    private bool IsLowHP()
    {
        return currentHealth <= phase3HealthThreshold;
    }

    // Kiểm tra xem Boss có thể né tránh không (dựa trên cooldown)
    private bool CanDodge()
    {
        return Time.time >= _lastDodgeTime + dodgeCooldown;
    }

    // Kiểm tra xem Boss có thể sử dụng kỹ năng đặc biệt không (dựa trên cooldown)
    private bool CanUseSpecialAbility()
    {
        // Khi nổi loạn, cooldown kỹ năng sẽ ngắn hơn (hoặc bỏ qua cooldown)
        float currentSpecialCooldown = _isEnraged ? specialAbilityCooldown * 0.5f : specialAbilityCooldown; // Giảm cooldown khi nổi loạn
        return Time.time >= _lastSpecialAbilityTime + currentSpecialCooldown;
    }

    // Kiểm tra xem Boss có đang thực hiện một hành động ưu tiên cao nào đó không (tấn công, né tránh, choáng, v..v.)
    private bool IsBossBusy()
    {
        return _isAttacking || _isDodging || _isPerformingSpecialAbility || _isStaggered || _isParrying;
    }

    // Hàm nhận tín hiệu từ Player khi Player thực hiện đòn đánh thường.
    // Cần gọi hàm này từ script PlayerCombat khi Player đánh trúng Boss.
    public void PlayerPerformedNormalAttack(int comboIndex) // comboIndex: 1 cho đòn 1, 2 cho đòn 2
    {
        if (comboIndex == 1)
        {
            _playerNormalAttackComboCount = 1;
            Debug.Log("Player hit Boss with normal attack 1.");
        }
        else if (comboIndex == 2)
        {
            _playerNormalAttackComboCount = 2;
            _canCastSkill1 = true; // Đặt cờ cho phép Boss dùng Skill 1
            Debug.Log("Player hit Boss with normal attack 2. Boss can now use Skill 1.");
        }
        else
        {
            _playerNormalAttackComboCount = 0; // Reset combo nếu không phải 1 hoặc 2 (ví dụ: đòn 3 trở lên)
        }
    }

    // Kiểm tra xem người chơi có đang tấn công hay không (ví dụ: cho Parry)
    private bool IsPlayerCurrentlyAttacking()
    {
        // TODO: Hàm này cần liên kết với PlayerCombat/PlayerState để biết Player có đang ở trạng thái tấn công không
        // Ví dụ: return targetPlayer.GetComponent<PlayerState>().IsAttackingState();
        return false; // Mặc định là false cho đến khi bạn triển khai nó
    }

    // Kiểm tra xem Boss có thể cast Skill 1 không (sau đòn đánh thứ 2 của Player)
    private bool CanCastSkill1()
    {
        return _canCastSkill1 && CanUseSpecialAbility(); // Đảm bảo có thể cast và không cooldown
    }

    // Đặt cờ khi người chơi né được skill của Boss
    public void PlayerDodgedBossSkill()
    {
        _playerDodgedBossSkill = true;
        Debug.Log("Player successfully dodged Boss's skill! Boss will react.");
    }

    // Kiểm tra xem người chơi có vừa né được skill của Boss không
    private bool HasPlayerDodgedBossSkill()
    {
        return _playerDodgedBossSkill;
    }


    // --- CÁC HÀM HÀNH ĐỘNG (ActionNodes) ---
    // Hành động di chuyển Boss về phía người chơi sử dụng AIPath
    private NodeState MoveTowardsPlayer()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (IsBossBusy()) // Nếu Boss đang bận, không di chuyển
        {
            _aiPath.isStopped = true; // Dừng di chuyển của AIPath
            _animator?.SetBool("isMoving", false);
            return NodeState.FAILURE;
        }

        // Cập nhật đích đến của AIPath mỗi khi hành động này được thực thi
        _aiPath.destination = targetPlayer.position;
        _aiPath.isStopped = false; // Đảm bảo AIPath đang di chuyển
        // Tốc độ di chuyển đã được cập nhật trong Tick() dựa trên trạng thái nổi loạn

        Debug.Log("Boss: Moving towards Player (using AIPath)");
        _animator?.SetBool("isMoving", true);

        // Kiểm tra nếu Boss đã đến gần đích đủ để dừng lại
        if (_aiPath.reachedDestination || Vector3.Distance(transform.position, targetPlayer.position) <= engageRange)
        {
            _aiPath.isStopped = true; // Dừng AIPath lại khi đã đến gần đích
            _animator?.SetBool("isMoving", false);
            Debug.Log("Boss: Reached engage range, stopping movement.");
            return NodeState.SUCCESS;
        }

        // Nếu AIPath đang tìm đường hoặc đang di chuyển, coi như hành động này đang chạy
        if (_aiPath.pathPending || !_aiPath.isStopped)
        {
            return NodeState.RUNNING;
        }

        // Nếu không tìm được đường hoặc có lỗi, coi như thất bại
        return NodeState.FAILURE;
    }

    // Hành động tấn công cận chiến (đòn đánh thường)
    // Hành động này sẽ được kích hoạt khi Boss chạm vào collider của Player
    private NodeState PerformNormalMeleeAttack()
    {
        // Logic này sẽ chủ yếu kích hoạt animation và reset cờ.
        // Việc gây sát thương sẽ được xử lý trong OnControllerColliderHit hoặc Trigger.
        if (targetPlayer == null) return NodeState.FAILURE;
        // Giả sử chỉ cần Boss không bận là có thể thử đánh
        if (IsBossBusy()) return NodeState.FAILURE;

        Debug.Log("Boss: Performing Normal Melee Attack!");
        _isAttacking = true; // Đặt cờ Boss đang tấn công
        _aiPath.isStopped = true; // Dừng di chuyển của AIPath khi tấn công
        _animator?.SetTrigger("MeleeAttack"); // Kích hoạt animation tấn công cận chiến (có thể đổi tên trigger)

        // Bắt đầu Coroutine để chờ animation và thời gian hồi phục
        StartCoroutine(AttackCoroutine(recoveryTimeAfterAttack, () => _isAttacking = false));
        return NodeState.RUNNING;
    }

    // Hành động tấn công tầm xa (nếu có, nhưng yêu cầu của bạn tập trung vào cận chiến và skill)
    private NodeState PerformRangedAttack()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (!IsPlayerInRangedAttackRange()) return NodeState.FAILURE;
        if (IsBossBusy()) return NodeState.FAILURE;

        Debug.Log("Boss: Performing Ranged Attack!");
        _isAttacking = true;
        _aiPath.isStopped = true;
        _animator?.SetTrigger("RangedAttack");

        StartCoroutine(AttackCoroutine(recoveryTimeAfterAttack, () => _isAttacking = false));
        return NodeState.RUNNING;
    }

    // Hành động sử dụng Kỹ năng 1
    private NodeState UseSkill1()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (IsBossBusy()) return NodeState.FAILURE;

        // Đặt lại cờ cho phép cast Skill 1
        _canCastSkill1 = false;
        _playerNormalAttackComboCount = 0; // Reset combo count của Player

        Debug.Log("Boss: Using Skill 1!");
        _isPerformingSpecialAbility = true; // Đặt cờ Boss đang thực hiện kỹ năng đặc biệt
        _lastSpecialAbilityTime = Time.time; // Cập nhật thời điểm sử dụng kỹ năng cuối cùng
        _aiPath.isStopped = true; // Dừng di chuyển của AIPath khi thực hiện kỹ năng đặc biệt
        _animator?.SetTrigger("Skill1"); // Kích hoạt animation Skill 1

        // TODO: Triển khai logic của Skill 1 ở đây (ví dụ: tạo hiệu ứng, gây sát thương, triệu hồi)
        // Khi Skill 1 được hoàn thành hoặc bị né, Boss cần thông báo lại cho PlayerDodgedBossSkill()
        // để có thể reset cờ _playerDodgedBossSkill nếu Skill 1 không bị né.
        // Hoặc bạn có thể gọi PlayerDodgedBossSkill() từ script Player khi nó né thành công.

        StartCoroutine(AttackCoroutine(2.0f, () => {
            _isPerformingSpecialAbility = false;
            // Nếu skill này không bị né, reset _playerDodgedBossSkill
            _playerDodgedBossSkill = false; // Reset cờ sau khi skill được cast
        }));
        return NodeState.RUNNING;
    }

    // Hành động né tránh
    private NodeState Dodge()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (!CanDodge() || IsBossBusy()) return NodeState.FAILURE; // Kiểm tra cooldown và trạng thái bận

        Debug.Log("Boss: Dodging!");
        _isDodging = true; // Đặt cờ Boss đang né tránh
        _lastDodgeTime = Time.time; // Cập nhật thời điểm né tránh cuối cùng
        _aiPath.isStopped = true; // Dừng di chuyển của AIPath khi né tránh
        _animator?.SetTrigger("Dodge"); // Kích hoạt animation né tránh

        // Tính toán hướng né tránh: ra xa người chơi và ngẫu nhiên sang trái/phải
        Vector3 playerToBoss = (transform.position - targetPlayer.position).normalized;
        // Xoay hướng 90 độ sang trái hoặc phải so với hướng từ Player đến Boss
        Vector3 dodgeDirection = Quaternion.Euler(0, UnityEngine.Random.Range(-90f, 90f), 0) * playerToBoss;
        dodgeDirection.y = 0; // Đảm bảo chỉ né tránh trên mặt phẳng ngang

        // Bắt đầu Coroutine để xử lý di chuyển né tránh trong một khoảng thời gian ngắn
        StartCoroutine(DodgeMovementCoroutine(dodgeDirection, dodgeSpeed, 0.5f, () => _isDodging = false));

        return NodeState.RUNNING;
    }

    // Hành động sử dụng kỹ năng đặc biệt (tổng quát hơn Skill 1)
    private NodeState UseSpecialAbility()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (!CanUseSpecialAbility() || IsBossBusy()) return NodeState.FAILURE;

        Debug.Log("Boss: Using Generic Special Ability!");
        _isPerformingSpecialAbility = true;
        _lastSpecialAbilityTime = Time.time;
        _aiPath.isStopped = true;
        _animator?.SetTrigger("SpecialAbility");

        StartCoroutine(AttackCoroutine(2.0f, () => {
            _isPerformingSpecialAbility = false;
            _playerDodgedBossSkill = false; // Reset cờ nếu skill này không bị né
        }));
        return NodeState.RUNNING;
    }

    // Hành động hồi phục từ trạng thái choáng (Stagger)
    private NodeState RecoverFromStagger()
    {
        if (!_isStaggered) return NodeState.FAILURE;

        Debug.Log("Boss: Recovering from Stagger.");
        _aiPath.isStopped = true;
        _animator?.SetTrigger("RecoverStagger");

        StartCoroutine(StaggerRecoveryCoroutine(1.0f, () => _isStaggered = false));
        return NodeState.RUNNING;
    }

    // Hành động đỡ đòn hoặc parry
    private NodeState BlockOrParry()
    {
        if (targetPlayer == null) return NodeState.FAILURE;
        if (!IsPlayerCurrentlyAttacking() || IsBossBusy()) return NodeState.FAILURE;

        Debug.Log("Boss: Attempting Block/Parry!");
        _isParrying = true;
        _aiPath.isStopped = true;
        _animator?.SetTrigger("Parry");

        StartCoroutine(ParryWindowCoroutine(parryWindowDuration, () => _isParrying = false));
        return NodeState.RUNNING;
    }

    // --- CÁC COROUTINE HỖ TRỢ ---
    private IEnumerator AttackCoroutine(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
        if (!IsBossBusy()) _aiPath.isStopped = false;
    }

    private IEnumerator DodgeMovementCoroutine(Vector3 direction, float speed, float duration, Action onComplete)
    {
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            if (_characterController != null)
            {
                _characterController.Move(direction * speed * Time.deltaTime);
            }
            else
            {
                transform.position += direction * speed * Time.deltaTime;
            }
            yield return null;
        }
        onComplete?.Invoke();
        if (!IsBossBusy()) _aiPath.isStopped = false;
    }

    private IEnumerator StaggerRecoveryCoroutine(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
        if (!IsBossBusy()) _aiPath.isStopped = false;
    }

    private IEnumerator ParryWindowCoroutine(float duration, Action onComplete)
    {
        yield return new WaitForSeconds(duration);
        onComplete?.Invoke();
        if (!IsBossBusy()) _aiPath.isStopped = false;
    }

    // --- XỬ LÝ VA CHẠM (CHO ĐÒN ĐÁNH THƯỜNG) ---
    // Sử dụng OnTriggerStay hoặc OnControllerColliderHit để phát hiện va chạm liên tục
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Khi Boss chạm vào Player và Boss không bận, thực hiện đòn đánh thường
        if (hit.gameObject.CompareTag("Player") && !IsBossBusy())
        {
            // Trigger hành động tấn công thường
            // (Bạn có thể bỏ qua việc gọi trực tiếp PerformNormalMeleeAttack ở đây,
            // thay vào đó, để BT quyết định. Nhưng nếu muốn phản ứng tức thì khi va chạm,
            // thì đây là nơi để làm.)
            // Tuy nhiên, để tích hợp tốt với BT, tốt hơn là BT quyết định tấn công khi ở gần
            // và logic gây sát thương được kích hoạt qua animation event hoặc một collider/trigger riêng của đòn đánh.
            // Dòng này chỉ mang tính tham khảo nếu bạn muốn trigger hành động từ va chạm.
            // NodeState result = PerformNormalMeleeAttack();
            // if (result == NodeState.RUNNING) Debug.Log("Boss: Initiated Normal Melee Attack on collision.");
        }
    }

    // Nếu bạn sử dụng Trigger cho vùng tấn công của Boss thay vì va chạm trực tiếp với CharacterController
    /*
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !IsBossBusy())
        {
            // Tương tự như OnControllerColliderHit, cân nhắc việc kích hoạt hành động từ BT.
            // Nếu bạn có một collider riêng cho vùng tấn công của Boss, hãy dùng nó để gây sát thương.
        }
    }
    */

    // --- KHỞI TẠO CÂY HÀNH VI ---
    // Hàm này thiết lập cấu trúc của Behavior Tree cho Boss
    private void SetupBehaviorTree()
    {
        // Các Node điều kiện chung
        var isPlayerDetected = new ConditionNode(IsPlayerInDetectionRange);
        var isPlayerMeleeRange = new ConditionNode(IsPlayerInMeleeAttackRange);
        var isPlayerRangedRange = new ConditionNode(IsPlayerInRangedAttackRange);
        var isBossBusy = new ConditionNode(IsBossBusy);
        var canDodge = new ConditionNode(CanDodge);
        var canUseSpecial = new ConditionNode(CanUseSpecialAbility); // Cooldown được điều chỉnh theo nổi loạn
        var isPlayerAttacking = new ConditionNode(IsPlayerCurrentlyAttacking);
        var canCastSkill1 = new ConditionNode(CanCastSkill1); // Điều kiện mới: có thể dùng Skill 1
        var hasPlayerDodgedBossSkill = new ConditionNode(HasPlayerDodgedBossSkill); // Điều kiện mới: người chơi né skill

        // Các Node hành động cụ thể
        var moveTowardsPlayer = new ActionNode(MoveTowardsPlayer);
        var normalMeleeAttack = new ActionNode(PerformNormalMeleeAttack); // Đổi tên thành Normal Melee
        var rangedAttack = new ActionNode(PerformRangedAttack); // Vẫn giữ nếu có ranged attack
        var dodge = new ActionNode(Dodge);
        var useSkill1 = new ActionNode(UseSkill1); // Hành động Skill 1
        var useGenericSpecialAbility = new ActionNode(UseSpecialAbility); // Hành động skill đặc biệt chung
        var recoverStagger = new ActionNode(RecoverFromStagger);
        var blockOrParry = new ActionNode(BlockOrParry);

        // Nhánh ưu tiên cao nhất: Xử lý choáng (Stagger)
        var staggerBranch = new SequenceNode(new List<Node>
        {
            new ConditionNode(() => _isStaggered),
            recoverStagger
        });

        // Nhánh ưu tiên cao thứ hai: Phản ứng với đòn tấn công của người chơi (né tránh/đỡ đòn)
        var reactToPlayerAttackBranch = new SequenceNode(new List<Node>
        {
            new ConditionNode(() => !isBossBusy.Evaluate().Equals(NodeState.SUCCESS)),
            isPlayerAttacking,
            new SelectorNode(new List<Node>
            {
                new SequenceNode(new List<Node> { canDodge, dodge }),
                blockOrParry
            })
        });

        // Nhánh ưu tiên cao thứ ba: Kích hoạt Skill 1 khi Player đánh thường đòn 2
        var triggerSkill1Branch = new SequenceNode(new List<Node>
        {
            new ConditionNode(() => !isBossBusy.Evaluate().Equals(NodeState.SUCCESS)),
            canCastSkill1, // Nếu Player đã đánh đòn 2 và skill không cooldown
            useSkill1
        });

        // Nhánh ưu tiên cao thứ tư: Phản ứng khi người chơi né skill của Boss
        var reactToDodgeBranch = new SequenceNode(new List<Node>
        {
            new ConditionNode(() => !isBossBusy.Evaluate().Equals(NodeState.SUCCESS)),
            hasPlayerDodgedBossSkill, // Nếu người chơi vừa né được skill của Boss
            new SelectorNode(new List<Node> // Boss sẽ tìm người chơi để tấn công hoặc tung skill bất kỳ
            {
                new SequenceNode(new List<Node> { // Ưu tiên tấn công thường nếu ở gần
                    isPlayerMeleeRange,
                    normalMeleeAttack
                }),
                new SequenceNode(new List<Node> { // Hoặc tung skill ngẫu nhiên nếu có thể
                    canUseSpecial,
                    useGenericSpecialAbility
                }),
                moveTowardsPlayer // Nếu không, tiếp tục đuổi theo
            })
        });

        // Định nghĩa hành vi cụ thể theo từng giai đoạn của Boss
        Node phaseSpecificBehavior;
        switch (currentPhase)
        {
            case BossPhase.Phase1:
                Debug.Log("Boss Phase 1 Behavior Activated.");
                phaseSpecificBehavior = new SelectorNode(new List<Node>
                {
                    // Ưu tiên tấn công gần nếu chạm collider (di chuyển về để chạm)
                    new SequenceNode(new List<Node> { isPlayerMeleeRange, normalMeleeAttack }),
                    new SequenceNode(new List<Node> { isPlayerRangedRange, rangedAttack }), // Vẫn có ranged nếu muốn
                    new SequenceNode(new List<Node> { canDodge, dodge }),
                    new SequenceNode(new List<Node> { canUseSpecial, useGenericSpecialAbility }),
                    moveTowardsPlayer
                });
                break;
            case BossPhase.Phase2:
                Debug.Log("Boss Phase 2 Behavior Activated.");
                phaseSpecificBehavior = new SelectorNode(new List<Node>
                {
                    new SequenceNode(new List<Node> { canUseSpecial, useGenericSpecialAbility }), // Ưu tiên skill
                    new SequenceNode(new List<Node> { canDodge, dodge }), // Né tránh thường xuyên hơn
                    new SequenceNode(new List<Node> { isPlayerMeleeRange, normalMeleeAttack }),
                    new SequenceNode(new List<Node> { isPlayerRangedRange, rangedAttack }),
                    moveTowardsPlayer
                });
                break;
            case BossPhase.Phase3: // Trạng thái "Nổi loạn"
                Debug.Log("Boss Phase 3 (Enraged) Behavior Activated.");
                // Tốc độ di chuyển đã được cập nhật ở Tick()
                // Cooldown kỹ năng đã được điều chỉnh trong CanUseSpecialAbility()
                phaseSpecificBehavior = new SelectorNode(new List<Node>
                {
                    // Spam kỹ năng đặc biệt
                    new SequenceNode(new List<Node> { canUseSpecial, useGenericSpecialAbility }),
                    // Tấn công cận chiến mạnh hơn/thường xuyên hơn
                    new SequenceNode(new List<Node> { isPlayerMeleeRange, normalMeleeAttack }),
                    // Vẫn có thể né tránh nhưng ít ưu tiên hơn
                    new SequenceNode(new List<Node> { canDodge, dodge }),
                    // Luôn đuổi theo
                    moveTowardsPlayer
                });
                break;
            default:
                Debug.LogWarning("Unknown Boss Phase. Defaulting to Phase 1 Behavior.");
                goto case BossPhase.Phase1;
        }

        // Cây hành vi gốc (_rootNode) của Boss
        _rootNode = new SelectorNode(new List<Node>
        {
            staggerBranch,                  // 1. Ưu tiên cao nhất: Xử lý choáng
            triggerSkill1Branch,            // 2. Kích hoạt Skill 1 khi Player đánh thường đòn 2
            reactToDodgeBranch,             // 3. Phản ứng khi Player né được skill của Boss
            reactToPlayerAttackBranch,      // 4. Phản ứng với đòn tấn công của người chơi (Parry/Block)
            new SequenceNode(new List<Node>
            {
                new ConditionNode(() => !isBossBusy.Evaluate().Equals(NodeState.SUCCESS)), // Điều kiện: Chỉ thực hiện nhánh này nếu Boss không bận
                isPlayerDetected, // Điều kiện: Phát hiện người chơi
                phaseSpecificBehavior // Thực hiện hành vi cụ thể của Phase hiện tại
            }),
            new ActionNode(() => { // Hành động mặc định: Idle
                Debug.Log("Boss: Idling...");
                _animator?.SetBool("isMoving", false);
                _aiPath.isStopped = true;
                return NodeState.RUNNING;
            })
        });

        Debug.Log("Behavior Tree setup complete for current phase: " + currentPhase);
    }

    // Hàm này được gọi khi GameObject bị hủy (ví dụ: Boss chết hoặc chuyển cảnh)
    private void OnDestroy()
    {
        if (EnemyAIManager.Instance != null)
        {
            EnemyAIManager.Instance.UnregisterEnemy(this);
        }
    }
}