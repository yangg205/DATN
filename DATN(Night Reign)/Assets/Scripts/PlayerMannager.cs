using UnityEngine;

public class PlayerMannager : MonoBehaviour
{
    InputHandler inputHandler;
    Animator anim;

    void Start()
    {
        inputHandler = GetComponent<InputHandler>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        inputHandler.isInteracting = anim.GetBool("isInteracting");
        inputHandler.rollFlag = false;
        inputHandler.sprintFlag = false;
    }
}
