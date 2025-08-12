using ND;
using UnityEngine;

public class PlayerBowHandler : MonoBehaviour
{
    PlayerLocomotion playerLocomotion;
    InputHandler inputHandler;
    AnimatorHandler animatorHandler;
    WeaponSlotManager weaponSlotManager;

    [Header("Arrow Settings")]
    public GameObject arrowPrefab;
    private Transform arrowSpawnPoint;
    public float arrowSpeed = 40f;

    void Awake()
    {
        playerLocomotion = GetComponent<PlayerLocomotion>();
        inputHandler = GetComponent<InputHandler>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
        weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
    }

    void Update()
    {
        HandleAiming();
        HandleShooting();
    }

    void HandleAiming()
    {
        if (inputHandler.aiming_input)
        {
            // Khi mới aim, tìm ArrowSpawn nếu chưa tìm được
            if (arrowSpawnPoint == null && weaponSlotManager.rightHandSlot.currentWeaponModel != null)
            {
                arrowSpawnPoint = weaponSlotManager.rightHandSlot.currentWeaponModel.transform.Find("ArrowSpawn");
                if (arrowSpawnPoint == null)
                {
                    Debug.LogWarning("ArrowSpawn point not found in bow model!");
                }
            }

            playerLocomotion.isAiming = true;
            playerLocomotion.rigidbody.linearVelocity = Vector3.zero;
            animatorHandler.PlayTargetAnimation("Aim_Bow", false);
        }
        else
        {
            if (playerLocomotion.isAiming)
            {
                playerLocomotion.isAiming = false;
                animatorHandler.PlayTargetAnimation("Empty", false);
            }
        }
    }

    void HandleShooting()
    {
        if (playerLocomotion.isAiming && inputHandler.shootArrow_input)
        {
            inputHandler.shootArrow_input = false;
            animatorHandler.PlayTargetAnimation("Shoot_Bow", true);
            // SpawnArrow sẽ được gọi qua Animation Event
        }
    }

    // Hàm này sẽ được gọi trong Animation Event của "Shoot_Bow"
    public void SpawnArrow()
    {
        if (arrowPrefab != null && arrowSpawnPoint != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
            Rigidbody rb = arrow.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = arrowSpawnPoint.forward * arrowSpeed;
            }
        }
        else
        {
            Debug.LogWarning("ArrowPrefab hoặc ArrowSpawnPoint chưa được gán!");
        }
    }
}
