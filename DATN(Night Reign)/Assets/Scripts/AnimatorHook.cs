using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem.Controls;

public class AnimatorHook : MonoBehaviour
{
    Animator anim;
    PlayerState states;
    public float rm_multi;
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
        if (rm_multi == 0)
            rm_multi = 1;
        Vector3 delta = anim.deltaPosition;
        delta.y = 0;
        Vector3 v = (delta * rm_multi) / states.delta;
        states.rigid.linearVelocity = v;
    }
}
