using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class mannequinAI : MonoBehaviour
{
    public Animator aiAnimator;
    public NewFPSController playerController;
    public NavMeshAgent mannequin;
    public Transform player;
    Vector3 destination;
    public Camera playerCam;
    public float aiSpeed;
    public float catchDistance;
    public float jumpscareTime;


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
            mannequin.speed = aiSpeed;
            aiAnimator.speed = 1;
            destination = player.position;
            mannequin.destination = destination;

            if (distance <= catchDistance)
            {
                player.gameObject.SetActive(false);
                playerController.KillPlayer();
            }

        }
    }





}
