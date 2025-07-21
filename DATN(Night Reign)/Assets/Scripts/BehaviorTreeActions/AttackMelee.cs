using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class AttackMelee : Action
{
    private BossCombat combat;
    private BossBlackboard blackboard;

    public SharedFloat cooldownTime = 2.5f;

    public override void OnStart()
    {
        combat = GetComponent<BossCombat>();
        blackboard = GetComponent<BossBlackboard>();
    }

    public override TaskStatus OnUpdate()
    {
        if (blackboard == null) return TaskStatus.Failure;

        float currentTime = Time.time;
        if (currentTime - blackboard.timeSinceLastAttack < cooldownTime.Value)
        {
            Debug.Log("[AttackMelee] Still in cooldown.");
            return TaskStatus.Running;
        }

        Debug.Log("[AttackMelee] Executing melee attack.");
        combat.AttackNormal();
        blackboard.timeSinceLastAttack = currentTime;
        return TaskStatus.Success;
    }
}