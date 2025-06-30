using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class AttackRanged : Action
{
    private BossCombat combat;

    public override void OnStart()
    {
        combat = GetComponent<BossCombat>();
    }

    public override TaskStatus OnUpdate()
    {
        Debug.Log("[AttackRanged] Casting ranged spell.");
        combat.CastSpell();
        return TaskStatus.Success;
    }
}