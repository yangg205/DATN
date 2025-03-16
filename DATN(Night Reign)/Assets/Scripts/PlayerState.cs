using Unity.VisualScripting;
using UnityEngine;

public class PlayerState : MonoBehaviour
{
    //Init
    public GameObject activeModel;
    //Inputs
    public float vertical;
    public float horizontal;
    public float moveAmount;
    public Vector3 moveDir;
    public bool rt,rb,lt,lb;
    public bool rollInput;

    //Stats
    public float moveSpeed = 2;
    public float runSpeed = 3.5f;
    public float rotateSpeed = 5;
    public float toGround = 0.5f;
    public float rollSpeed = 1;
    //States
    public bool run;
    public bool onGround;
    public bool lockOn;
    public bool inAction;
    public bool canMove;
    public bool isTwoHanded;
    //Other(Duyen)
    public DemoQuaiCuaDuyenDeLockOn lockonTarget;
    [HideInInspector]
    public Animator anim;
    [HideInInspector]
    public Rigidbody rigid;
    [HideInInspector]
    public AnimatorHook a_hook;
    [HideInInspector]
    public float delta;
    [HideInInspector]
    public LayerMask ignoreLayers;
    float _actionDelay;

    public void Init()
    {
        SetUpAnimator();
        rigid = GetComponent<Rigidbody>();
        rigid.angularDamping = 999;
        rigid.linearDamping = 4;
        rigid.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        a_hook = activeModel.AddComponent<AnimatorHook>();
        a_hook.Init(this);
        gameObject.layer = 8;
        ignoreLayers = ~(1 << 9);

    }
    void SetUpAnimator()
    {
        if(activeModel == null)
        {
            anim.GetComponentInChildren<Animator>();
            if(anim == null)
            {
                Debug.Log("no model found");
            }
            else
            {
                activeModel = anim.gameObject;
            }
        }
        if(anim == null)
            anim = activeModel.GetComponent<Animator>();
        anim.applyRootMotion = false;
    }
    public void FixedTick(float d)
    {
        delta = d;

        DetectAction();
        if (inAction)
        {
            anim.applyRootMotion = true;
            _actionDelay += delta;
            if(_actionDelay > 0.3f)
            {
                inAction = false;
                _actionDelay = 0;
            }
            else
            {
                return;
            }
        }
        canMove = anim.GetBool("canMove");
        if(!canMove)
            return;
        a_hook.rm_multi = 1;
        HandleRolls();
        anim.applyRootMotion = false;
        rigid.linearDamping = (moveAmount > 0 || onGround == false) ? 0 : 4;
        float targetSpeed = moveSpeed;
        if(run)
            targetSpeed = runSpeed;
        if(onGround)
            rigid.linearVelocity = moveDir * (targetSpeed * moveAmount);
        if(run)
            lockOn = false;

            Vector3 targetDir = (lockOn == false)? moveDir 
                : lockonTarget.transform.position - transform.position;
            targetDir.y = 0;
            if (targetDir == Vector3.zero)
                targetDir = transform.forward;
            Quaternion tr = Quaternion.LookRotation(targetDir);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, delta * moveAmount * rotateSpeed);
            transform.rotation = targetRotation;

        anim.SetBool("lockon", lockOn);
        if (lockOn == false)
            HandleMovementAnimations();
        else
            HandleLockOnAnimations(moveDir);
    }
    public void DetectAction()
    {
        if(canMove == false)
            return;
        if (rb == false && rt == false && lt == false && lb == false)
            return;
        string targetAnim = null;
        if (rb)
            targetAnim = "OneHand_Up_Attack_1_InPlace";
        if (rt)
            targetAnim = "OneHand_Up_Attack_2_InPlace";
        if (lt)
            targetAnim = "OneHand_Up_Attack_3_InPlace";
        if (lb)
            targetAnim = "Longs_Attack_D";
        if (string.IsNullOrEmpty(targetAnim))
            return;
        canMove = false;
        inAction = true;
        anim.CrossFade(targetAnim, 0.2f);
        //rigid.linearVelocity = Vector3.zero;
    }
    public void Tick(float d)
    {
        delta = d;
        onGround = OnGround();
        anim.SetBool("onGround", onGround);
    }
    void HandleRolls()
    {
        if (!rollInput)
            return;
        float v = vertical;
        float h = horizontal;
        v = (moveAmount > 0.3f) ? 1 : 0;
        h = 0;

        //if(lockOn == false)
        //{
        //    v = (moveAmount > 0.3f) ? 1 : 0;
        //    h = 0;
        //}
        //else
        //{
        //    if (Mathf.Abs(v) < 0.3f)
        //        v = 0;
        //    if (Mathf.Abs(h) < 0.3f)
        //        h = 0;
        //}
        if(v != 0)
        {
            if (moveDir == Vector3.zero)
                moveDir = transform.forward;
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = targetRot;
        }
        a_hook.rm_multi = rollSpeed;
        anim.SetFloat("vertical", v);
        anim.SetFloat("horizontal", h);

        canMove = false;
        inAction = true;
        anim.CrossFade("Rolls", 0.2f);
    }
    void HandleMovementAnimations()
    {
        anim.SetBool("run", run);
        anim.SetFloat("vertical", moveAmount, 0.04f, delta);
    }
    void HandleLockOnAnimations(Vector3 moveDir)
    {
        Vector3 relativeDir = transform.InverseTransformDirection(moveDir);
        float h = relativeDir.x;
        float v = relativeDir.z;

        anim.SetFloat("vertical", v, 0.2f, delta);
        anim.SetFloat("horizontal", h, 0.2f, delta);
    }
    public bool OnGround()
    {
        bool r = false;
        Vector3 origin = transform.position + (Vector3.up * toGround);
        Vector3 dir = -Vector3.up;
        float dis = toGround + 0.3f;
        RaycastHit hit;
        Debug.DrawLine(origin, dir * dis);
        if(Physics.Raycast(origin, dir, out hit, dis, ignoreLayers))
        {
            r = true;
            Vector3 targetPosition = hit.point;
            transform.position = targetPosition;
        }

        return r;
    }
    public void HandleTwoHanded()
    {
        anim.SetBool("two_handed", isTwoHanded);
    }
}
