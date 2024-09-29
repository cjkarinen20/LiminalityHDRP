using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class stalkerAI : MonoBehaviour
{
    public NavMeshAgent stalker;
    public Transform player;
    Vector3 destination;
    public Camera playerCam;
    public float aiSpeed;

    private void Update()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCam);

        if (GeometryUtility.TestPlanesAABB(planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            stalker.speed = 0;
            stalker.SetDestination(transform.position);
        }

        if (!GeometryUtility.TestPlanesAABB(planes, this.gameObject.GetComponent<Renderer>().bounds))
        {
            stalker.speed = aiSpeed;
            destination = player.position;
            stalker.destination = destination;
        }
    }
}
