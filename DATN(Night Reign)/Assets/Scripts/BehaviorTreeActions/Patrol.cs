using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class Patrol : Action
{
    public Transform[] waypoints;
    private int currentIndex = 0;
    private BossMovementAStar movement;
    private BossBlackboard blackboard;

    public float arriveThreshold = 1.5f;

    float timer;

    public override void OnStart()
    {
        timer = 0; 

        movement = GetComponent<BossMovementAStar>();
        blackboard = GetComponent<BossBlackboard>();

        if (waypoints.Length == 0)
            Debug.LogWarning("[Patrol] No waypoints assigned.");

        //if (blackboard != null && blackboard.animator != null)
        //    blackboard.animator.SetFloat("Speed", 1f); // Gán trạng thái đi bộ
    }

    public override TaskStatus OnUpdate()
    {

        timer += Time.deltaTime;

        if (waypoints.Length == 0)
        {
            if (timer > 4f) return TaskStatus.Failure;
            return TaskStatus.Running;
        }



        movement.MoveTo(waypoints[currentIndex].position);

        float dist = Vector3.Distance(transform.position, waypoints[currentIndex].position);
        if (dist < arriveThreshold)
        {
            Debug.Log("[Patrol] Reached waypoint " + currentIndex);
            currentIndex = (currentIndex + 1) % waypoints.Length;
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        if (blackboard != null && blackboard.animator != null)
            blackboard.animator.SetFloat("Speed", 0f); // Dừng di chuyển
    }
}