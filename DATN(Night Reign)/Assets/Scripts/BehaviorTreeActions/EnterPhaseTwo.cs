using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class EnterPhaseTwo : Action
{
    private BossBlackboard blackboard;
    private BossCombat combat;

    public override void OnStart()
    {
        blackboard = GetComponent<BossBlackboard>();
        combat = GetComponent<BossCombat>();

    }

    public override TaskStatus OnUpdate()
    {
        if (!blackboard.isInSecondPhase && blackboard.currentHP <= blackboard.maxHP * 0.35f)
        {
            blackboard.isInSecondPhase = true;
            blackboard.animator.SetTrigger("ChangePhase2");
            Debug.Log("[EnterPhaseTwo] Boss entered Phase 2!");
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}