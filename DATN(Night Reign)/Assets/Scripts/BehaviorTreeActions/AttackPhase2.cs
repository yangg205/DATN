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


    public override void OnStart()
    {
        combat = GetComponent<BossCombat>();
        blackboard = GetComponent<BossBlackboard>();

    }

    public override TaskStatus OnUpdate()
    {
        combat.Atk2();
        return TaskStatus.Success;
    }

}
