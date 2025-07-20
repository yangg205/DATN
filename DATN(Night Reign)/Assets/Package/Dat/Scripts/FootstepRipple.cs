using UnityEngine;

public class FootstepRipple : MonoBehaviour
{
    [Header("Raycast Foot Positions")]
    public Transform footL;
    public Transform footR;

    [Header("Ripple Settings")]
    public ParticleSystem rippleEffect;
    public LayerMask waterLayer;
    public float raycastDistance = 0.2f;
    public float stepRate = 0.3f;

    private float lastStepL;
    private float lastStepR;

    void FixedUpdate()
    {
        CheckFootstep(footL, ref lastStepL);
        CheckFootstep(footR, ref lastStepR);
    }

    void CheckFootstep(Transform foot, ref float lastStepTime)
    {
        if (Time.time - lastStepTime > stepRate)
        {
            Vector3 rayOrigin = foot.position + Vector3.up * 0.05f;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, waterLayer))
            {
                EmitRipple(hit.point);
                lastStepTime = Time.time;
            }
        }
    }

    void EmitRipple(Vector3 position)
    {
        if (rippleEffect == null) return;

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = position
        };

        rippleEffect.Emit(emitParams, 1);
    }
}
