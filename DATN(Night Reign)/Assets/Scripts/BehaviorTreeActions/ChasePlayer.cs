using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor.Experimental.GraphView;

public class ChasePlayer : Action
{
    //public SharedTransform playerTransform;
    //private BossMovementAStar movement;
    //private BossBlackboard _blackboard;
    //public float stopDistance = 2.5f; // chỉnh đúng khoảng tấn công

    //public override void OnStart()
    //{
    //    playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    //    movement = GetComponent<BossMovementAStar>();
    //    _blackboard = GetComponent<BossBlackboard>();
    //    Debug.Log("[ChasePlayer] Start chasing player.");
    //}

    //public override TaskStatus OnUpdate()
    //{
    //    /*if (playerTransform.Value == null)
    //    {
    //        Debug.Log("[ChasePlayer] No player found.");
    //        return TaskStatus.Failure;
    //    }
    //    movement.MoveTo(playerTransform.Value.position);
    //    Debug.Log("[ChasePlayer] Moving toward player.");
    //    return TaskStatus.Running;*/

    //    if (playerTransform.Value == null) return TaskStatus.Failure;

    //    float dist = Vector3.Distance(transform.position, playerTransform.Value.position);
    //    if (dist <= stopDistance)
    //    {
    //        movement.StopMoving();
    //        Debug.Log("[ChasePlayer] Reached attack range.");
    //        return TaskStatus.Success;
    //    }

    //    movement.MoveTo(playerTransform.Value.position);
    //    return TaskStatus.Running;
    //}

    public SharedTransform playerTransform;
    private BossMovementAStar movement;
    private BossBlackboard _blackboard;

    public float stopDistance = 2.5f; // khoảng cách dừng để tấn công
    public float rotationSpeed = 120f; // tốc độ xoay kiểu Souls (độ/giây)

    public override void OnStart()
    {
        movement = GetComponent<BossMovementAStar>();
        _blackboard = GetComponent<BossBlackboard>();

        if (playerTransform.Value == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform.Value = player.transform;
        }

        Debug.Log("[ChasePlayer] Start chasing player.");
    }

    public override TaskStatus OnUpdate()
    {
        if (_blackboard.isDead) return TaskStatus.Failure;

        if (playerTransform.Value == null)
            return TaskStatus.Failure;

        float dist = Vector3.Distance(transform.position, playerTransform.Value.position);

        // Xoay về hướng player (trên mặt phẳng ngang)
        Vector3 direction = (playerTransform.Value.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        if (dist <= stopDistance)
        {
            movement.StopMoving();
            Debug.Log("[ChasePlayer] Reached attack range.");
            return TaskStatus.Success;
        }

        // Di chuyển tới player
        movement.MoveTo(playerTransform.Value.position);
        return TaskStatus.Running;
    }
}