using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseState : StateMachineBehaviour
{
    Seeker seeker;
    Rigidbody rb;
    Transform player;
    Path path;
    int currentWaypoint = 0;
    float speed = 3f;
    float chaseRange = 13f;
    float attackRange = 3.3f;
    float nextWaypointDistance = 1f;
    float pathUpdateInterval = 0.5f;
    float pathTimer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        seeker = animator.GetComponent<Seeker>();
        rb = animator.GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        pathTimer = 0;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        NightMare nightMare = animator.GetComponent<NightMare>();
        DragonSoulEater soulEater = animator.GetComponent<DragonSoulEater>();

        if (nightMare != null && nightMare.isTakingDamage) return;

        if (soulEater != null && soulEater.isTakingDamage) return;

        if (player == null)
        {
            animator.SetBool("isChasing", false);
            return;
        }

        Vector3 dirToPlayer = (player.position - animator.transform.position).normalized;
        float angleToPlayer = Vector3.Angle(animator.transform.forward, dirToPlayer);
        float distanceToPlayer = Vector3.Distance(animator.transform.position, player.position);

        if (distanceToPlayer > chaseRange || angleToPlayer > 80f)
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isPatrolling", true);
            return;
        }
        if (angleToPlayer <= 80f && distanceToPlayer < attackRange)
        {
            animator.SetBool("isAttacking", true);
            return;
        }


        pathTimer += Time.deltaTime;
        if (pathTimer >= pathUpdateInterval)
        {
            seeker.StartPath(animator.transform.position, player.position, OnPathComplete);
            pathTimer = 0;
        }

        if (path == null) return;
        if (currentWaypoint >= path.vectorPath.Count) return;


        Vector3 dir = (path.vectorPath[currentWaypoint] - animator.transform.position).normalized;
        rb.MovePosition(animator.transform.position + dir * speed * Time.deltaTime);

        if (Vector3.Distance(animator.transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance)
            currentWaypoint++;
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }
}
