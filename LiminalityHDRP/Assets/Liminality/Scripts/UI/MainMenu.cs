using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
   public void NewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void LoadGame()
    {
        //add load game functionality
    }
    public void OnApplicationQuit()
    {
        Application.Quit();
    }
    public void SettingsMenu()
    {
        //add settings menu
        //determine whether to make a new scene or just a new canvas
    }
}
