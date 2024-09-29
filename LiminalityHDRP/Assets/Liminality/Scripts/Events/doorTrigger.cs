using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class doorTrigger : Interactable
{
    public LevelManager levelManager;
    public string levelName;
    public override void OnFocus()
    {

    }
    public override void OnLoseFocus()
    {

    }
    public override void OnInteract()
    {
        levelManager.LoadLevel(levelName);
    }

}
