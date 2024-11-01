using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interactable
{
    private float dot;
    private bool isOpen = false;
    private bool canInteract = true;

    [Header("Interaction Parameters")]
    [SerializeField] private bool canPlayerOpen = true;
    [SerializeField] private bool autoClose = false;


    [Header("Animator")]
    public Animator animator;

    [Header("Audio Parameters")]
    public AudioSource audioSource;
    public AudioClip doorOpen;
    public AudioClip doorClose;

    private void Start()
    {
        animator = transform.parent.GetComponent<Animator>();
        audioSource = transform.parent.GetComponent<AudioSource>();
    }
    public override void OnFocus()
    {
        Debug.Log(dot);
    }

    public override void OnInteract()
    {
        Debug.Log("Door Triggered");
        Debug.Log(canInteract);
        if (canInteract)
        {
            isOpen = !isOpen;
            Vector3 doorTransformDirection = this.transform.TransformDirection(Vector3.forward);
            Vector3 playerTransformDirection = NewFPSController.instance.transform.position - transform.position;
            dot = Vector3.Dot(doorTransformDirection, playerTransformDirection);
            Debug.Log(dot);

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


            StartCoroutine(AutoClose());
        }
    }

    public override void OnLoseFocus()
    {
        
    }

    private IEnumerator AutoClose()
    {
        if (!autoClose) yield break; 

        while (isOpen)
        {
            yield return new WaitForSeconds(3);

            if(Vector3.Distance(transform.position, NewFPSController.instance.transform.position) > 3)
            {
                audioSource.PlayOneShot(doorClose);
                isOpen = false;
                animator.SetFloat("Dot", 0);
                animator.SetBool("isOpen", isOpen);
            }
        }
    }
}
