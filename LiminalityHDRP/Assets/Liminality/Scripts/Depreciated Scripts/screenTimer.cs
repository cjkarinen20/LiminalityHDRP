using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class screenTimer : MonoBehaviour
{
    public PauseMenu pauseMenu;
    public GameObject timeText;
    private Text timerText;

    void Start()
    {
        timerText = timeText.GetComponent<Text>();
    }

    void Update()
    {
        displayTime();
    }
    string LeadingZero (int n)
    {
        return n.ToString().PadLeft(2, '0');
    }
    private void displayTime()
    {
        DateTime time = DateTime.Now;
        string hour = LeadingZero(time.Hour);
        string minute = LeadingZero(time.Minute);
        string second = LeadingZero(time.Second);

        timerText.text = hour + ":" + minute + ":" + second;
    }
}
