using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelBoundaryTrigger : MonoBehaviour
{
    public LevelManager levelManager;


    private void OnTriggerExit(Collider other)
    {
        levelManager.LoadLevel(SceneManager.GetActiveScene().name);
    }
}
