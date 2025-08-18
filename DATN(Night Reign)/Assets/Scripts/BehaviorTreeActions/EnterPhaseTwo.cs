using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class EnterPhaseTwo : Conditional
{
    private BossBlackboard blackboard;

    public override void OnStart()
    {
        blackboard = GetComponent<BossBlackboard>();
    }

    public override TaskStatus OnUpdate()
    {
        if (!blackboard.isInSecondPhase && blackboard.currentHP <= blackboard.maxHP * 0.5f)
        {
            blackboard.isInSecondPhase = true;
            blackboard.animator.SetTrigger("ChangePhase2");
            Debug.Log("[EnterPhaseTwo] Boss entered Phase 2!");
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}