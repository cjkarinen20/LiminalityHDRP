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
    private float loadTarget;

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
        loadTarget = 0;
        loadingIcon.fillAmount = 0;

        var level = SceneManager.LoadSceneAsync(levelName);
        level.allowSceneActivation = false;

        loadingScreen.SetActive(true);

        do
        {
            await Task.Delay(100);
            loadTarget = level.progress;
        }
        while (level.progress < 0.9f);

        await Task.Delay(1000);

        level.allowSceneActivation = true;
        if (level.isDone)
        {
            loadingScreen.SetActive(false);
        }

    }
    public async void LoadNextLevel()
    {
        loadTarget = 0;
        loadingIcon.fillAmount = 0;

        var level = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        level.allowSceneActivation = false;

        loadingScreen.SetActive(true);

        do
        {
            await Task.Delay(100);
            loadTarget = level.progress;
        }
        while (level.progress < 0.9f);

        await Task.Delay(1000);

        level.allowSceneActivation = true;
        if (level.isDone)
        {
            loadingScreen.SetActive(false);
        }


    }
    private void Update()
    {
        loadingIcon.fillAmount = Mathf.MoveTowards(loadingIcon.fillAmount, loadTarget, Time.deltaTime);
    }
}
