using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class AttackMelee : Action
{
    private BossCombat combat;

    public override void OnStart()
    {
        combat = GetComponent<BossCombat>();
    }

    public override TaskStatus OnUpdate()
    {
        Debug.Log("[AttackMelee] Executing melee attack.");
        combat.AttackNormal();
        return TaskStatus.Success;
    }
}