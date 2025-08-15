using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEditor.Experimental.GraphView;

public class AttackMelee : Action
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
                Debug.Log("[AttackMelee] Still in cooldown.");
                return TaskStatus.Failure;
            }
            Debug.Log("[AttackMelee] Executing melee attack.");
            combat.AttackNormal();
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

    private void LookAtPlayer()
    {
        Transform boss = transform;
        Transform player = blackboard.player;

        Vector3 direction = (player.position - boss.position).normalized;
        direction.y = 0; // Xoay theo mặt phẳng ngang

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            boss.rotation = lookRotation;
            Debug.Log("[AttackMelee] Boss rotated to face player.");
        }
    }
}