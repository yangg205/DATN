using ND;
using System.Collections;
using UnityEngine;

public class MountSystem : MonoBehaviour
{
    [Header("References")]
    public Transform mountSeat;                // Vị trí ngồi trên thú
    public GameObject mountObject;             // Con thú cưỡi
    public MonoBehaviour mountController;      // Script điều khiển thú cưỡi (nếu có)

    [Header("Settings")]
    public Vector3 dismountOffset = new Vector3(2, 0, 0); // Offset khi xuống ngựa
    public float mountAnimTime = 1.2f;
    public float dismountAnimTime = 1f;

    [Header("Debug")]
    public bool isMounted = false;

    private InputHandler inputHandler;
    private AnimatorHandler animatorHandler;
    private bool isTransitioning = false;

    private void Start()
    {
        inputHandler = GetComponent<InputHandler>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();

        if (mountController != null)
            mountController.enabled = false; // Tắt điều khiển thú cưỡi ban đầu
    }

    private void Update()
    {
        if (inputHandler == null || isTransitioning)
            return;

        if (inputHandler.interact_input)
        {
            inputHandler.interact_input = false; // reset input sau khi bấm
            TryMountOrDismount();
        }
    }

    public void TryMountOrDismount()
    {
        if (isMounted)
            StartCoroutine(Dismount());
        else
            StartCoroutine(Mount());
    }

    private IEnumerator Mount()
    {
        isTransitioning = true;

        // Play animation Mount
        if (animatorHandler != null)
            animatorHandler.anim.SetTrigger("triggerMount");

        yield return new WaitForSeconds(mountAnimTime);

        // Thực hiện gắn lên thú
        transform.SetParent(mountObject.transform);

        if (TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;
        if (TryGetComponent(out Collider col)) col.enabled = false;

        transform.position = mountSeat.position;
        transform.rotation = Quaternion.Euler(0, mountSeat.eulerAngles.y, 0);

        if (mountController != null)
            mountController.enabled = true;

        if (inputHandler != null)
/*            inputHandler.isInputDisabled = true;
*/
        if (animatorHandler != null)
        {
            animatorHandler.UpdateAnimatorValues(0f, 0f, false);
            animatorHandler.PlayTargetAnimation("RidingIdle", false); // Ngồi yên
        }

        isMounted = true;
        isTransitioning = false;
    }

    private IEnumerator Dismount()
    {
        isTransitioning = true;

        // Play animation Dismount
        if (animatorHandler != null)
            animatorHandler.anim.SetTrigger("triggerDismount");

        yield return new WaitForSeconds(dismountAnimTime);

        // Thực hiện xuống thú
        transform.SetParent(null);
        transform.position = mountObject.transform.position + dismountOffset;

        if (TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
        if (TryGetComponent(out Collider col)) col.enabled = true;

        if (mountController != null)
            mountController.enabled = false;

/*        if (inputHandler != null)
            inputHandler.isInputDisabled = false;*/

        if (animatorHandler != null)
            animatorHandler.PlayTargetAnimation("Empty", false);

        isMounted = false;
        isTransitioning = false;
    }
}
