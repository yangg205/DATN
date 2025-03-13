using UnityEngine;

public class Helper : MonoBehaviour
{
    [Range(0, 1)]
    Animator anim;
    public float vertical;
    public string[] oneHand;
    public string[] twoHands;
    public bool playAnim;
    public bool twoHanded;
    public bool enableRM;
    void Start()
    {
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        enableRM = !anim.GetBool("canMove");
        anim.applyRootMotion = enableRM;
        if(enableRM)
            return;
        anim.SetBool("two_handed", twoHanded);
        if(playAnim)
        {
            string targetAnim;
            if(!twoHanded)
            {
                int r = Random.Range(0, oneHand.Length);
                targetAnim = oneHand[r];
            }
            else
            {
                int r = Random.Range(0, twoHands.Length);
                targetAnim = twoHands[r];

            }
            vertical = 0;
/*            anim.SetBool("canMove", false);
            enabled = true;*/
            anim.CrossFade(targetAnim, 0.2f);
            playAnim = false;
        }
        anim.SetFloat("vertical", vertical);
    }
    public void SendEvent()
    {
        Debug.Log("Animation Event SendEvent được gọi!");
    }
}
