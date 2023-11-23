using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FootstepSystem : MonoBehaviour
{
    public AudioSource AudioSource;

    public AudioClip concrete;
    public AudioClip grass;
    public AudioClip water;

    RaycastHit hit;
    public Transform RayStart;
    public float range;
    public LayerMask layerMask;

    public void Footstep()
    {
        if (Physics.Raycast(RayStart.position, RayStart.transform.up * -1, out hit, range, layerMask))
        {
            Debug.Log("FootstepRayHit");
            if (hit.collider.CompareTag("Ground"))
            {
                PlayFootstepSoundL(concrete);
            }
            if (hit.collider.CompareTag("Water"))
            {
                PlayFootstepSoundL(water);

            }
        }

    }

    private void PlayFootstepSoundL(AudioClip source)
    {
        AudioSource.pitch = Random.Range(0.8f, 1f);
        AudioSource.PlayOneShot(source);
    }
    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(RayStart.position, RayStart.transform.up * range * -1, Color.green);
    }
}
