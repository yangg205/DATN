using Pathfinding;
using UnityEditor.Experimental.GraphView;
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
        //Debug.Log("[BossCombat] Normal melee attack triggered.");

        int animationChoice = UnityEngine.Random.Range(1, 3);
        switch (animationChoice)
        {
            case 1:
                _blackboard.animator?.SetTrigger("Attack1");
                break;
            case 2:
                _blackboard.animator?.SetTrigger("Attack2");
                break;
        }

        if (_blackboard.isInSecondPhase)
        {
            int animationChoice1 = UnityEngine.Random.Range(1, 3);
            switch (animationChoice1)
            {
                case 1:
                    _blackboard.animator?.SetTrigger("AttackPhase2");
                    break;
                case 2:
                    _blackboard.animator?.SetTrigger("AttackPhase2,1");
                    break;
            }
        }
       
    }

    public void CastSpell()
    {
        int animationChoice = UnityEngine.Random.Range(1, 3);
        switch (animationChoice)
        {
            case 1:
                _blackboard.animator?.SetTrigger("Attack1");
                break;
            case 2:
                _blackboard.animator?.SetTrigger("JumpToPlayer");
                break;
        } 
    }

    public void Evade()
    {
        Debug.Log("[BossCombat] Evade triggered.");
        // Add dash or animation
    }


}
