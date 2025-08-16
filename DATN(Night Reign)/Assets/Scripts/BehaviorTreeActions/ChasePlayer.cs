using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor.Experimental.GraphView;

public class ChasePlayer : Action
{
    public SharedTransform playerTransform;
    private BossMovementAStar movement;
    private BossBlackboard _blackboard;
    public float stopDistance = 2.5f; // chỉnh đúng khoảng tấn công

    public override void OnStart()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        movement = GetComponent<BossMovementAStar>();
        _blackboard = GetComponent<BossBlackboard>();
        Debug.Log("[ChasePlayer] Start chasing player.");
    }

    public override TaskStatus OnUpdate()
    {
        /*if (playerTransform.Value == null)
        {
            Debug.Log("[ChasePlayer] No player found.");
            return TaskStatus.Failure;
        }
        movement.MoveTo(playerTransform.Value.position);
        Debug.Log("[ChasePlayer] Moving toward player.");
        return TaskStatus.Running;*/

        if (playerTransform.Value == null) return TaskStatus.Failure;

        float dist = Vector3.Distance(transform.position, playerTransform.Value.position);
        if (dist <= stopDistance)
        {
            movement.StopMoving();
            Debug.Log("[ChasePlayer] Reached attack range.");
            return TaskStatus.Success;
        }

        movement.MoveTo(playerTransform.Value.position);
        return TaskStatus.Running;
    }
}