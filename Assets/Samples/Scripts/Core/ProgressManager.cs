using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Singleton-менеджер прогресса для всех 148 сцен.
/// Порядок прохождения:
/// Level 1:
///   Утро: 1.1 → 1.2 → 1.3
///   Вечер: 1.3 → 1.2 → 1.1 (обратный порядок!)
/// Level 2-18:
///   Утро: X.1 → X.2
///   Вечер: X.2 → X.1 (обратный порядок!)
/// Level 19:
///   Утро: 19.1 → 19.2 → 19.3
///   Вечер: 19.3 → 19.2 → 19.1 (обратный порядок!)
/// Level 20-36:
///   Утро: X.1 → X.2
///   Вечер: X.2 → X.1 (обратный порядок!)
/// Итого: 148 сцен
/// </summary>
public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance { get; private set; }

    private const string SAVE_FILENAME = "global_progress.json";
    private const int TOTAL_LEVELS = 36;

    [Serializable]
    private class SceneProgress
    {
        public string sceneName;
        public bool isCompleted;
        public int stars;
        public float time;
    }

    private Dictionary<string, SceneProgress> _sceneProgress = new Dictionary<string, SceneProgress>();
    private List<string> _sceneOrder;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Убедимся, что GameObject является root перед вызовом DontDestroyOnLoad
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning($"[ProgressManager] GameObject не является root! Родитель: {transform.parent.name}");
        }
        
        InitializeSceneOrder();
        LoadProgress();
    }

    /// <summary>
    /// Определяет порядок всех 148 сцен.
    /// Утро: прямой порядок (1.1 → 1.2 → 1.3)
    /// Вечер: обратный порядок (1.3 → 1.2 → 1.1)
    /// </summary>
    private void InitializeSceneOrder()
    {
        _sceneOrder = new List<string>();

        for (int level = 1; level <= TOTAL_LEVELS; level++)
        {
            bool isSpecialLevel = (level == 1 || level == 19);
            int partsPerTime = isSpecialLevel ? 3 : 2;

            // Утро: прямой порядок (Morning 1.1 → 1.2 → 1.3)
            for (int part = 1; part <= partsPerTime; part++)
            {
                string sceneName = $"Level {level}/Morning/Morning {level}.{part}";
                _sceneOrder.Add(sceneName);
                if (!_sceneProgress.ContainsKey(sceneName))
                {
                    _sceneProgress[sceneName] = new SceneProgress
                    {
                        sceneName = sceneName,
                        isCompleted = false,
                        stars = 0,
                        time = 0f
                    };
                }
            }

            // Вечер: обратный порядок (Evening 1.3 → 1.2 → 1.1)
            for (int part = partsPerTime; part >= 1; part--)
            {
                string sceneName = $"Level {level}/Evening/Evening {level}.{part}";
                _sceneOrder.Add(sceneName);
                if (!_sceneProgress.ContainsKey(sceneName))
                {
                    _sceneProgress[sceneName] = new SceneProgress
                    {
                        sceneName = sceneName,
                        isCompleted = false,
                        stars = 0,
                        time = 0f
                    };
                }
            }
        }

        Debug.Log($"✅ ProgressManager инициализирован: {_sceneOrder.Count} сцен в порядке прохождения");
        Debug.Log($"📋 Порядок: {_sceneOrder[0]} → {_sceneOrder[_sceneOrder.Count - 1]}");
    }

    /// <summary>
    /// Получает имя следующей сцены для прохождения.
    /// </summary>
    public string GetNextScene()
    {
        foreach (string sceneName in _sceneOrder)
        {
            if (!_sceneProgress[sceneName].isCompleted)
            {
                return sceneName;
            }
        }
        Debug.Log("🏆 Все сцены пройдены!");
        return null;
    }

    /// <summary>
    /// Проверяет, разблокирована ли сцена (предыдущая пройдена).
    /// </summary>
    public bool IsSceneUnlocked(string sceneName)
    {
        if (!_sceneProgress.ContainsKey(sceneName)) return false;

        int index = _sceneOrder.IndexOf(sceneName);
        if (index <= 0) return true;

        return _sceneProgress[_sceneOrder[index - 1]].isCompleted;
    }

    /// <summary>
    /// Сохраняет прохождение сцены.
    /// </summary>
    public void SaveSceneCompletion(string sceneName, int stars = 0, float time = 0f)
    {
        if (!_sceneProgress.ContainsKey(sceneName))
        {
            Debug.LogWarning($"⚠️ Сцена {sceneName} не найдена в прогрессе");
            return;
        }

        _sceneProgress[sceneName].isCompleted = true;
        _sceneProgress[sceneName].stars = Mathf.Clamp(stars, 0, 3);
        _sceneProgress[sceneName].time = time;

        SaveProgress();
        Debug.Log($"✅ Сцена {sceneName} сохранена как пройденная ({stars}/3 ★, {time:F0}s)");
    }

    /// <summary>
    /// Проверяет, пройдена ли сцена.
    /// </summary>
    public bool IsSceneCompleted(string sceneName)
    {
        return _sceneProgress.ContainsKey(sceneName) && _sceneProgress[sceneName].isCompleted;
    }

    /// <summary>
    /// Получает количество звёзд за сцену.
    /// </summary>
    public int GetSceneStars(string sceneName)
    {
        return _sceneProgress.ContainsKey(sceneName) ? _sceneProgress[sceneName].stars : 0;
    }

    /// <summary>
    /// Получает общее количество звёзд.
    /// </summary>
    public int GetTotalStars()
    {
        int total = 0;
        foreach (var progress in _sceneProgress.Values)
        {
            total += progress.stars;
        }
        return total;
    }

    /// <summary>
    /// Получает общую статистику.
    /// </summary>
    public (int completed, int total) GetCompletionStats()
    {
        int completed = 0;
        foreach (var progress in _sceneProgress.Values)
        {
            if (progress.isCompleted) completed++;
        }
        return (completed, _sceneOrder.Count);
    }

    /// <summary>
    /// Сохраняет прогресс в JSON.
    /// </summary>
    private void SaveProgress()
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
            string json = JsonUtility.ToJson(new SaveDataWrapper { progress = _sceneProgress }, true);
            File.WriteAllText(savePath, json);
            Debug.Log($"💾 Прогресс сохранён: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Ошибка сохранения прогресса: {e.Message}");
        }
    }

    /// <summary>
    /// Загружает прогресс из JSON.
    /// </summary>
    private void LoadProgress()
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILENAME);
            if (File.Exists(savePath))
            {
                string json = File.ReadAllText(savePath);
                SaveDataWrapper wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);
                if (wrapper.progress != null)
                {
                    _sceneProgress = wrapper.progress;
                    Debug.Log($"💾 Прогресс загружен: {_sceneOrder.Count} сцен");
                }
            }
            else
            {
                Debug.Log($"ℹ️ Новый прогресс: {_sceneOrder.Count} сцен");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Ошибка загрузки прогресса: {e.Message}");
        }
    }

    /// <summary>
    /// Сбрасывает весь прогресс.
    /// </summary>
    public void ResetProgress()
    {
        _sceneProgress.Clear();
        InitializeSceneOrder();
        SaveProgress();
        Debug.Log("🔄 Прогресс сброшен");
    }

    [Serializable]
    private class SaveDataWrapper
    {
        public Dictionary<string, SceneProgress> progress;
    }
}
