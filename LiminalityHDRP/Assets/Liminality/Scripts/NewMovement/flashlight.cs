using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class flashlight : MonoBehaviour
{
    [Header("Flashlight Parameters")]
    public AudioSource AudioSource;
    public AudioClip flashlightClick;
    public AudioClip flashlightFlicker;
    public GameObject followTarget;
    public Light flashLight;
    public KeyCode lightToggle = KeyCode.F;
    private Vector3 vectorOffset;
    [SerializeField] private float speed = 3.0f;


    [Header("Battery Life Parameters")]
    [SerializeField] private bool batteryLifeEnabled = true;
    [SerializeField] private float maxBattery = 100;
    [SerializeField] private float batteryUseMultiplier = 1;
    [SerializeField] private float timeBeforeBatteryRegenStarts = 5;
    [SerializeField] private float batteryIncreaseIncrement = 2;
    [SerializeField] private float batteryTimeIncrement = 0.1f;
    private float currentBatteryLevel;
    private Coroutine rechargingBattery;
    public static Action<float> OnBatteryChange;






    void Start()
    {
        currentBatteryLevel = maxBattery;
        flashLight = GetComponent<Light>();
        GetComponent<Light>().enabled = false;
        followTarget = Camera.main.gameObject;
        vectorOffset = transform.position - followTarget.transform.position;
    }


    void Update()
    {
       HandleFlashlight();
       HandleBattery();

       Debug.Log("Battery: " + currentBatteryLevel);
    }
    private void HandleBattery()
    {
        if (batteryLifeEnabled && GetComponent<Light>().enabled)
        {
            if (rechargingBattery != null)
            {
                StopCoroutine(rechargingBattery);
                rechargingBattery = null;
            }
            currentBatteryLevel -= batteryUseMultiplier * Time.deltaTime;

            if (currentBatteryLevel < 0)
                currentBatteryLevel = 0;

            OnBatteryChange?.Invoke(currentBatteryLevel);

            if (currentBatteryLevel <= 0)
            {
                StartCoroutine(BatteryDeath());
            }
        }
        if (!GetComponent<Light>().enabled && currentBatteryLevel < maxBattery && rechargingBattery == null)
        {
            rechargingBattery = StartCoroutine(RechargeBattery());
        }
    }
    private void HandleFlashlight()
    {
        //Added this line to rotate the offset vector according to the players facing direction
        Vector3 rotatedVectorOffset = Vector3.RotateTowards(vectorOffset, followTarget.transform.forward, float.MaxValue, float.MaxValue);
        transform.position = followTarget.transform.position + rotatedVectorOffset; // Replaced vectorOffset with rotatedVectorOffset variable
        transform.rotation = Quaternion.Slerp(transform.rotation, followTarget.transform.rotation, speed * Time.deltaTime);
        if (Input.GetKeyDown(lightToggle))
        {
            if (GetComponent<Light>().enabled)
            {
                AudioSource.PlayOneShot(flashlightClick);
                GetComponent<Light>().enabled = false;
            }
            else if (!GetComponent<Light>().enabled && currentBatteryLevel > 0)
            {
                AudioSource.PlayOneShot(flashlightClick);
                GetComponent<Light>().enabled = true;
            }

        }
    }
    private IEnumerator RechargeBattery()
    {
        yield return new WaitForSeconds(timeBeforeBatteryRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(batteryTimeIncrement);

        while (currentBatteryLevel < maxBattery)
        {
            currentBatteryLevel += batteryIncreaseIncrement;

            if (currentBatteryLevel > maxBattery)
                currentBatteryLevel = maxBattery;

            yield return timeToWait;
        }
        rechargingBattery = null;
    }
    private IEnumerator BatteryDeath()
    {
        AudioSource.PlayOneShot(flashlightFlicker);
        yield return new WaitForSeconds(2);
        GetComponent<Light>().enabled = false;
    }
}
