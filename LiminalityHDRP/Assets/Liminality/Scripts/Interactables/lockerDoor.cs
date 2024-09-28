using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class lockerDoor : Interactable
{
    private float dot;
    private bool isOpen = false;
    private bool canInteract = true;
    private Animator animator;

    public AudioSource audioSource;
    public AudioClip doorOpen;
    public AudioClip doorClose;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public override void OnFocus()
    {

    }

    public override void OnInteract()
    {
        Debug.Log("Door Triggered");
        Debug.Log(canInteract);
        if (canInteract)
        {
            isOpen = !isOpen;
            Vector3 doorTransformDirection = transform.TransformDirection(Vector3.forward);
            Vector3 playerTransformDirection = NewFPSController.instance.transform.position - transform.position;
            dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);

            if (isOpen)
            {
                audioSource.PlayOneShot(doorOpen);
            }
            else
            {
                audioSource.PlayOneShot(doorClose);
            }

            animator.SetFloat("Dot", dot);
            animator.SetBool("isOpen", isOpen);
        }
    }

    public override void OnLoseFocus()
    {

    }

}
