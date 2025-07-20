using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ND;

public class Player : MonoBehaviour
{
    public GameObject Cameraman, RippleCamera;
    public ParticleSystem ripple;

    private Vector3 previousPosition;
    private float velocityXZ, velocityY;
    private bool inWater;
    private RaycastHit isGround;

    private CharacterController cc;
    private PlayerLocomotion locomotion;
    private InputHandler input;

    void Start()
    {
        Application.targetFrameRate = 60;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cc = GetComponent<CharacterController>();
        locomotion = GetComponent<PlayerLocomotion>();
        input = GetComponent<InputHandler>();

        previousPosition = transform.position;
    }

    void Update()
    {
        UpdateVelocity();
        UpdateRipplePosition();
        CheckWaterState();
        Shader.SetGlobalVector("_Player", transform.position);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }

    void UpdateVelocity()
    {
        velocityXZ = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(previousPosition.x, 0, previousPosition.z)
        );
        velocityY = Mathf.Abs(transform.position.y - previousPosition.y);
        previousPosition = transform.position;
    }

    void UpdateRipplePosition()
    {
        if (isGround.collider)
            ripple.transform.position = transform.position + transform.forward;
        else
            ripple.transform.position = transform.position;
    }

    void CheckWaterState()
    {
        float checkHeight = cc != null ? (cc.height + cc.radius) : 2.5f;
        Vector3 start = transform.position + Vector3.up * checkHeight;
        inWater = Physics.Raycast(start, Vector3.down, checkHeight * 2, LayerMask.GetMask("Water"));

        Physics.Raycast(transform.position, Vector3.down, out isGround, 2.7f, LayerMask.GetMask("Ground"));

        ripple.gameObject.SetActive(inWater);
        Debug.DrawRay(start, Vector3.down * checkHeight, Color.blue);
        Debug.DrawRay(transform.position, Vector3.down * 2.7f, Color.red);
    }

    void CreateRipple(int Start, int End, int Delta, float Speed, float Size, float Lifetime)
    {
        Vector3 forward = ripple.transform.eulerAngles;
        forward.y = Start;
        ripple.transform.eulerAngles = forward;

        for (int i = Start; i < End; i += Delta)
        {
            var emitParams = new ParticleSystem.EmitParams
            {
                position = transform.position + ripple.transform.forward * 1.15f,
                velocity = ripple.transform.forward * Speed,
                startSize = Size,
                startLifetime = Lifetime,
                startColor = Color.white
            };

            ripple.Emit(emitParams, 1);
            ripple.transform.Rotate(Vector3.up * Delta, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            var emitParams = new ParticleSystem.EmitParams
            {
                position = transform.position,
                velocity = Vector3.zero,
                startSize = 5f,
                startLifetime = 0.1f,
                startColor = Color.white
            };
            ripple.Emit(emitParams, 1);
        }
        Debug.Log("Player Entered: " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            var emitParams = new ParticleSystem.EmitParams
            {
                position = transform.position,
                velocity = Vector3.zero,
                startSize = 5f,
                startLifetime = 0.1f,
                startColor = Color.white
            };
            ripple.Emit(emitParams, 1);
        }
    }
}
