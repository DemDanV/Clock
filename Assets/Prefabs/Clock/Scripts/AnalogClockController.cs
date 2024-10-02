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
        // Для часовой стрелки: 360 градусов = 12 часов, т.е. 30 градусов на каждый час
        // Учитываем долю минут, чтобы стрелка плавно двигалась между часами
        float hours = (time.Hour % 12) + time.Minute / 60f;  // 12-часовой формат, + минута влияет на смещение
        HourArrow.transform.rotation = Quaternion.Euler(0, 0, -hours * 30f);

        // Для минутной стрелки: 360 градусов = 60 минут, т.е. 6 градусов на каждую минуту
        // Учитываем долю секунд, чтобы стрелка плавно двигалась между минутами
        float minutes = time.Minute + time.Second / 60f;
        MinuteArrow.transform.rotation = Quaternion.Euler(0, 0, -minutes * 6f);

        // Для секундной стрелки: 360 градусов = 60 секунд, т.е. 6 градусов на каждую секунду
        float seconds = time.Second + time.Millisecond / 1000f;  // Добавляем точность через миллисекунды
        SecondArrow.transform.rotation = Quaternion.Euler(0, 0, -seconds * 6f);
    }

    private void Update()
    {
        if(displayAlarm == false)
        {
            // Добавляем плавность для перемещения секундной стрелки
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

    // Останавливаем поворот стрелок, для того чтобы задать время для будильника
    void Stop()
    {
        if (startTimer != null)
            StopCoroutine(startTimer);

        ClockManager.instance.OnTimeChanged -= DisplayTime;
        displayAlarm = true;
    }

    // Возобновляем поворот стрелок
    void StartTimer()
    {
        if (startTimer != null)
            StopCoroutine(startTimer);
        startTimer = StartCoroutine(Timer());
    }

    // Тймер для возобновления поворота стрелок
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
