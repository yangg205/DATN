using UnityEngine;
using Fusion;
using UnityEngine.UI;
using Unity.Cinemachine;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    public string Name; //tên
    public TextMeshProUGUI nameText;
    public CinemachineCamera FollowCamera;
    public CharacterController characterController;

    [Networked, OnChangedRender(nameof(OnChangedSpeed))]
    public float speed { get; set; } = 12;
    public Animator animator;

    public void OnChangedSpeed()
    {
        animator.SetFloat("Speed", speed);
    }

    //HP
    [Networked, OnChangedRender(nameof(OnChangedHealth))]
    public int Health { get; set; }
    public Slider healthSlider;

    public void OnChangedHealth()
    {
        healthSlider.value = Health;
    }

    private void Start()
    {
        healthSlider.value = Health;
    }

    public override void Spawned()
    {
        base.Spawned();
        FollowCamera = FindFirstObjectByType<CinemachineCamera>();
        if (Object.HasInputAuthority && FollowCamera != null)
        {
            FollowCamera.Follow = transform;
            FollowCamera.LookAt = transform;
        }    

        if(Object.HasInputAuthority)
        {
            name = PlayerPrefs.GetString("PlayerName");
            nameText.text = Name;

            characterController = GetComponent<CharacterController>();
        }    

        if(Object.HasInputAuthority)
        {
            RpcUpdateHealth(30);
        }    
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcUpdateHealth(int health)
    {
        Health = health;
        healthSlider.value = Health;
    }

    public override void FixedUpdateNetwork()
    {
        if(FollowCamera != null)
        {
            nameText.transform.LookAt(FollowCamera.transform);
            healthSlider.transform.LookAt(FollowCamera.transform);
        }    

        if(Object.HasInputAuthority)
        {
            var move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            characterController.Move(move * Time.deltaTime * 5);
            speed = move.magnitude;
        }    
    }

    private void OnTriggerEnter(Collider other)
    {
        if(Object.HasStateAuthority)
        {
            if(other.CompareTag("Enemy"))
            {
                RpcUpdateHealth(Health - 10);
            }    
        }    
    }
}
