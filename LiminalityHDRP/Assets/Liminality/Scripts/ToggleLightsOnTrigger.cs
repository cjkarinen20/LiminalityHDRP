using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleLightsOnTrigger : MonoBehaviour
{
    public AudioSource audioSource;

    public Light light1;
    public Light light2;
    public Light light3;

    // Start is called before the first frame update
    void Start()
    {
        light1 = GetComponent<Light>();
        light2 = GetComponent<Light>();
        light3 = GetComponent<Light>();

        light1.enabled = false;
        light2.enabled = false;
        light3.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Lighting Trigger Entered");
            light1.enabled = true;
            light2.enabled = true;
            light3.enabled = true;
        }
    }
}
