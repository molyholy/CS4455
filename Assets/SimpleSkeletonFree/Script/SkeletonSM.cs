﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SkeletonSM : MonoBehaviour
{
    public enum AIState
    {
        idle_state,
        patrol_state,
        chase_state,
        attack_state,
        ready_state,
    };

    public AIState aiState;
    private NavMeshAgent skeletonNav;
    private Animator skeletonAnim;

    public float wanderRadius;
    public float wanderTimer;

    private Transform target;
    private float timer;

    private WaitForSeconds cooldown;
    private WaitForSeconds idleTime;
    private WaitForSeconds readyTime;

    private GameObject player;

    bool inPatrol;
    bool inRange;
    bool ifSwapIdle;
    bool ifSwapPatrol;
    bool inMeleeDist;
    bool attackBuffer;
    bool ifSwapReady;
    bool ifSwapAttack;

    // Start is called before the first frame update
    void Start()
    {
        skeletonNav = GetComponent<NavMeshAgent>();
        skeletonAnim = GetComponent<Animator>();
        aiState = AIState.idle_state;
        timer = 0;
        inRange = false;
        ifSwapIdle = true;
        ifSwapPatrol = true;
        cooldown = new WaitForSeconds(5f);
        idleTime = new WaitForSeconds(1f);
        player = GameObject.FindWithTag("Player");
        inMeleeDist = false;
        attackBuffer = false;
        readyTime = new WaitForSeconds(3f);
        ifSwapReady = true;
        ifSwapAttack = true;
    }

    // Update is called once per frame
    void Update()
    {
        float distToPlayer = Vector3.Distance(transform.position, player.transform.position);
        Debug.Log(distToPlayer);
        switch(aiState)
        {
            case AIState.idle_state:
                if (distToPlayer < 20.0f)
                {
                    skeletonAnim.SetBool("inPatrol", true);
                    aiState = AIState.chase_state;
                    break;
                }
                if (ifSwapIdle)
                {
                    StartCoroutine(SwapToPatrol());
                    ifSwapIdle = false;
                    ifSwapPatrol = true;
                }
                break;
            case AIState.patrol_state:
                //timer += Time.deltaTime;
                //timer >= wanderTimer &&
                //Debug.Log("patrol");
                if (distToPlayer < 20.0f)
                {
                    skeletonAnim.SetBool("inPatrol", true);
                    aiState = AIState.chase_state;
                    break;
                }

                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, NavMesh.AllAreas);
                skeletonNav.SetDestination(newPos);
                //timer = 0;

                if (!inRange && !skeletonNav.pathPending && ifSwapPatrol)
                {
                    StartCoroutine(SwapToIdle());
                    ifSwapIdle = true;
                    ifSwapPatrol = false;
                    //Debug.Log("swap");
                }
                break;
            case AIState.chase_state:
                Vector3 dirToPlayer = transform.position - player.transform.position;
                Vector3 chasePos = transform.position - dirToPlayer;
                skeletonNav.SetDestination(chasePos);
                if (distToPlayer < 3.0f)
                {
                    aiState = AIState.ready_state;
                    skeletonNav.stoppingDistance = 3.0f;
                }
                break;
            case AIState.ready_state:
                skeletonAnim.SetBool("inPatrol", false);
                if (distToPlayer < 3.0f)
                {
                    skeletonAnim.SetBool("inMeleeDist", true);
                    if (ifSwapAttack)
                    {
                        StartCoroutine(attackDelay());
                        ifSwapReady = true;
                        ifSwapAttack = false;
                        break;
                    }
                }
                if (3.0f < distToPlayer && distToPlayer < 20.0f)
                {
                    aiState = AIState.chase_state;
                    skeletonAnim.SetBool("inPatrol", true);
                    skeletonAnim.SetBool("inMeleeDist", false);
                    break;
                }
                if (distToPlayer > 20.0f)
                {
                    inRange = false;
                    aiState = AIState.patrol_state;
                    skeletonAnim.SetBool("inPatrol", true);
                    skeletonAnim.SetBool("inMeleeDist", false);
                    break;
                }
                break;
            case AIState.attack_state:
                if (ifSwapReady)
                {
                    StartCoroutine(SwapToReady());
                    ifSwapAttack = true;
                    ifSwapReady = false;
                }
                break;
        }


    }

    public IEnumerator SwapToPatrol()
    {
        yield return idleTime;
        aiState = AIState.patrol_state;
        skeletonAnim.SetBool("inPatrol", true);
    }

    public IEnumerator SwapToIdle()
    {
        yield return cooldown;
        aiState = AIState.idle_state;
        skeletonAnim.SetBool("inPatrol", false);
    }

    public IEnumerator attackDelay()
    {
        yield return readyTime;
        skeletonAnim.SetBool("attackBuffer", true);
        aiState = AIState.attack_state;
    }

    public IEnumerator SwapToReady()
    {
        yield return readyTime;
        skeletonAnim.SetBool("attackBuffer", false);
        aiState = AIState.ready_state;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {

        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}