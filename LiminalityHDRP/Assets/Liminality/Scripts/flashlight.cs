using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashlight : MonoBehaviour
{
    public AudioSource AudioSource;

    public AudioClip flashlightClick;

    private Vector3 vectorOffset;
    private GameObject followTarget;
    public Light light;
    public KeyCode lightToggle = KeyCode.F;
    public Camera cam;
    [SerializeField] private float speed = 3.0f;
    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
        light.enabled = true;
        followTarget = Camera.main.gameObject;
        vectorOffset = transform.position - followTarget.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = followTarget.transform.position + vectorOffset;
        transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.transform.rotation, speed * Time.deltaTime);
        if (Input.GetKeyDown(lightToggle))
        {
            if (light.enabled)
            {
                AudioSource.PlayOneShot(flashlightClick);
                light.enabled = false;
            }
            else if (!light.enabled)
            {
                AudioSource.PlayOneShot(flashlightClick);
                light.enabled = true;
            }

        }
    }
}
