using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;

public class PatrollState : StateMachineBehaviour
{
    float timer;
    List<Transform> wayPoints = new List<Transform>();
    NavMeshAgent agent;

    Transform player;
    float chaseRange = 8f;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = animator.GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogWarning("NavMeshAgent is missing on enemy!");
            return;
        }

        timer = 0;
        GameObject waypointsObj = GameObject.FindGameObjectWithTag("WayPoints");

        if (waypointsObj != null)
        {
            foreach (Transform t in waypointsObj.transform)
                wayPoints.Add(t);
        }

        if (wayPoints.Count == 0)
        {
            Debug.LogWarning("No waypoints found!");
            return;
        }

        agent.speed = 1.5f;
        agent.SetDestination(wayPoints[Random.Range(0, wayPoints.Count)].position);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent == null || wayPoints.Count == 0) return;

        if (player == null)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
                agent.SetDestination(wayPoints[Random.Range(0, wayPoints.Count)].position);
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            agent.SetDestination(wayPoints[Random.Range(0, wayPoints.Count)].position);

        timer += Time.deltaTime;
        if (timer > 10)
            animator.SetBool("isPatrolling", false);


        float distance = Vector3.Distance(player.position, animator.transform.position);
        if (distance < chaseRange)
            animator.SetBool("isChasing", true);

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (agent != null)
            agent.SetDestination(agent.transform.position);
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
    //}
}