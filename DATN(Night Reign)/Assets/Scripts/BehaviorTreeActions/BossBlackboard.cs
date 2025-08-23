// ===================== BossBlackboard.cs =====================
using System.Collections;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.UI;

public class BossBlackboard : MonoBehaviour
{
    public Transform player;
    public float currentHP;
    public float maxHP = 1000f;
    public bool isInSecondPhase;
    public bool isInThirdPhase;
    public bool canSeePlayer;
    public Vector3 lastKnownPlayerPosition;
    public float attackCooldown;
    public float timeSinceLastAttack;
    public Animator animator;
    [Header("VFX")]
    public ParticleSystem vfxSlash;
    public ParticleSystem vfxSlash1;

    [Header("UI")]
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public TextMeshProUGUI hpText;
    private float lerpSpeed = 0.05f;

    public bool hasTarget = false;

    public int damageAmount;

    public static bool IsPaused = false;


    void Awake()
    {
        GameObject healthSliderObj = GameObject.Find("HealthBarFinal");
        if (healthSliderObj != null)
            healthSlider = healthSliderObj.GetComponent<Slider>();

        GameObject easeSliderObj = GameObject.Find("EaseHealthBarFinal");
        if (easeSliderObj != null)
            easeHealthSlider = easeSliderObj.GetComponent<Slider>();

        GameObject hpTextObj = GameObject.Find("HPTextFinal");
        if (hpTextObj != null)
            hpText = hpTextObj.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        StartCoroutine(SpawnAfterDelay(2f));
    
        currentHP = maxHP;
    }

    void Update()
    {
        if (IsPaused) return;


        if (healthSlider.value != currentHP)
        {
            healthSlider.value = currentHP;
        }

        if (healthSlider.value != easeHealthSlider.value)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHP, lerpSpeed);
        }

        hpText.text = $"{currentHP}";

        if (Input.GetKeyUp(KeyCode.T))
        {
            TakeDamage();
        }
    }

    public void TakeDamage()
    {
        currentHP -= damageAmount;
        animator.SetTrigger("takeHit");
    }

    IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger("Spawn");
    }

    public void PlaySlash()
    {
        vfxSlash.Play();
    }

    public void PlaySlash1()
    {
        vfxSlash1.Play();
    }
}