using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ClockManager : MonoBehaviour
{
    public static ClockManager instance;

    const string FREE_TIME_API = "https://worldtimeapi.org/api/ip";
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
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(transform.root.gameObject);
    }

    private async void Start()
    {
        NoAPIConnectionIcon.gameObject.SetActive(false);

        if (await TryGetTime() == false)
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
            yield return TryGetTime();
        }
    }

    private async Task<bool> TryGetTime()
    {
        if (await TryGetTimeFromAPI(FREE_TIME_API) == false)
            if (await TryGetTimeFromAPI(TIME_API) == false)
            {
                NoAPIConnectionIcon.gameObject.SetActive(true);
                return false;
            }

        NoAPIConnectionIcon.gameObject.SetActive(false);
        return true;
    }

    private async Task<bool> TryGetTimeFromAPI(string api)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(api))
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
                Debug.LogWarning($"Error fetching time: {request.error}");
                return false;
            }
            else
            {
                try
                {
                    string jsonResult = request.downloadHandler.text;
                    var apiResponse = JsonUtility.FromJson<TimeApiResponse>(jsonResult);
                    SetTime(DateTime.Parse(apiResponse.date_time));

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error parsing time: {ex.Message}");
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

        if (await TryGetTime() == false)
            SetTime(DateTime.Now);
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