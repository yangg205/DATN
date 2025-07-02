using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class EnterPhaseThree : Action
{
    private BossBlackboard blackboard;

    public override void OnStart()
    {
        blackboard = GetComponent<BossBlackboard>();
    }

    public override TaskStatus OnUpdate()
    {
        if (!blackboard.isInThirdPhase && blackboard.currentHP <= blackboard.maxHP - 2000f)
        {
            blackboard.isInThirdPhase = true;
            Debug.Log("[EnterPhaseThree] Boss entered Phase 3!");
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}
