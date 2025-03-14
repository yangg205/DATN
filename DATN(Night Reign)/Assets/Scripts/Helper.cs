using UnityEngine;

public class Helper : MonoBehaviour
{
    [Range(-1, 1)]
    public float vertical;
    [Range(-1, 1)]
    public float horizontal;
    public string[] oneHand;
    public string[] twoHands;

    public bool playAnim;
    public bool twoHanded;
    public bool enableRM;
    public bool useItem;
    public bool interacting;
    public bool lockOn;
    Animator anim;
    void Start()
    {
        anim = GetComponent<Animator>();
    }
    void Update()
    {
     
        enableRM = !anim.GetBool("canMove");
        anim.applyRootMotion = enableRM;
        interacting = anim.GetBool("interacting");
        if(lockOn == false)
        {
            horizontal = 0;
            vertical = Mathf.Clamp01(vertical);
        }
        anim.SetBool("lockon", lockOn);
        if(enableRM)
            return;
        if (useItem)
        {
            playAnim = false;
            twoHanded = false;
            anim.Play("Drinking");
            useItem = false;
        }
        if(interacting)
        {
            playAnim = false;
            vertical = Mathf.Clamp(vertical, 0, 0.5f);
        }
            anim.SetBool("two_handed", twoHanded);
        if(playAnim)
        {
            string targetAnim;
            if(!twoHanded)
            {
                int r = Random.Range(0, oneHand.Length);
                targetAnim = oneHand[r];
                if (vertical > 0.5f)
                    targetAnim = "OneHand_Up_Attack_B_3";
            }
            else
            {
                int r = Random.Range(0, twoHands.Length);
                targetAnim = twoHands[r];
            }
            if (vertical > 0.5f)
                targetAnim = "OneHand_Up_Attack_B_3";
            vertical = 0;
/*            anim.SetBool("canMove", false);
            enabled = true;*/
            anim.CrossFade(targetAnim, 0.2f);
            playAnim = false;
        }
        anim.SetFloat("vertical", vertical);
        anim.SetFloat("horizontal", horizontal);
    }
    public void SendEvent()
    {
        Debug.Log("Animation Event SendEvent được gọi!");
    }
}
