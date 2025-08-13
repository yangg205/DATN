// ===================== BossBlackboard.cs =====================
using UnityEngine;

public class BossBlackboard : MonoBehaviour
{
    public Transform player;
    public float currentHP = 3000f;
    public float maxHP = 3000f;
    public bool isInSecondPhase;
    public bool isInThirdPhase;
    public bool canSeePlayer;
    public Vector3 lastKnownPlayerPosition;
    public float attackCooldown;
    public float timeSinceLastAttack;
    public Animator animator;

    public bool hasTarget = false;

}