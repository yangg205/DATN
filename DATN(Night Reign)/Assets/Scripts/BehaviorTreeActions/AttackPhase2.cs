using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor.Experimental.GraphView;
using BehaviorDesigner.Runtime.Tasks.Movement;
public class AttackPhase2 : Action
{
    private BossCombat combat;
    private BossBlackboard blackboard;
    public SharedFloat cooldownTime = 2.5f;
    private bool hasAttacked = false;
    private float attackStartTime = -999f;

    public override void OnStart()
    {
        combat = GetComponent<BossCombat>();
        blackboard = GetComponent<BossBlackboard>();
        hasAttacked = false;
    }

    public override TaskStatus OnUpdate()
    {
        if (blackboard == null) return TaskStatus.Failure;

        float currentTime = Time.time;

        if (!hasAttacked)
        {
            if (currentTime - blackboard.timeSinceLastAttack < cooldownTime.Value)
            {
                Debug.Log("[Attack2] Still in cooldown.");
                return TaskStatus.Failure;
            }
            Debug.Log("[Attack2] Executing attack2.");
            combat.Atk2();
            blackboard.timeSinceLastAttack = currentTime;
            attackStartTime = currentTime;
            hasAttacked = true;
            return TaskStatus.Running;
        }

        if (currentTime - attackStartTime >= cooldownTime.Value)
        {
            hasAttacked = false;
            return TaskStatus.Success;
        }

        return TaskStatus.Running;
    }

}
