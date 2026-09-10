using System;
using UnityEngine;

public static class ProgressManager
{
    private const string PREFIX = "HourPeak_";

    /// <summary>
    /// ЕДИНСТВЕННЫЙ метод для сохранения прогресса.
    /// Вызывай его ПЕРЕД ЛЮБЫМ ПЕРЕХОДОМ (Next, Again, Exit, Auto-Finish).
    /// Дата сохраняется в формате: 12.10.2023 14:30:05
    /// </summary>
    public static void RecordAttempt(int level, int stage, int stars)
    {
        // 1. Сохраняем звёзды
        PlayerPrefs.SetInt(PREFIX + $"Stage_{level}_{stage}", stars);
        
        // 2. Запоминаем последнюю позицию (для кнопок Retry/Resume)
        PlayerPrefs.SetInt(PREFIX + "LastLevel", level);
        PlayerPrefs.SetInt(PREFIX + "LastStage", stage);

        // 3. 🔥 ГЛАВНОЕ: Сохраняем дату в ТОЧНОМ формате
        string currentDate = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
        PlayerPrefs.SetString(PREFIX + $"Date_{level}_{stage}", currentDate);

        PlayerPrefs.Save();
        
        Debug.Log($"[PROGRESS] Saved: Lvl {level}, Stage {stage}, Stars {stars}, Date: {currentDate}");
    }

    // Получить звёзды для конкретного этапа
    public static int GetStars(int level, int stage)
    {
        return PlayerPrefs.GetInt(PREFIX + $"Stage_{level}_{stage}", 0);
    }
    
    // Получить дату для конкретного этапа (для SaveSlotSystem)
    public static string GetDate(int level, int stage)
    {
        return PlayerPrefs.GetString(PREFIX + $"Date_{level}_{stage}", "Нет данных");
    }

    // Геттеры для кнопок в меню (Pause, EndMenu), где нет локальных переменных уровня
    public static int GetLastLevel() => PlayerPrefs.GetInt(PREFIX + "LastLevel", 1);
    public static int GetLastStage() => PlayerPrefs.GetInt(PREFIX + "LastStage", 1);

    internal static int GetStars(object level, object stage)
    {
        throw new NotImplementedException();
    }

    internal static void RecordAttempt(object currentLevel, object currentStage, object earnedStars)
    {
        throw new NotImplementedException();
    }
}