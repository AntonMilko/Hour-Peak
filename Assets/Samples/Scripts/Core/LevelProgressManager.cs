using System.Collections.Generic;
using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    private static LevelProgressManager _instance;
    public static LevelProgressManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<LevelProgressManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("LevelProgressManager");
                    _instance = go.AddComponent<LevelProgressManager>();
                }
            }
            return _instance;
        }
    }

    private Dictionary<int, Dictionary<int, int>> _progress = new Dictionary<int, Dictionary<int, int>>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeProgress();
    }

    private void InitializeProgress()
    {
        for (int i = 0; i < 36; i++)
        {
            if (!_progress.ContainsKey(i))
            {
                _progress[i] = new Dictionary<int, int>();
                _progress[i][0] = 0; // Morning
                _progress[i][1] = 0; // Evening
            }
        }
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex == 0) return true;
        return IsPartCompleted(levelIndex - 1, 1);
    }

    public bool IsMorningCompleted(int levelIndex)
    {
        return IsPartCompleted(levelIndex, 0);
    }

    public bool IsEveningCompleted(int levelIndex)
    {
        return IsPartCompleted(levelIndex, 1);
    }

    public bool IsPartCompleted(int levelIndex, int partIndex)
    {
        if (_progress.ContainsKey(levelIndex) && _progress[levelIndex].ContainsKey(partIndex))
            return _progress[levelIndex][partIndex] > 0;
        return false;
    }

    public int GetMorningStars(int levelIndex)
    {
        return GetPartStars(levelIndex, 0);
    }

    public int GetEveningStars(int levelIndex)
    {
        return GetPartStars(levelIndex, 1);
    }

    public int GetPartStars(int levelIndex, int partIndex)
    {
        if (_progress.ContainsKey(levelIndex) && _progress[levelIndex].ContainsKey(partIndex))
            return _progress[levelIndex][partIndex];
        return 0;
    }

    public void SaveProgress(int levelIndex, int partIndex, int stars)
    {
        if (!_progress.ContainsKey(levelIndex))
            _progress[levelIndex] = new Dictionary<int, int>();
        _progress[levelIndex][partIndex] = Mathf.Clamp(stars, 0, 3);
    }
}