using UnityEngine;

/// <summary>
/// Профиль сборки сцены. Сохраняется как .asset файл рядом с сценой.
/// Включённые сцены автоматически добавляются в Build Settings при запуске редактора.
/// </summary>
[CreateAssetMenu(fileName = "SceneBuildProfile", menuName = "HourPeak/Scene Build Profile")]
public class SceneBuildProfile : ScriptableObject
{
    /// <summary>Порядковый номер в билде</summary>
    public int buildOrder;

    /// <summary>Включена ли сцена в билд</summary>
    public bool enabled = true;

    /// <summary>Пути к зависимым сценам (preloading)</summary>
    public string[] dependencies = new string[0];
}
