using System;
using System.Collections;
using UnityEngine;

public class AnalogClockController : MonoBehaviour, ITimeDisplayer
{
    [SerializeField] ArrowController HourArrow;
    [SerializeField] ArrowController MinuteArrow;
    [SerializeField] ArrowController SecondArrow;

    bool displayAlarm = false;

    Coroutine startTimer;

    public void DisplayTime(DateTime time)
    {
        // ��� ������� �������: 360 �������� = 12 �����, �.�. 30 �������� �� ������ ���
        // ��������� ���� �����, ����� ������� ������ ��������� ����� ������
        float hours = (time.Hour % 12) + time.Minute / 60f;  // 12-������� ������, + ������ ������ �� ��������
        HourArrow.transform.rotation = Quaternion.Euler(0, 0, -hours * 30f);

        // ��� �������� �������: 360 �������� = 60 �����, �.�. 6 �������� �� ������ ������
        // ��������� ���� ������, ����� ������� ������ ��������� ����� ��������
        float minutes = time.Minute + time.Second / 60f;
        MinuteArrow.transform.rotation = Quaternion.Euler(0, 0, -minutes * 6f);

        // ��� ��������� �������: 360 �������� = 60 ������, �.�. 6 �������� �� ������ �������
        float seconds = time.Second + time.Millisecond / 1000f;  // ��������� �������� ����� ������������
        SecondArrow.transform.rotation = Quaternion.Euler(0, 0, -seconds * 6f);
    }

    private void Update()
    {
        if(displayAlarm == false)
        {
            // ��������� ��������� ��� ����������� ��������� �������
            float seconds = Time.deltaTime;  
            SecondArrow.transform.rotation *= Quaternion.Euler(0, 0, -seconds * 6f);
        }
    }

    private void Awake()
    {
        ClockManager.instance.OnTimeChanged += DisplayTime;

        HourArrow.OnDragStarted += Stop;
        HourArrow.OnDragFinished += StartTimer;

        MinuteArrow.OnDragStarted += Stop;
        MinuteArrow.OnDragFinished += StartTimer;

        SecondArrow.OnDragStarted += Stop;
        SecondArrow.OnDragFinished += StartTimer;
    }

    // ������������� ������� �������, ��� ���� ����� ������ ����� ��� ����������
    void Stop()
    {
        if (startTimer != null)
            StopCoroutine(startTimer);

        ClockManager.instance.OnTimeChanged -= DisplayTime;
        displayAlarm = true;
    }

    // ������������ ������� �������
    void StartTimer()
    {
        if (startTimer != null)
            StopCoroutine(startTimer);
        startTimer = StartCoroutine(Timer());
    }

    // ����� ��� ������������� �������� �������
    IEnumerator Timer()
    {
        yield return new WaitForSeconds(10);
        ClockManager.instance.OnTimeChanged += DisplayTime;
        displayAlarm = false;
        startTimer = null;
    }

    private void OnDestroy()
    {
        ClockManager.instance.OnTimeChanged -= DisplayTime;

        HourArrow.OnDragStarted -= Stop;
        HourArrow.OnDragFinished -= StartTimer;

        MinuteArrow.OnDragStarted -= Stop;
        MinuteArrow.OnDragFinished -= StartTimer;

        SecondArrow.OnDragStarted -= Stop;
        SecondArrow.OnDragFinished -= StartTimer;
    }
}
