using UnityEngine;
using Pathfinding;

public class BossMovementAStar : MonoBehaviour
{
    private AIPath _aiPath;
    private Seeker _seeker;
    private BossBlackboard _blackboard;
    void Awake()
    {
        _aiPath = GetComponent<AIPath>();
        _seeker = GetComponent<Seeker>();
        _blackboard = GetComponent<BossBlackboard>();
    }

    public void MoveTo(Vector3 position)
    {
        Debug.Log("[BossMovementAStar] Moving to: " + position);
        if (_aiPath != null)
        {
            _aiPath.destination = position;
            _aiPath.canMove = true;
            _blackboard.animator?.SetFloat("Speed", 1f); // Set walking animation
        }
    }

    public void StopMoving()
    {
        if (_aiPath != null)
        {
            _aiPath.canMove = false;
            _blackboard.animator?.SetFloat("Speed", 0f); // Idle
        }
    }

}