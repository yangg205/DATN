// ===================== BossBlackboard.cs =====================
using System.Collections;
using TMPro;
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

    [Header("UI")]
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public TextMeshProUGUI hpText;
    public Canvas canvas;
    private float lerpSpeed = 0.05f;

    public bool hasTarget = false;

    public bool isDead;

    public int damageAmount;

    public static bool IsPaused = false;

    private BossMovementAStar movement;

    void Awake()
    {
        //GameObject healthSliderObj = GameObject.Find("HealthBarFinal");
        //if (healthSliderObj != null)
        //    healthSlider = healthSliderObj.GetComponent<Slider>();

        //GameObject easeSliderObj = GameObject.Find("EaseHealthBarFinal");
        //if (easeSliderObj != null)
        //    easeHealthSlider = easeSliderObj.GetComponent<Slider>();

        //GameObject hpTextObj = GameObject.Find("HPTextFinal");
        //if (hpTextObj != null)
        //    hpText = hpTextObj.GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        movement = GetComponent<BossMovementAStar>();

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

        #if UNITY_EDITOR
        if (Input.GetKeyUp(KeyCode.T))
        {
            TakeDamage(damageAmount); // test damage
        }
        #endif
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHP -= damageAmount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        animator.SetTrigger("takeHit");

        if(currentHP <= 0)
        {
            die();
        }

    }

    IEnumerator SpawnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetTrigger("Spawn");
    }

   public void die()
   {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("death");
        movement?.StopMoving();
        StartCoroutine(DestroyAfterDelay(10f));
   }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);


        if (canvas != null)
            Destroy(canvas.gameObject);

        Destroy(gameObject);
    }
}