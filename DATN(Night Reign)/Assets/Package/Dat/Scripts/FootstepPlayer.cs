using System.Collections.Generic;
using UnityEngine;

public class FootstepPlayer : MonoBehaviour
{
    [Header("Raycast Foot Positions")]
    public Transform footL;
    public Transform footR;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip defaultFootstepClip;

    [System.Serializable]
    public class MaterialFootstepData
    {
        public string materialName;  // Tên của PhysicsMaterial (ví dụ "SandMaterial")
        public AudioClip clip;      // Clip tương ứng
    }

    [Header("Footstep Clips by Material Name")]
    public List<MaterialFootstepData> footstepClips;
    private Dictionary<string, AudioClip> materialToClipMap;

    [Header("Settings")]
    public float stepRate = 0.5f;
    public float raycastDistance = 1f;
    public LayerMask groundMask;

    private float stepTimerL;
    private float stepTimerR;
    private bool leftFootDown = false;
    private bool rightFootDown = false;

    private Animator animator;
    private Rigidbody rb;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        materialToClipMap = new Dictionary<string, AudioClip>();
        foreach (var entry in footstepClips)
        {
            if (!materialToClipMap.ContainsKey(entry.materialName) && entry.clip != null)
            {
                materialToClipMap.Add(entry.materialName, entry.clip);
            }
            else if (entry.clip == null)
            {
                Debug.LogWarning($"[FootstepPlayer] MaterialFootstepData for '{entry.materialName}' has a null AudioClip.", this);
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("FootstepPlayer requires an AudioSource component on the same GameObject or assigned in Inspector.", this);
            }
        }
    }

    void Update()
    {
        if (IsWalking())
        {
            HandleFoot(footL, ref leftFootDown, ref stepTimerL);
            HandleFoot(footR, ref rightFootDown, ref stepTimerR);
        }
        else
        {
            leftFootDown = false;
            rightFootDown = false;
        }
    }

    void HandleFoot(Transform foot, ref bool footDown, ref float stepTimer)
    {
        Debug.DrawRay(foot.position + Vector3.up * 0.1f, Vector3.down * raycastDistance, Color.red);

        if (Physics.Raycast(foot.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            if (!footDown && stepTimer <= 0f)
            {
                PlayFootstep(hit);
                footDown = true;
                stepTimer = stepRate;
            }
        }
        else
        {
            footDown = false;
        }

        stepTimer -= Time.deltaTime;
    }

    void PlayFootstep(RaycastHit hit)
    {
        if (audioSource == null) return;

        AudioClip clipToPlay = defaultFootstepClip;
        string materialOrTagName = "Default";

        // Thay đổi PhysicMaterial thành PhysicsMaterial
        PhysicsMaterial hitPhysicsMaterial = hit.collider.sharedMaterial;

        if (hitPhysicsMaterial != null)
        {
            // Debug.Log($"[Footstep] Hit PhysicsMaterial Name: {hitPhysicsMaterial.name}", hit.collider.gameObject);

            if (materialToClipMap.TryGetValue(hitPhysicsMaterial.name, out AudioClip matchedClip))
            {
                clipToPlay = matchedClip;
                materialOrTagName = hitPhysicsMaterial.name;
            }
            else
            {
                Debug.LogWarning($"[Footstep] PhysicsMaterial '{hitPhysicsMaterial.name}' found, but no corresponding AudioClip defined in Footstep Clips list. Using default.", this);
            }
        }
        else
        {
            Debug.LogWarning($"[Footstep] Raycast hit object '{hit.collider.name}' but collider has no PhysicsMaterial. Using default footstep clip.", hit.collider.gameObject);
        }

        if (clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
            Debug.Log($"[Footstep] Played '{clipToPlay.name}' for '{materialOrTagName}' at {hit.point} (Object: {hit.collider.name})", hit.collider.gameObject);
        }
        else
        {
            Debug.LogWarning("[Footstep] Final clipToPlay is null, cannot play sound. Default clip might be missing.", this);
        }
    }

    bool IsWalking()
    {
        if (animator != null)
        {
            float speed = animator.GetFloat("Speed");
            return speed > 0.1f;
        }

        if (rb != null)
        {
            return rb.linearVelocity.magnitude > 0.1f;
        }

        return Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
    }
}