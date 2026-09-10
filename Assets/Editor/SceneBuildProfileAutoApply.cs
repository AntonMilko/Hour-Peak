using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Автоматически сканирует папку Assets/Scenes и подхватывает
/// SceneBuildProfile (.asset) файлы, привязанные к сценам.
/// На основе найденных профилей заполняет EditorBuildSettings.scenes.
/// 
/// Связка: рядом с .unity файлом должен лежать .asset файл с тем же именем.
/// Например:
///   Morning 1.1.unity
///   Morning 1.1.unity.asset  <-- SceneBuildProfile
/// 
/// Срабатывает при:
///   - Запуске Unity (OnInitializeOnLoadCallback)
///   - Сохранении любого ассета (OnPostprocessAllAssets)
///   - Ручном вызове через меню Tools/Apply Scene Build Profiles
/// </summary>
public static class SceneBuildProfileAutoApply
{
    private const string SCENES_FOLDER = "Assets/Scenes";

    // ВНИМАНИЕ: ApplyAllProfiles() НЕ вызывается автоматически при запуске,
    // потому что если профилей нет — он сотрёт все сцены из Build Settings.
    // Используйте меню Tools > Apply Scene Build Profiles вручную.

    /// <summary>
    /// Создаёт SceneBuildProfile для выбранной сцены в Project Window.
    /// Профиль сохраняется рядом с сценой, с тем же именем.
    /// </summary>
    [MenuItem("Assets/Create Scene Build Profile")]
    static void CreateProfileForSelectedScene()
    {
        var selections = Selection.assetGUIDs
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .Where(p => p.EndsWith(".unity"))
            .ToList();

        if (selections.Count == 0)
        {
            Debug.LogWarning("[SceneBuildProfileAutoApply] Выберите .unity файл в Project Window.");
            return;
        }

        foreach (var scenePath in selections)
        {
            string assetPath = scenePath + ".asset";
            if (AssetDatabase.LoadAssetAtPath<SceneBuildProfile>(assetPath) != null)
            {
                Debug.LogWarning($"[SceneBuildProfileAutoApply] Профиль уже существует: {assetPath}");
                continue;
            }

            var profile = ScriptableObject.CreateInstance<SceneBuildProfile>();
            profile.buildOrder = 0;
            profile.enabled = true;
            AssetDatabase.CreateAsset(profile, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SceneBuildProfileAutoApply] Создан профиль: {assetPath}");
        }

        ApplyAllProfiles();
    }

    /// <summary>
    /// Показывает все найденные профили в консоли.
    /// </summary>
    [MenuItem("Tools/Show Scene Build Profiles")]
    public static void ShowProfiles()
    {
        var profiles = FindAllProfiles();
        if (profiles.Count == 0)
        {
            Debug.Log("[SceneBuildProfileAutoApply] Профили не найдены. Создайте их через Assets > Create Scene Build Profile.");
            return;
        }

        var sorted = profiles.OrderBy(p => p.buildOrder).ToList();
        Debug.Log($"[SceneBuildProfileAutoApply] Найдено профилей: {sorted.Count}");
        foreach (var p in sorted)
        {
            Debug.Log($"  [{p.buildOrder}] {p.name}.unity — enabled: {p.enabled}, deps: {p.dependencies.Length}");
        }
    }

    [MenuItem("Tools/Apply Scene Build Profiles")]
    public static void ApplyAllProfiles()
    {
        if (!Directory.Exists(SCENES_FOLDER))
        {
            Debug.LogWarning($"[SceneBuildProfileAutoApply] Папка {SCENES_FOLDER} не найдена.");
            return;
        }

        var profiles = FindAllProfiles();
        
        // Защита: если профилей нет — НЕ трогаем Build Settings!
        if (profiles.Count == 0)
        {
            Debug.Log("[SceneBuildProfileAutoApply] Профили не найдены — Build Settings не изменены.");
            return;
        }

        var scenesList = new List<EditorBuildSettingsScene>();

        // Сортируем по buildOrder
        var sorted = profiles.OrderBy(p => p.buildOrder).ToList();

        foreach (var profile in sorted)
        {
            string sceneName = profile.name + ".unity";

            // Ищем сцену по имени через FindAssets
            string[] sceneGuids = AssetDatabase.FindAssets($"name:{sceneName}", new[] { SCENES_FOLDER });
            bool added = false;

            foreach (var guid in sceneGuids)
            {
                string actualPath = AssetDatabase.GUIDToAssetPath(guid);
                if (actualPath.EndsWith(sceneName, System.StringComparison.OrdinalIgnoreCase))
                {
                    scenesList.Add(new EditorBuildSettingsScene(actualPath, profile.enabled));
                    added = true;
                    break;
                }
            }

            if (!added)
                Debug.LogWarning($"[SceneBuildProfileAutoApply] Сцена не найдена: {sceneName}");

            // Добавляем зависимости
            foreach (var dep in profile.dependencies)
            {
                string[] depGuids = AssetDatabase.FindAssets($"name:{dep}", new[] { SCENES_FOLDER });
                foreach (var guid in depGuids)
                {
                    string depPath = AssetDatabase.GUIDToAssetPath(guid);
                    if (!scenesList.Any(s => s.path == depPath))
                        scenesList.Add(new EditorBuildSettingsScene(depPath, true));
                }
            }
        }

        EditorBuildSettings.scenes = scenesList.ToArray();
        Debug.Log($"[SceneBuildProfileAutoApply] Применено {sorted.Count} профилей. Всего сцен в билде: {scenesList.Count}");
    }

    /// <summary>
    /// Ищет все .asset файлы в папке Scenes, которые являются SceneBuildProfile.
    /// </summary>
    public static List<SceneBuildProfile> FindAllProfiles()
    {
        var profiles = new List<SceneBuildProfile>();
        string[] guids = AssetDatabase.FindAssets("t:SceneBuildProfile");

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/Scenes/"))
                continue;
                
            var profile = AssetDatabase.LoadAssetAtPath<SceneBuildProfile>(path);
            if (profile != null)
                profiles.Add(profile);
        }

        return profiles;
    }
}
