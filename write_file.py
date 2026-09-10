import os

content = r'''using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class AddAllScenesToBuild
{
    [MenuItem("Tools/Add All Scenes (NEW)")]
    public static void AddAllScenes()
    {
        string scenesPath = "Assets/Scenes";
        if (!Directory.Exists(scenesPath)) { EditorUtility.DisplayDialog("Error", "Folder not found", "OK"); return; }
        string[] sceneFiles = Directory.GetFiles(scenesPath, "*.unity", SearchOption.AllDirectories);
        if (sceneFiles.Length == 0) { EditorUtility.DisplayDialog("Error", "No scenes found", "OK"); return; }
        var existingScenes = EditorBuildSettings.scenes.ToList();
        var existingPaths = existingScenes.Select(s => s.path).ToHashSet();
        int addedCount = 0; int skippedCount = 0;
        foreach (string sceneFile in sceneFiles)
        {
            string fullPath = Path.GetFullPath(sceneFile);
            string assetsPath = Path.GetFullPath("Assets");
            string relativePath = fullPath.Substring(assetsPath.Length - "Assets".Length);
            relativePath = relativePath.Replace(System.IO.Path.DirectorySeparatorChar, '/');
            if (existingPaths.Contains(relativePath)) { skippedCount++; continue; }
            existingScenes.Add(new EditorBuildSettingsScene(relativePath, true));
            existingPaths.Add(relativePath);
            addedCount++;
        }
        EditorBuildSettings.scenes = existingScenes.ToArray();
        EditorUtility.DisplayDialog("Done!", "Added: " + addedCount + " Skipped: " + skippedCount + " Total: " + EditorBuildSettings.scenes.Length, "OK");
        Debug.Log("[AddAllScenesToBuild] Added " + addedCount + " scenes, skipped " + skippedCount);
    }
    [MenuItem("Tools/Show Build Settings")]
    public static void ShowBuildSettings()
    {
        Debug.Log("[Build Settings] Total scenes: " + EditorBuildSettings.scenes.Length);
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            var scene = EditorBuildSettings.scenes[i];
            Debug.Log("  [" + i + "] " + scene.path);
        }
    }
}
'''

with open('Assets/Editor/AddAllScenesToBuild.cs', 'w', encoding='utf-8') as f:
    f.write(content)

print('File written successfully')
