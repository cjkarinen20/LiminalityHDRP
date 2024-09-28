using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class enemyAI : MonoBehaviour
{
    public NavMeshAgent ai;
    public Camera killCam;
    public Camera mainCam;
    public List<Transform> destinations;
    public Animator aiAnim;
    public Transform player;
    public NewFPSController playerController;
    [Header("AI Settings")]
    public float walkSpeed, chaseSpeed, minIdleTime, maxIdleTime, idleTime, detectionDistance, catchDistance, searchDistance, minChaseTime, maxChaseTime, minSearchTime, maxSearchTime, jumpscareTime;
    public bool walking, chasing, searching;

    Transform currentDest;
    Vector3 dest;
    public Vector3 rayCastOffset;
    public float aiDistance;
    private float footstepTimer;

    [Header("Footstep Sounds")]
    [SerializeField] private AudioClip[] waterSounds = default;
    [SerializeField] private float baseStepSpeed = 0.7f;
    [SerializeField] private float sprintStepSpeed = 0.3f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    //public GameObject hideText, stopHideText;

    private void Start()
    {
        walking = true;
        currentDest = destinations[Random.Range(0, destinations.Count)];
        killCam.enabled = false;
    }
    private void Update()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        RaycastHit hit;
        aiDistance = Vector3.Distance(player.position, this.transform.position);
        if (Physics.Raycast(transform.position + rayCastOffset, direction, out hit, detectionDistance))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                walking = false;
                StopCoroutine("stayIdle");
                StopCoroutine("searchRoutine");
                StartCoroutine("searchRoutine");
                searching = true;
            }
        }
        if (searching == true)
        {
            ai.speed = 0;
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.ResetTrigger("sprint");
            aiAnim.SetTrigger("search");
            if (aiDistance <= searchDistance)
            {
                StopCoroutine("stayIdle");
                StopCoroutine("searchRoutine");
                StopCoroutine("chaseRoutine");
                StartCoroutine("chaseRoutine");
                chasing = true;
                searching = false;
            }
        }
        if (chasing == true)
        {
            HandleFootsteps();
            dest = player.position;
            ai.destination = dest;
            ai.speed = chaseSpeed;
            aiAnim.ResetTrigger("walk");
            aiAnim.ResetTrigger("idle");
            aiAnim.ResetTrigger("search");
            aiAnim.SetTrigger("sprint");
            if (aiDistance <= catchDistance)
            {
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("idle");
                aiAnim.ResetTrigger("search");
                //hideText.SetActive(false);
                //stopHideText.SetActive(false);
                aiAnim.ResetTrigger("sprint");
                aiAnim.SetTrigger("jumpscare");
                ai.speed = 0;
                killCam.enabled = true;
                mainCam.enabled = false;
                StartCoroutine("deathRoutine");

                chasing = false;
            }
        }
        if (walking == true)
        {
            HandleFootsteps();
            dest = currentDest.position;
            ai.destination = dest;
            ai.speed = walkSpeed;
            aiAnim.ResetTrigger("sprint");
            aiAnim.ResetTrigger("idle");
            aiAnim.ResetTrigger("search");
            aiAnim.SetTrigger("walk");
            if (ai.remainingDistance <= ai.stoppingDistance)
            {
                aiAnim.ResetTrigger("sprint");
                aiAnim.ResetTrigger("walk");
                aiAnim.ResetTrigger("search");
                aiAnim.SetTrigger("idle");
                ai.speed = 0;
                StopCoroutine("stayIdle");
                StartCoroutine("stayIdle");
                walking = false;
            }
        }
    }
    public void stopChase()
    {
        walking = true;
        chasing = false;
        StopCoroutine("chaseRoutine");
        currentDest = destinations[Random.Range(0, destinations.Count)];
    }
    private void HandleFootsteps()
    {
        footstepTimer -= Time.deltaTime;
        if (walking == true)
        {
            if (footstepTimer <= 0)
            {
                footstepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                footstepAudioSource.PlayOneShot(waterSounds[UnityEngine.Random.Range(0, waterSounds.Length - 1)]);
                footstepTimer = baseStepSpeed;
            }
        }
        else if (chasing == true)
        {
            if (footstepTimer <= 0)
            {
                footstepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                footstepAudioSource.PlayOneShot(waterSounds[UnityEngine.Random.Range(0, waterSounds.Length - 1)]);
                footstepTimer = sprintStepSpeed;
            }
        }
    }
    IEnumerator stayIdle()
    {
        idleTime = Random.Range(minIdleTime, maxIdleTime);
        yield return new WaitForSeconds(idleTime);
        walking = true;
        currentDest = destinations[Random.Range(0, destinations.Count)];
    }
    IEnumerator searchRoutine()
    {
        yield return new WaitForSeconds(Random.Range(minSearchTime, maxSearchTime));
        searching = false;
        walking = true;
        currentDest = destinations[Random.Range(0, destinations.Count)];
    }
    IEnumerator chaseRoutine()
    {
        yield return new WaitForSeconds(Random.Range(minChaseTime, maxChaseTime));
        stopChase();
    }
    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);
        playerController.KillPlayer();
    }
}