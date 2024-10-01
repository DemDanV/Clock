using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks; // Добавлено для async/await
using UnityEngine;
using UnityEngine.Networking;

public class ClockManager : MonoBehaviour, ITimeKeeper
{
    public static ClockManager instance;

    const string TIME_API2 = "https://worldtimeapi.org/api/ip";
    const string TIME_API3 = "https://worldclockapi.com/api/json/utc/now";
    const string TIME_API = "https://api.ipgeolocation.io/timezone?apiKey=71d5d0df26db4de5a2c5e5143a5e8d3a";



    [SerializeField] Transform NoAPIConnectionIcon;

    DateTime _currentTime;
    DateTime currentTime
    {
        get { return _currentTime; }
        set { _currentTime = value; OnTimeChanged?.Invoke(_currentTime); }
    }
    public DateTime CurrentTime => _currentTime;

    Coroutine timeCalculator;

    public Action<DateTime> OnTimeChanged;
    public Action<DateTime> OnTimeRefreshed;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance == this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        NoAPIConnectionIcon.gameObject.SetActive(false);

        //await CheckPing();

        if (await TryGetTimeFromAPI() == false)
            SetTime(DateTime.Now);

        StartCoroutine(RefreshTimeEveryHour());
    }

    [ContextMenu("Move back in time")]
    // Moves CurrentTime back 10 seconds
    void BackInFuture()
    {
        currentTime = currentTime.Subtract(new TimeSpan(0, 0, 10));
        OnTimeRefreshed?.Invoke(_currentTime);
    }

    IEnumerator RefreshTimeEveryHour()
    {
        while(true)
        {
            yield return new WaitForSecondsRealtime(60 * 60);
            yield return TryGetTimeFromAPI();
        }
    }

    //private async Task<bool> CheckPing()
    //{
    //    // Засекаем время начала запроса
    //    float startTime = Time.time;

    //    // Отправляем запрос
    //    using (UnityWebRequest request = UnityWebRequest.Get(TIME_API))
    //    {
    //        request.certificateHandler = new AcceptAllCertificatesSigned();
    //        var operation = request.SendWebRequest();

    //        while (!operation.isDone)
    //        {
    //            await Task.Yield();
    //        }

    //        // Засекаем время окончания запроса
    //        float endTime = Time.time;
    //        float ping = (endTime - startTime) * 1000f; // Пинг в миллисекундах

    //        // Проверка на ошибки
    //        if (request.result != UnityWebRequest.Result.Success)
    //        {
    //            Debug.LogError("Сайт недоступен. Ошибка: " + request.error);
    //            return true;
    //        }
    //        else
    //        {
    //            Debug.Log("Сайт доступен. Пинг: " + ping + " мс");
    //            return false;
    //        }
    //    }
    //}

    private async Task<bool> TryGetTimeFromAPI()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(TIME_API))
        {
            request.certificateHandler = new AcceptAllCertificatesSigned();
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching time: {request.error}");
                NoAPIConnectionIcon.gameObject.SetActive(true);

                return false;
            }
            else
            {
                try
                {
                    string jsonResult = request.downloadHandler.text;
                    var apiResponse = JsonUtility.FromJson<TimeApiResponse>(jsonResult);
                    SetTime(DateTime.Parse(apiResponse.date_time));
                    NoAPIConnectionIcon.gameObject.SetActive(false);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error parsing time: {ex.Message}");
                    NoAPIConnectionIcon.gameObject.SetActive(true);

                    return false;
                }
            }
        }
    }

    IEnumerator CalculateTime()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(1);
            currentTime = currentTime.AddSeconds(1);
        }
    }

    public void SetTime(DateTime time)
    {
        currentTime = time;
        OnTimeRefreshed?.Invoke(currentTime);

        if (timeCalculator != null)
            StopCoroutine(timeCalculator);

        timeCalculator = StartCoroutine(CalculateTime());
    }

    private async void OnApplicationFocus(bool focus)
    {
        if (focus == false)
            return;

        if (await TryGetTimeFromAPI() == false)
            SetTime(DateTime.Now);
    }

    public DateTime GetTime()
    {
        return CurrentTime;
    }
}

[Serializable]
public class TimeApiResponse
{
    public string date_time;
}

public class AcceptAllCertificatesSigned : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        // Принудительно принимаем все сертификаты
        return true;
    }
}

public interface ITimeKeeper
{
    public DateTime GetTime();
}