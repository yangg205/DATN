using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class EvadeIfLowStamina : Action
{
    private BossBlackboard blackboard;
    private BossCombat combat;

    public float threshold = 20f;

    public override void OnStart()
    {
        blackboard = GetComponent<BossBlackboard>();
        combat = GetComponent<BossCombat>();
    }

    public override TaskStatus OnUpdate()
    {
        if (blackboard.stamina < threshold)
        {
            Debug.Log("[EvadeIfLowStamina] Stamina low, evading.");
            combat.Evade();
            return TaskStatus.Success;
        }
        Debug.Log("[EvadeIfLowStamina] Stamina sufficient, no need to evade.");
        return TaskStatus.Failure;
    }
}