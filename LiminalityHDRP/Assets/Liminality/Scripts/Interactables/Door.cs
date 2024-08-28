using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    private float dot;
    private bool isOpen = false;
    private bool canInteract = true;
    public Animator animator;

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

            animator.SetFloat("Dot", dot);
            animator.SetBool("isOpen", isOpen);

            StartCoroutine(AutoClose());
        }
    }

    public override void OnLoseFocus()
    {
        
    }

    private IEnumerator AutoClose()
    {
        while (isOpen)
        {
            yield return new WaitForSeconds(3);

            if(Vector3.Distance(transform.position, NewFPSController.instance.transform.position) > 3)
            {
                isOpen = false;
                animator.SetFloat("Dot", 0);
                animator.SetBool("isOpen", isOpen);
            }
        }
    }
}
