using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public bool lockOn;
    public float followSpeed = 9;
    public float mouseSpeed = 2;
    public float controllerSpeed = 7;
    public Transform target;
    public Transform lockonTarget;
    [HideInInspector]
    public Transform pivot;
    [HideInInspector]
    Transform camTrans;
    float turnSmoothing = .1f;
    public float minAngle = -35;
    public float maxAngle = 35;
    float smouthX;
    float smouthY;
    float smouthXvelocity;
    float smouthYvelocity;
    public float lookAngle;
    public float tiltAngle;
    public void Init(Transform t)
    {
        target = t;
        camTrans = Camera.main.transform;
        pivot = camTrans.parent;
    }
    public void Tick(float d)
    {
        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");

        float c_h = Input.GetAxis("RightAxis X");
        float c_v = Input.GetAxis("RightAxis Y");
        float targetSpeed = mouseSpeed;
        if(c_h != 0 ||  c_v != 0)
        {
            h = c_h;
            v = c_v;
            targetSpeed = controllerSpeed;  
        }
        FollowTarget(d);
        HandleRotation(d, v, h, targetSpeed);
    }
    void FollowTarget(float d)
    {
        float speed = d * followSpeed;
        Vector3 targetPosition = Vector3.Lerp(transform.position, target.position, speed);
        transform.position = targetPosition;
    }
    void HandleRotation(float d, float v, float h, float targetSpeed)
    {
        if(turnSmoothing > 0)
        {
            smouthX = Mathf.SmoothDamp(smouthX, h, ref smouthXvelocity, turnSmoothing);
            smouthY = Mathf.SmoothDamp(smouthY, v, ref smouthYvelocity, turnSmoothing);
        }
        else
        {
            smouthX = h;
            smouthY = v;
        }
        tiltAngle -= smouthY * targetSpeed;
        tiltAngle = Mathf.Clamp(tiltAngle, minAngle, maxAngle);
        pivot.localRotation = Quaternion.Euler(tiltAngle, 0, 0);

        if (lockOn && lockonTarget != null)
        {
            Vector3 targetDir = lockonTarget.position - transform.position;
            targetDir.Normalize();
            //targetDir.y = 0;
            if(targetDir == Vector3.zero)
                targetDir = transform.forward;
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, d * 9);
            lookAngle = transform.eulerAngles.y;
            return;
        }
        lookAngle += smouthX * targetSpeed;
        transform.rotation = Quaternion.Euler(0, lookAngle, 0);
      
    }
    public static CameraManager singleton;
    void Awake()
    {
        singleton = this;
    }
}
