using UnityEngine;
using Pathfinding;

public class BossMovementAStar : MonoBehaviour
{
    private AIPath _aiPath;
    private Seeker _seeker;

    void Awake()
    {
        _aiPath = GetComponent<AIPath>();
        _seeker = GetComponent<Seeker>();
    }

    public void MoveTo(Vector3 position)
    {
        Debug.Log("[BossMovementAStar] Moving to: " + position);
        if (_aiPath != null)
        {
            _aiPath.destination = position;
            _aiPath.canMove = true;
        }
    }
}