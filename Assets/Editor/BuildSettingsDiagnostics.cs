using UnityEngine;
using UnityEditor;
using System.Linq;

/// <summary>
/// Диагностика Build Settings — показывает все сцены в билде.
/// Вызывается через меню Tools > Diagnostics > Show Build Settings
/// </summary>
public static class BuildSettingsDiagnostics
{
    [MenuItem("Tools/Diagnostics/Show Build Settings")]
    public static void ShowBuildSettings()
    {
        int sceneCount = EditorBuildSettings.scenes.Length;
        Debug.Log($"[BuildSettingsDiagnostics] Всего сцен в Build Settings: {sceneCount}");
        
        for (int i = 0; i < sceneCount; i++)
        {
            var scene = EditorBuildSettings.scenes[i];
            Debug.Log($"  [{i}] {scene.path}");
        }
    }

    [MenuItem("Tools/Diagnostics/Check StartMenu")]
    public static void CheckStartMenu()
    {
        bool found = false;
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            var scene = EditorBuildSettings.scenes[i];
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
            
            if (sceneName == "StartMenu")
            {
                found = true;
                Debug.Log($"✅ StartMenu НАЙДЕНА в Build Settings (index {i}): {scene.path}");
                break;
            }
        }
        
        if (!found)
        {
            Debug.LogError("❌ StartMenu НЕ НАЙДЕНА в Build Settings!");
            Debug.LogError("Добавьте StartMenu.unity через File > Build Settings > Add Open Scenes или Add Scene...");
        }
    }

    [MenuItem("Tools/Diagnostics/Check All Level Scenes")]
    public static void CheckAllLevelScenes()
    {
        int totalScenes = 0;
        int missingScenes = 0;
        
        Debug.Log("[BuildSettingsDiagnostics] === ПРОВЕРКА СЦЕН УРОВНЕЙ ===");
        
        // Проверяем Morning и Evening сцены для всех уровней
        for (int level = 1; level <= 36; level++)
        {
            int scenesPerPart = (level == 1 || level == 19) ? 3 : 2;
            
            for (int part = 1; part <= scenesPerPart; part++)
            {
                string morningSceneName = $"Morning {level}.{part}";
                string eveningSceneName = $"Evening {level}.{part}";
                
                bool morningFound = CheckSceneExists(morningSceneName);
                bool eveningFound = CheckSceneExists(eveningSceneName);
                
                totalScenes += 2;
                if (!morningFound) missingScenes++;
                if (!eveningFound) missingScenes++;
            }
        }
        
        Debug.Log($"[BuildSettingsDiagnostics] Итого проверено: {totalScenes} сцен");
        Debug.Log($"[BuildSettingsDiagnostics] Пропущено: {missingScenes} сцен");
    }

    static bool CheckSceneExists(string sceneName)
    {
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            var scene = EditorBuildSettings.scenes[i];
            string pathName = System.IO.Path.GetFileNameWithoutExtension(scene.path);
            
            if (pathName == sceneName)
            {
                Debug.Log($"  ✅ {sceneName} — НАЙДЕНА (index {i})");
                return true;
            }
        }
        
        Debug.LogError($"  ❌ {sceneName} — НЕ НАЙДЕНА!");
        return false;
    }
}
