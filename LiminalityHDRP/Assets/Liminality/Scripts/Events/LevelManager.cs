using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Image loadingIcon;

    public static LevelManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void LoadLevel(string levelName)
    {
        var level = SceneManager.LoadSceneAsync(levelName);
        level.allowSceneActivation = false;

        loadingScreen.SetActive(true);

        do
        {
            await Task.Delay(100);
            loadingIcon.fillAmount = level.progress;
        }
        while (level.progress < 0.9f);

        await Task.Delay(1000);

        level.allowSceneActivation = true;
        loadingScreen.SetActive(false);

    }
}
