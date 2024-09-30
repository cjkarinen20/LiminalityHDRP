using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class mannequinAI : MonoBehaviour
{
    public Canvas deathCanvas;
    public Animator aiAnimator;
    public NewFPSController playerController;
    public NavMeshAgent mannequin;
    public Transform player;
    Vector3 destination;
    public Camera playerCam;
    public Camera killCam;
    public float aiSpeed;
    public float catchDistance;
    public float jumpscareTime;
    private float footstepTimer;

    [Header("Footstep Sounds")]
    [SerializeField] private float baseStepSpeed = 0.7f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] grassSounds = default;
    [SerializeField] private AudioClip[] dirtSounds = default;
    [SerializeField] private AudioClip[] tileSounds = default;
    [SerializeField] private AudioClip[] waterSounds = default;

    private void Start()
    {
        killCam.enabled = false;
    }
    private void Update()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCam);

        float distance = Vector3.Distance(transform.position, player.position);

        if (GeometryUtility.TestPlanesAABB(planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            mannequin.speed = 0;
            aiAnimator.speed = 0;

            mannequin.SetDestination(transform.position);
        }

        if (!GeometryUtility.TestPlanesAABB(planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            HandleFootsteps();
            mannequin.speed = aiSpeed;
            aiAnimator.speed = 1;
            destination = player.position;
            mannequin.destination = destination;

            if (distance <= catchDistance)
            {
                aiAnimator.ResetTrigger("walking");
                aiAnimator.SetTrigger("idle");
                playerCam.enabled = false;
                killCam.enabled = true;
                aiAnimator.speed = 0;
                deathCanvas.enabled = true;
                StartCoroutine("deathRoutine");
            }

        }
    }
    IEnumerator deathRoutine()
    {
        yield return new WaitForSeconds(jumpscareTime);
        playerController.KillPlayer();
    }
    private void HandleFootsteps()
    {

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            footstepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            if (Physics.Raycast(this.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {

                    case "Grass":
                        {
                            footstepAudioSource.PlayOneShot(grassSounds[UnityEngine.Random.Range(0, grassSounds.Length - 1)]);
                        }
                        break;
                    case "Dirt":
                        {
                            footstepAudioSource.PlayOneShot(dirtSounds[UnityEngine.Random.Range(0, dirtSounds.Length - 1)]);
                        }
                        break;
                    case "Tile":
                        {
                            footstepAudioSource.PlayOneShot(tileSounds[UnityEngine.Random.Range(0, tileSounds.Length - 1)]);
                        }
                        break;
                    case "Water":
                        {
                            footstepAudioSource.PlayOneShot(waterSounds[UnityEngine.Random.Range(0, waterSounds.Length - 1)]);
                        }
                        break;
                    default:
                        footstepTimer = baseStepSpeed;
                        break;
                }
            }
            footstepTimer = baseStepSpeed;
        }
    }
}
