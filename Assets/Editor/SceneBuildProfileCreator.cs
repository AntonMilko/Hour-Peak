using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Создаёт SceneBuildProfile для всех сцен в проекте автоматически.
/// Порядок в билде определяется порядком в папке (по имени файла).
/// </summary>
public static class SceneBuildProfileCreator
{
    [MenuItem("Tools/Scene Profiles/Create For All Scenes")]
    public static void CreateForAllScenes()
    {
        string[] allSceneGuids = AssetDatabase.FindAssets("t:UnityEditor.SceneAsset");
        
        int created = 0;
        int skipped = 0;
        int errors = 0;

        foreach (var guid in allSceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            
            // Пропускаем сцены вне папки Scenes
            if (!scenePath.StartsWith("Assets/Scenes/"))
                continue;

            string assetPath = scenePath + ".asset";
            
            // Проверяем, не существует ли уже профиль
            if (AssetDatabase.LoadAssetAtPath<SceneBuildProfile>(assetPath) != null)
            {
                skipped++;
                continue;
            }

            var profile = ScriptableObject.CreateInstance<SceneBuildProfile>();
            profile.buildOrder = 0; // Будет исправлено ниже
            profile.enabled = true;
            
            try
            {
                AssetDatabase.CreateAsset(profile, assetPath);
                created++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SceneBuildProfileCreator] Ошибка создания для {scenePath}: {e.Message}");
                errors++;
            }
        }

        // Присваиваем порядковые номера
        AssignBuildOrders();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[SceneBuildProfileCreator] Создано: {created}, Пропущено: {skipped}, Ошибки: {errors}");
    }

    [MenuItem("Tools/Scene Profiles/Reassign Build Orders")]
    public static void AssignBuildOrders()
    {
        var profiles = SceneBuildProfileAutoApply.FindAllProfiles();
        
        // Сортируем по имени файла (естественный порядок)
        profiles.Sort((a, b) => 
        {
            // Извлекаем номер уровня из имени файла
            int numA = ExtractLevelNumber(a.name);
            int numB = ExtractLevelNumber(b.name);
            return numA.CompareTo(numB);
        });

        for (int i = 0; i < profiles.Count; i++)
        {
            profiles[i].buildOrder = i;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[SceneBuildProfileCreator] Переназначено {profiles.Count} порядков.");
    }

    [MenuItem("Tools/Scene Profiles/Delete All")]
    public static void DeleteAllProfiles()
    {
        var profiles = SceneBuildProfileAutoApply.FindAllProfiles();
        int deleted = 0;

        foreach (var profile in profiles)
        {
            string assetPath = AssetDatabase.GetAssetPath(profile);
            AssetDatabase.DeleteAsset(assetPath);
            deleted++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SceneBuildProfileCreator] Удалено {deleted} профилей.");
    }

    static int ExtractLevelNumber(string fileName)
    {
        // Пытаемся извлечь число из имени файла
        // Morning 1.1.unity → 1
        // Evening 1.3.unity → 1
        // StartMenu.unity → 0
        // Game completion certificate.unity → 999
        
        if (fileName.Contains("StartMenu")) return 0;
        if (fileName.Contains("completion")) return 999;
        
        // Ищем первое число в имени
        string prefix = fileName.Split('.')[0];
        foreach (char c in prefix)
        {
            if (char.IsDigit(c))
            {
                string numStr = "";
                foreach (char c2 in prefix)
                {
                    if (char.IsDigit(c2)) numStr += c2;
                    else break;
                }
                int.TryParse(numStr, out int num);
                return num;
            }
        }
        
        return 500; // Среднее значение для неизвестных
    }
}
