using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthTest : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            NewFPSController.OnTakeDamage(1);
    }
}
