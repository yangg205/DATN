using ND;
using UnityEngine;

public class BillboardNameplate : MonoBehaviour
{
    [SerializeField] private GameObject objectToRotate;

    private CameraHandler playerCameraHandler;

    void Awake()
    {
        if (objectToRotate == null)
        {
            objectToRotate = this.gameObject;
        }
    }

    void Start()
    {
        if (objectToRotate == null)
        {
            Debug.LogError("BillboardNameplate: 'Object To Rotate' field is not assigned on GameObject: " + gameObject.name + ". Script will be disabled.", this);
            enabled = false;
            return;
        }

        playerCameraHandler = FindObjectOfType<CameraHandler>();
        if (playerCameraHandler == null)
        {
            Debug.LogWarning("BillboardNameplate: CameraHandler not found in scene on GameObject: " + gameObject.name + ". Billboard rotation will not function.", this);
        }
    }

    void LateUpdate()
    {
        if (objectToRotate != null && playerCameraHandler != null && playerCameraHandler.cameraTransform != null)
        {
            Vector3 camPosition = playerCameraHandler.cameraTransform.position;
            Vector3 lookAtTarget = new Vector3(camPosition.x, objectToRotate.transform.position.y, camPosition.z);
            objectToRotate.transform.LookAt(lookAtTarget);
            objectToRotate.transform.Rotate(0, 180f, 0); // nếu bị ngược
        }
    }

}