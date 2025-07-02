using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class EnterPhaseTwo : Action
{
    private BossBlackboard blackboard;

    public override void OnStart()
    {
        blackboard = GetComponent<BossBlackboard>();
    }

    public override TaskStatus OnUpdate()
    {
        if (!blackboard.isInSecondPhase && blackboard.currentHP <= blackboard.maxHP - 1000f)
        {
            blackboard.isInSecondPhase = true;
            Debug.Log("[EnterPhaseTwo] Boss entered Phase 2!");
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}