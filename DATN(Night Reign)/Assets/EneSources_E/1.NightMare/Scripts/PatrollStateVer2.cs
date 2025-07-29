using Pathfinding;
using System.Collections.Generic;
using UnityEngine;

public class PatrollStateVer2 : StateMachineBehaviour
{
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

        GameObject waypointObj = GameObject.FindWithTag("WayPointsVer2");
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
        NightMare nightMare = animator.GetComponent<NightMare>();
        DragonSoulEater soulEater = animator.GetComponent<DragonSoulEater>();

        if (nightMare != null && nightMare.isTakingDamage) return;

        if (soulEater != null && soulEater.isTakingDamage) return;

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
