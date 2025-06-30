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
        if (!blackboard.isInSecondPhase && blackboard.currentHP < blackboard.maxHP * 0.5f)
        {
            blackboard.isInSecondPhase = true;
            Debug.Log("[EnterPhaseTwo] Boss entered Phase 2!");
            return TaskStatus.Success;
        }
        Debug.Log("[EnterPhaseTwo] Boss is already in Phase 2 or HP not low enough.");
        return TaskStatus.Failure;
    }
}