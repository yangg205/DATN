using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem.Controls;

public class AnimatorHook : MonoBehaviour
{
    Animator anim;
    PlayerState states;
    public void Init(PlayerState st)
    {
        states = st;
        anim = st.anim;
    }
    void OnAnimatorMove()
    {
        if (states.canMove)
            return;
        states.rigid.linearDamping = 0;
        float multiplier = 1;
        Vector3 delta = anim.deltaPosition;
        delta.y = 0;
        Vector3 v = (delta * multiplier) / states.delta;
        states.rigid.linearVelocity = v;
    }
    //public void LateTick()
    //{
    //    if()
    //}
}
