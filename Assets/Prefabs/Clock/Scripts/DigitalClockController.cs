using System;
using TMPro;
using UnityEngine;

public class DigitalClockController : MonoBehaviour, ITimeDisplayer
{
    [SerializeField] TMP_Text textField;

    public void DisplayTime(DateTime time)
    {
        textField.text = time.ToString("HH:mm:ss");
    }

    private void Awake()
    {
        ClockManager.instance.OnTimeChanged += DisplayTime;
    }

    private void OnDestroy()
    {
        ClockManager.instance.OnTimeChanged -= DisplayTime;
    }
}
