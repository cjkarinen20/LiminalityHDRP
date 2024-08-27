using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class UIClock : MonoBehaviour
{

    public string clockFormat;

    public TextMeshProUGUI clockText;

    public System.TimeSpan timeSpan = new System.TimeSpan(0, 0, 0, 0, 0);

    public System.DateTime date = new System.DateTime(1999,04,24);

    public float timeRate = 1;

    private void Update()
    {
        float milliseconds = Time.deltaTime * 1000 * timeRate;

        timeSpan += new System.TimeSpan(0, 0, 0, 0, (int)milliseconds);

        System.DateTime dateTime = date.Add(timeSpan);

        clockText.text = dateTime.ToString(@clockFormat, new CultureInfo("en-US"));
    }
    public void AddTime(int value)
    {
        timeSpan += new System.TimeSpan(value, 0, 0);
    }

}
