using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class CheckDistance : Conditional
{
    public SharedTransform playerTransform;
    public float minDistance = 0f;
    public float maxDistance = 100f;

    public override TaskStatus OnUpdate()
    {
        if (playerTransform.Value == null) return TaskStatus.Failure;
        float dist = Vector3.Distance(transform.position, playerTransform.Value.position);
        if (dist >= minDistance && dist <= maxDistance)
        {
            //Debug.Log($"[CheckDistance] Distance {dist} in range ({minDistance}-{maxDistance})");
            return TaskStatus.Success;
        }
        return TaskStatus.Failure;
    }
}
