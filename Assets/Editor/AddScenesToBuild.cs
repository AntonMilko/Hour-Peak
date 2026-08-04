using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class AddScenesToBuild : EditorWindow
{
    [MenuItem("Tools/Add All Scenes to Build Settings")]
    public static void AddAllScenes()
    {
        string scenesPath = "Assets/Scenes";
        
        if (!Directory.Exists(scenesPath))
        {
            EditorUtility.DisplayDialog("Ошибка", $"Папка {scenesPath} не найдена!", "OK");
            return;
        }
        
        string[] sceneFiles = Directory.GetFiles(scenesPath, "*.unity", SearchOption.AllDirectories);
        
        if (sceneFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("Ошибка", "Сцены не найдены!", "OK");
            return;
        }
        
        var existingScenes = EditorBuildSettings.scenes.ToList();
        var existingPaths = existingScenes.Select(s => s.path).ToHashSet();
        
        int addedCount = 0;
        int skippedCount = 0;
        
        foreach (string sceneFile in sceneFiles)
        {
            string fullPath = Path.GetFullPath(sceneFile);
            string assetsPath = Path.GetFullPath("Assets");
            string relativePath = fullPath.Substring(assetsPath.Length - "Assets".Length);
            relativePath = relativePath.Replace("\\", "/");
            
            if (existingPaths.Contains(relativePath))
            {
                skippedCount++;
                continue;
            }
            
            EditorBuildSettingsScene newScene = new EditorBuildSettingsScene(relativePath, true);
            var newScenes = existingScenes.ToList();
            newScenes.Add(newScene);
            EditorBuildSettings.scenes = newScenes.ToArray();
            addedCount++;
        }
        
        EditorUtility.DisplayDialog(
            "Готово!",
            $"Добавлено сцен: {addedCount}\nПропущено (уже есть): {skippedCount}\nВсего сцен в билде: {EditorBuildSettings.scenes.Length}",
            "OK"
        );
        
        Debug.Log($"[AddScenesToBuild] Добавлено {addedCount} сцен, пропущено {skippedCount}");
    }
}
