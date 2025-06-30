using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class Patrol : Action
{
    public Transform[] waypoints;
    private int currentIndex = 0;
    private BossMovementAStar movement;

    public float arriveThreshold = 1.5f;

    public override void OnStart()
    {
        movement = GetComponent<BossMovementAStar>();
        if (waypoints.Length == 0) Debug.LogWarning("[Patrol] No waypoints assigned.");
    }

    public override TaskStatus OnUpdate()
    {
        if (waypoints.Length == 0) return TaskStatus.Failure;

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
}