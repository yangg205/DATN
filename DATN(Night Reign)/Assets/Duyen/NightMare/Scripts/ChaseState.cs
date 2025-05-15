using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChaseState : StateMachineBehaviour
{
    /*NavMeshAgent agent;
    Transform player;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        agent = animator.GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (agent == null)
            return;
        

        if (player == null)
            return;
        

        agent.speed = 3.5f;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) 
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isPatrolling", true);
            return;
        }

        agent.SetDestination(player.position);

        float distance = Vector3.Distance(player.position, animator.transform.position);
        if (distance > 15)
            animator.SetBool("isChasing", false);
        if (distance < 2.5f)
            animator.SetBool("isAttacking", true);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateinfo, int layerindex)
    {
        if (agent != null)
            agent.SetDestination(animator.transform.position);

    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}*/

    Seeker seeker;
    Rigidbody rb;
    Transform player;
    Path path;
    int currentWaypoint = 0;
    float speed = 4f;
    float chaseRange = 15f;
    float attackRange = 2f;
    float nextWaypointDistance = 0.5f;
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
        if (player == null)
        {
            animator.SetBool("isChasing", false);
            return;
        }

        float distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance > chaseRange)
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isPatrolling", true);
            return;
        }
        if (distance < attackRange)
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
