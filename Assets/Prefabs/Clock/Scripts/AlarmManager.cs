using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlarmManager : MonoBehaviour
{
    public static AlarmManager instance;

    [SerializeField] AudioClip alarmSound;

    [SerializeField] TMP_InputField hourIF;
    [SerializeField] TMP_InputField minuteIF;
    [SerializeField] TMP_InputField secondIF;

    [SerializeField] ArrowController HourArrow;
    [SerializeField] ArrowController MinuteArrow;
    [SerializeField] ArrowController SecondArrow;

    [SerializeField] Button SetButton;
    [SerializeField] Button RemoveButton;



    DateTime alarmAtTime;
    Coroutine alarmTimer = null;

    float previousAngle = 0f;
    int fullRotations = 0;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(transform.root.gameObject);

        SetButton.interactable = false;
        RemoveButton.interactable = false;

        // ������������� �� ������� ���������� �������
        ClockManager.instance.OnTimeRefreshed += RecalculateAlarmTimer;

        HourArrow.OnDrag += (angle) =>
        {
            // ���������� ������� ����� ������� ����� � ����������
            float angleDelta = angle - previousAngle;

            // ���� ��������� ������� ����� ���� (������� ����� 360 � 0 ���������)
            if (Mathf.Abs(angleDelta) > 180) // ������� �� 360 � 0
            {
                fullRotations++;
            }

            previousAngle = angle;

            // ������������ ���, �������� �� �������� ���������� ������ ��������
            int hour = (11 - (int)(angle / 30)) + (fullRotations * 12) % 24;


            // ��������� ��������� ����
            hourIF.text = hour.ToString();
            hourIF.onValueChanged?.Invoke(hourIF.text);
        };
        MinuteArrow.OnDrag += (angle) =>
        {
            minuteIF.text = (59 - (int)(angle / 6)).ToString();
            minuteIF.onValueChanged?.Invoke(minuteIF.text);
        };
        SecondArrow.OnDrag += (angle) =>
        {
            secondIF.text = (59 - (int)(angle / 6)).ToString();
            secondIF.onValueChanged?.Invoke(secondIF.text);
        };

        hourIF.onSelect.AddListener((_) => SetEmptyStringsToZero());
        minuteIF.onSelect.AddListener((_) => SetEmptyStringsToZero());
        secondIF.onSelect.AddListener((_) => SetEmptyStringsToZero());


        hourIF.onEndEdit.AddListener(input =>
        {
            // ���������� �������� [0;23]
            if (!Regex.IsMatch(input, @"^(?:[0-9]|1[0-9]|2[0-3])$"))
                hourIF.text = "0";
        });
        minuteIF.onEndEdit.AddListener(input =>
        {
            // ���������� �������� [0;59]
            if (!Regex.IsMatch(input, @"^([0-5]?[0-9])$"))
                minuteIF.text = "0";
        });
        secondIF.onEndEdit.AddListener(input =>
        {
            // ���������� �������� [0;59]
            if (!Regex.IsMatch(input, @"^([0-5]?[0-9])$"))
                secondIF.text = "0";
        });

        hourIF.onValueChanged.AddListener((value) => OnInputFieldValueChanged(value));
        minuteIF.onValueChanged.AddListener((value) => OnInputFieldValueChanged(value));
        secondIF.onValueChanged.AddListener((value) => OnInputFieldValueChanged(value));
    }

    private void OnInputFieldValueChanged(string value)
    {
        if (value != "")
            SetEmptyStringsToZero();

        if (CheckValueChanged())
            SetButton.interactable = true;
    }

    bool CheckValueChanged()
    {
        if (TryGetIFTime(out DateTime time))
            if (time.Equals(alarmAtTime))
                return false;

        return true;
    }

    private bool TryGetIFTime(out DateTime time)
    {
        time = DateTime.MinValue;
        if (int.TryParse(hourIF.text, out int hour) == false)
            return false;

        if (int.TryParse(minuteIF.text, out int minute) == false)
            return false;

        if (int.TryParse(secondIF.text, out int second) == false)
            return false;

        time = new DateTime(DateTime.MinValue.Year,
                                     DateTime.MinValue.Month,
                                     DateTime.MinValue.Day,
                                     hour,
                                     minute,
                                     second);
        return true;
    }

    void SetEmptyStringsToZero()
    {
        if (hourIF.text == "")
        {
            hourIF.text = "0";
            hourIF.onValueChanged?.Invoke("0");
        }
        if (minuteIF.text == "")
        {
            minuteIF.text = "0";
            minuteIF.onValueChanged?.Invoke("0");
        }
        if (secondIF.text == "")
        {
            secondIF.text = "0";
            secondIF.onValueChanged?.Invoke("0");
        }
    }

    // �������� ������� �� ������ �������� �������
    private void RecalculateAlarmTimer(DateTime currentTime)
    {
        if (alarmTimer == null)
            return;

        SetAlarmTimer(currentTime);
    }

    // �������� ������� �� ������ �������� �������
    private void SetAlarmTimer(DateTime currentTime)
    {
        if (alarmTimer != null)
            StopCoroutine(alarmTimer);

        // ������������ ������� �� ������� �� ������� ����������
        TimeSpan timeUntilAlarm = alarmAtTime.TimeOfDay - currentTime.TimeOfDay;

        Debug.Log(timeUntilAlarm);

        if (timeUntilAlarm < TimeSpan.Zero) // ���� ������� ����� ��� ������ ������� ����������
            // ��������� ������ ��������� �� ��������� ����
            timeUntilAlarm = timeUntilAlarm.Add(TimeSpan.FromDays(1));

        alarmTimer = StartCoroutine(AlarmTimer((float)timeUntilAlarm.TotalSeconds));

    }

    // ���������� ������� Set � ����������
    public void SetAlarm()
    {
        if (TryGetIFTime(out DateTime time))
            SetAlarm(time);
    }

    // ������������� ����� ����� ����������
    public void SetAlarm(DateTime time)
    {
        hourIF.text = time.TimeOfDay.Hours.ToString();
        minuteIF.text = time.TimeOfDay.Minutes.ToString();
        secondIF.text = time.TimeOfDay.Seconds.ToString();

        alarmAtTime = time;

        // ���� ������� ���, ������ �� �������������
        if (alarmTimer == null)
            ClockManager.instance.OnTimeRefreshed += RecalculateAlarmTimer;

        SetAlarmTimer(ClockManager.instance.CurrentTime);

        SetButton.interactable = false;
        RemoveButton.interactable = true;
    }

    // ���������� ������� Remove � ����������
    public void RemoveAlarm()
    {
        RemoveButton.interactable = false;

        hourIF.text = "";
        minuteIF.text = "";
        secondIF.text = "";

        ClockManager.instance.OnTimeRefreshed -= RecalculateAlarmTimer;

        StopCoroutine(alarmTimer);
        alarmTimer = null;

        SoundManager.instance.StopSound();
    }

    // ������������ ����������
    private void TriggerAlarm()
    {
        SoundManager.instance.PlaySound(alarmSound);
        Debug.Log("��������� ��������!");
    }

    // ������ ��� ����������
    IEnumerator AlarmTimer(float seconds)
    {
        while (true)
        {
            Debug.Log("Waiting for " + seconds);
            yield return new WaitForSeconds(seconds);
            TriggerAlarm();
            seconds = 24 * 60 * 60;
        }
    }

    private void OnDestroy()
    {
        if (alarmTimer != null)
            ClockManager.instance.OnTimeRefreshed -= RecalculateAlarmTimer;
    }
}
