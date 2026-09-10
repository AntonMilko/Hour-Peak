using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Автоматически добавляет сцены в Build Settings в строгом порядке:
/// StartMenu → Уровни (по 4 сцены каждый) → Game completion certificate
/// </summary>
public class AddScenesToBuildProfile : MonoBehaviour
{
    [MenuItem("Tools/Configure Build Settings")]
    public static void ConfigureBuildSettings()
    {
        Debug.Log("🔧 Настройка Build Settings...");

        var scenesList = new List<EditorBuildSettingsScene>();

        // 1. StartMenu (первая сцена)
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/StartMenu.unity", true));
        Debug.Log("  ✅ StartMenu");

        // 2. Level 1 — 3 сцены на часть (Morning → Evening)
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 1/Morning/Morning 1.1.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 1/Morning/Morning 1.2.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 1/Morning/Morning 1.3.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 1/Evening/Evening 1.3.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 1/Evening/Evening 1.2.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 1/Evening/Evening 1.1.unity", true));
        Debug.Log("  ✅ Level 1 (6 сцен)");

        // 3. Уровни 2-18 — по 4 сцены (Morning 1.1, 1.2 → Evening 2.1, 2.2)
        int[] levels2To18 = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
        
        foreach (int level in levels2To18)
        {
            scenesList.Add(new EditorBuildSettingsScene($"Assets/Scenes/Level {level}/Morning/Morning {level}.1.unity", true));
            scenesList.Add(new EditorBuildSettingsScene($"Assets/Scenes/Level {level}/Morning/Morning {level}.2.unity", true));
            scenesList.Add(new EditorBuildSettingsScene($"Assets/Scenes/Level {level}/Evening/Evening {level}.2.unity", true));
            scenesList.Add(new EditorBuildSettingsScene($"Assets/Scenes/Level {level}/Evening/Evening {level}.1.unity", true));
        }
        Debug.Log($"  ✅ Уровни 2-18: {levels2To18.Length} уровней × 4 сцены = {levels2To18.Length * 4} сцен");

        // 4. Level 19 — 3 сцены на часть (Morning → Evening)
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 19/Morning/Morning 19.1.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 19/Morning/Morning 19.2.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 19/Morning/Morning 19.3.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 19/Evening/Evening 19.3.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 19/Evening/Evening 19.2.unity", true));
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Level 19/Evening/Evening 19.1.unity", true));
        Debug.Log("  ✅ Level 19 (6 сцен)");

        // 5. Уровни 20-36 — по 4 сцены (Morning 1.1, 1.2 → Evening 2.1, 2.2)
        int[] levels20To36 = { 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36 };
        
        foreach (int level in levels20To36)
        {
            scenesList.Add(new EditorBuildSettingsScene($"Assets/Scenes/Level {level}/Morning/Morning {level}.1.unity", true));
            scenesList.Add(new EditorBuildSettingsScene($"Assets/Scenes/Level {level}/Morning/Morning {level}.2.unity", true));
            scenesList.Add(new EditorBuildSettingsScene($"Assets/Scenes/Level {level}/Evening/Evening {level}.2.unity", true));
            scenesList.Add(new EditorBuildSettingsScene($"Assets/Scenes/Level {level}/Evening/Evening {level}.1.unity", true));
        }
        Debug.Log($"  ✅ Уровни 20-36: {levels20To36.Length} уровней × 4 сцены = {levels20To36.Length * 4} сцен");

        // 5. Game completion certificate (последняя сцена)
        scenesList.Add(new EditorBuildSettingsScene("Assets/Scenes/Game completion certificate.unity", true));
        Debug.Log("  ✅ Game completion certificate");

        // Применяем список
        EditorBuildSettings.scenes = scenesList.ToArray();

        Debug.Log($"📊 ИТОГО: {scenesList.Count} сцен в Build Settings");
        Debug.Log($"✅ Завершено! Откройте File → Build Settings для проверки.");
    }

    [MenuItem("Tools/Show Current Build Scenes")]
    public static void ShowCurrentBuildScenes()
    {
        Debug.Log("📋 Текущие сцены в Build Settings:");
        
        int count = 0;
        foreach (var scene in EditorBuildSettings.scenes)
        {
            Debug.Log($"  [{count}] {scene.path}");
            count++;
        }
        Debug.Log($"✅ Всего сцен: {count}");
    }
}
