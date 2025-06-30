using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class ChasePlayer : Action
{
    public SharedTransform playerTransform;
    private BossMovementAStar movement;

    public override void OnStart()
    {
        movement = GetComponent<BossMovementAStar>();
        Debug.Log("[ChasePlayer] Start chasing player.");
    }

    public override TaskStatus OnUpdate()
    {
        if (playerTransform.Value == null)
        {
            Debug.Log("[ChasePlayer] No player found.");
            return TaskStatus.Failure;
        }
        movement.MoveTo(playerTransform.Value.position);
        Debug.Log("[ChasePlayer] Moving toward player.");
        return TaskStatus.Running;
    }
}