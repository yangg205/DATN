using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PatrollState : StateMachineBehaviour
{
    /* float timer;
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
     //}*/

    Seeker seeker;
    Rigidbody rb;
    Transform player;
    List<Transform> waypoints = new();
    Path path;
    int currentWaypoint = 0;
    float speed = 2f;
    float waypointDistance = 2f;
    float timer;
    float chaseRange = 10f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        seeker = animator.GetComponent<Seeker>();
        rb = animator.GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        GameObject waypointObj = GameObject.FindWithTag("WayPoints");
        if (waypointObj != null)
        {
            foreach (Transform t in waypointObj.transform)
                waypoints.Add(t);
        }

        timer = 0;
        RequestNewPath(animator.transform.position, GetRandomWaypoint());
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;

        if (player && Vector3.Distance(animator.transform.position, player.position) < chaseRange)
            animator.SetBool("isChasing", true);

        if (path == null) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            RequestNewPath(animator.transform.position, GetRandomWaypoint());
            return;
        }

        Vector3 dir = (path.vectorPath[currentWaypoint] - animator.transform.position).normalized;
        rb.MovePosition(animator.transform.position + dir * speed * Time.deltaTime);

        if (Vector3.Distance(animator.transform.position, path.vectorPath[currentWaypoint]) < waypointDistance)
            currentWaypoint++;
    }

    void RequestNewPath(Vector3 start, Vector3 target)
    {
        currentWaypoint = 0;
        seeker.StartPath(start, target, OnPathComplete);
    }

    void OnPathComplete(Path p)
    {
        if (!p.error) path = p;
    }

    Vector3 GetRandomWaypoint()
    {
        return waypoints[Random.Range(0, waypoints.Count)].position;
    }
}