using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class DetectPlayer : Conditional
{
    public float detectionRange = 15f;
    public LayerMask playerLayer;
    public SharedTransform playerTransform;

    public override TaskStatus OnUpdate()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        if (hits.Length > 0)
        {
            playerTransform.Value = hits[0].transform;
            Debug.Log("[DetectPlayer] Player detected.");
            return TaskStatus.Success;
        }
        Debug.Log("[DetectPlayer] No player in range.");
        return TaskStatus.Failure;
    }
}