using UnityEngine;

public class BossCombat : MonoBehaviour
{
    private BossBlackboard _blackboard;

    void Awake()
    {
        _blackboard = GetComponent<BossBlackboard>();
    }
    public void AttackNormal()
    {
        if (_blackboard.isInSecondPhase)
        {
            // Random attack trong Phase 2
            int animationChoice = UnityEngine.Random.Range(1, 3); // 1 hoặc 2
            switch (animationChoice)
            {
                case 1:
                    _blackboard.animator?.SetTrigger("AttackPhase2");
                    break;
                case 2:
                    _blackboard.animator?.SetTrigger("AttackPhase2_1");
                    break;
            }
        }
        else
        {
            // Random attack trong Phase 1
            int animationChoice = UnityEngine.Random.Range(1, 3); // 1 hoặc 2
            switch (animationChoice)
            {
                case 1:
                    _blackboard.animator?.SetTrigger("Attack1");
                    break;
                case 2:
                    _blackboard.animator?.SetTrigger("Attack2");
                    break;
            }
        }
    }

    public void CastSpell()
    {

        _blackboard.animator?.SetTrigger("JumpToPlayer");

        //if (_blackboard.isInSecondPhase) return;
        //int animationChoice = UnityEngine.Random.Range(1, 3);
        //switch (animationChoice)
        //{
        //    case 1:
        //        _blackboard.animator?.SetTrigger("Attack1///");
        //        break;
        //    case 2:
        //        _blackboard.animator?.SetTrigger("JumpToPlayer");
        //        break;
        //} 
    }

    public void Evade()
    {
        Debug.Log("[BossCombat] Evade triggered.");

    }

   
}
