using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

public class BusStop : MonoBehaviour
{
    public GameObject schedulePanel;   // Панель с расписанием
    public TMP_Text scheduleText;      // TMP_Text для отображения расписания
    public SAVENUMBER timerDisplay;    // Ссылка на TimerDisplay
    public GameObject spawnPoint;
    public Camera cameraMain;

    public string busStopID;           // Уникальный ID остановки
    private string jsonPath => $"{Application.dataPath}/Scenes/Schedule/{busStopID}_busSchedule.json";

    public IComponent myObject { get; private set; }

    public Dictionary<string, List<string>> busSchedules = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> recordedBusTimes = new Dictionary<string, List<string>>();

    [SerializeField] private bool isRecordingMode = true;

    // Смещение времени из другой сцены
    private float timeOffset;

    private void Awake()
    {
        if (string.IsNullOrEmpty(busStopID))
        {
            busStopID = System.Guid.NewGuid().ToString();  // Генерируем уникальный ID
        }

    }

    private static void Awake2()
    {
        throw new NotImplementedException(); // Строка 33
    }

    private static void Awake1()
    {
        // Реальная логика инициализации
        Debug.Log("BusStop initialized");
    }

    private SAVENUMBER GetFindObjectOfType()
    {
        throw new NotImplementedException();
    }

    private void Start()
    {
        schedulePanel.SetActive(false);
        if (File.Exists(jsonPath))
        {
            LoadRecordedBusTimes();  // Загружаем данные из JSON
        }
        else
        {
            Debug.LogWarning("Файл с расписанием не найден.");
        }

        if (isRecordingMode)
        {
            Debug.Log("Режим записи активен.");
        }
        Debug.Log($"Смещение времени: {timeOffset} минут.");
    }

    private void Start2()
    {
        if (myObject == null)
        {
            myObject = GetComponent<IComponent>();
        }
    }

    private static void Start1()
    {
        throw new NotImplementedException(); // Вот виновник ошибки
    }

    private SAVENUMBER GetFindObjectOfType1()
    {
        throw new NotImplementedException();
    }

    public void SpawnPlayer(GameObject player)
    {
        player.transform.position = spawnPoint.transform.position;
        Debug.Log("Игрок перемещён на остановку.");
    }

    void OnMouseDown()
    {
        if (cameraMain == Camera.main)
        {
            schedulePanel.SetActive(!schedulePanel.activeSelf);
            if (schedulePanel.activeSelf)
            {
                UpdateSchedule();
            }
        }
    }

    void UpdateSchedule()
    {
        float currentTimeInMinutes = timerDisplay.currentTime;
        scheduleText.text = "Расписание автобусов:\n";

        foreach (var route in recordedBusTimes)
        {
            scheduleText.text += $"{route.Key}:\n";

            foreach (string busTime in route.Value)
            {
                if (TryParseBusTime(busTime, out float busTimeInMinutes))
                {
                    float adjustedTime = (busTimeInMinutes + timeOffset) % 1440;  // Ограничиваем до 24 часов

                    if (adjustedTime > currentTimeInMinutes)
                    {
                        string formattedTime = FormatTime(adjustedTime);
                        scheduleText.text += $"  {formattedTime}\n";
                    }
                }
            }
        }

        if (scheduleText.text == "Расписание автобусов:\n")
        {
            scheduleText.text += "Сегодня больше автобусов нет.";
        }
    }

    string FormatTime(float minutes)
    {
        int hours = Mathf.FloorToInt(minutes / 60) % 24;
        int mins = Mathf.FloorToInt(minutes % 60);
        return $"{hours:D2}:{mins:D2}";
    }

    bool TryParseBusTime(string time, out float minutes)
    {
        string[] parts = time.Replace('.', ':').Split(':');
        if (parts.Length == 2 && int.TryParse(parts[0], out int hours) && int.TryParse(parts[1], out int mins))
        {
            minutes = hours * 60 + mins;
            return true;
        }

        minutes = 0;
        return false;
    }

    public void RecordBusArrival(string routeName)
    {
        float currentTimeInMinutes = timerDisplay.currentTime;
        string formattedTime = FormatTime(currentTimeInMinutes);

        if (!recordedBusTimes.ContainsKey(routeName))
        {
            recordedBusTimes[routeName] = new List<string>();
        }

        if (recordedBusTimes[routeName].Contains(formattedTime))
        {
            Debug.LogWarning($"Время {formattedTime} для маршрута {routeName} уже существует.");
            return;
        }

        recordedBusTimes[routeName].Add(formattedTime);
        SaveRecordedBusTimes();

        Debug.Log($"Записано прибытие автобуса на маршрут {routeName} в {formattedTime}.");
    }

    public void RecordSchedule()
    {
        if (isRecordingMode)
        {
            RecordBusTimes();
            Debug.Log("Расписание записано вручную.");
        }
        else
        {
            Debug.LogWarning("Невозможно записать расписание: режим записи выключен.");
        }
    }

    void RecordBusTimes()
    {
        recordedBusTimes.Clear();
        foreach (var route in busSchedules)
        {
            recordedBusTimes[route.Key] = new List<string>(route.Value);
        }
        SaveRecordedBusTimes();
        Debug.Log("Время прибытия автобусов записано.");
    }

    private void SaveRecordedBusTimes()
    {
        try
        {
            string directoryPath = Path.GetDirectoryName(jsonPath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string jsonData = JsonUtility.ToJson(new BusScheduleData(recordedBusTimes), true);
            File.WriteAllText(jsonPath, jsonData);
            Debug.Log($"Расписание сохранено для остановки {busStopID}.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при сохранении JSON для {busStopID}: {e.Message}");
        }
    }

    private void LoadRecordedBusTimes()
    {
        try
        {
            if (File.Exists(jsonPath))
            {
                string jsonData = File.ReadAllText(jsonPath);
                BusScheduleData data = JsonUtility.FromJson<BusScheduleData>(jsonData);

                if (data != null)
                {
                    recordedBusTimes = data.ToDictionary();
                    Debug.Log($"Расписание загружено для остановки {busStopID}.");
                }
                else
                {
                    Debug.LogWarning($"JSON-файл для остановки {busStopID} пустой или некорректный.");
                }
            }
            else
            {
                Debug.LogWarning($"Файл с расписанием для остановки {busStopID} не найден.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при загрузке JSON для {busStopID}: {e.Message}");
        }
    }

    [System.Serializable]
    public class BusScheduleData
    {
        public List<RouteData> routes = new List<RouteData>();

        public BusScheduleData(Dictionary<string, List<string>> data)
        {
            foreach (var route in data)
            {
                routes.Add(new RouteData(route.Key, route.Value));
            }
        }

        public Dictionary<string, List<string>> ToDictionary()
        {
            var dictionary = new Dictionary<string, List<string>>();
            foreach (var route in routes)
            {
                dictionary[route.routeName] = new List<string>(route.times);
            }
            return dictionary;
        }
    }

    [System.Serializable]
    public class RouteData
    {
        public string routeName;
        public List<string> times;

        public RouteData(string name, List<string> times)
        {
            routeName = name;
            this.times = times;
        }
    }
}

public class SAVENUMBER
{
    internal readonly float currentTime;
}