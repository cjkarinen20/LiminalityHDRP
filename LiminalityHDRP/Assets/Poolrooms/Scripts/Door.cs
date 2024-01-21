using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{

    private bool isOpen = false;
    private bool canInteract = true;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public override void OnFocus()
    {
        
    }

    public override void OnInteract()
    {
        if (canInteract)
        {
            isOpen = !isOpen;
            Vector3 doorTransformDirection = transform.TransformDirection(Vector3.forward);
            Vector3 playerTransformDirection = NewFPSController.instance.transform.position - transform.position;
            float dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);

            animator.SetFloat("dot", dot);
            animator.SetBool("isOpen", isOpen);
        }
    }

    public override void OnLoseFocus()
    {
        
    }
}
