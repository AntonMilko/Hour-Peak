using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Контроллер скачивания сертификата.
/// Захватывает изображение с камеры, сохраняет в PNG на устройство и переходит в главное меню.
/// </summary>
public class DownloadCertificate : MonoBehaviour
{
    [Header("Настройки сцены")]
    [Tooltip("Имя сцены для перехода после сохранения сертификата")]
    [SerializeField] private string startMenuSceneName = "StartMenu";
    
    [Header("Настройки файла")]
    [Tooltip("Имя файла сертификата")]
    [SerializeField] private string certificateFileName = "HourPeak_Certificate.png";
    
    [Header("Настройки камеры")]
    [Tooltip("Камера, с которой делать скриншот сертификата")]
    [SerializeField] private Camera certificateCamera;
    
    [Header("Параметры изображения")]
    [Tooltip("Ширина захвата изображения (пиксели)")]
    [SerializeField] private int imageWidth = 1920;
    [Tooltip("Высота захвата изображения (пиксели)")]
    [SerializeField] private int imageHeight = 1080;

    private void Start()
    {
        // Если камера не назначена, используем Main Camera
        if (certificateCamera == null)
        {
            certificateCamera = Camera.main;
        }
    }

    /// <summary>
    /// Вызывается при нажатии кнопки "Скачать сертификат"
    /// </summary>
    public void OnDownloadButtonClick()
    {
        StartCoroutine(DownloadAndReturnToMenu());
    }

    private IEnumerator DownloadAndReturnToMenu()
    {
        Debug.Log("📜 Подготовка сертификата...");

        // 1. Захватываем скриншот сертификата с камеры
        bool captureSuccess = CaptureCertificateScreenshot(out string savedPath);

        if (captureSuccess && !string.IsNullOrEmpty(savedPath))
        {
            Debug.Log("✅ Сертификат захвачен. Сохранение на устройство...");

            // 2. Сохраняем файл на устройство (в папку Downloads на Android)
            bool saveSuccess = SaveToDownloads(savedPath);

            if (saveSuccess)
            {
                Debug.Log("✅ Сертификат сохранён. Переход в меню...");
                
                // Небольшая задержка для завершения операции записи
                yield return new WaitForSeconds(0.3f);

                // 3. Переходим в сцену StartMenu
                SceneManager.LoadScene(startMenuSceneName);
            }
            else
            {
                Debug.LogError("❌ Ошибка сохранения сертификата на устройство.");
                yield return new WaitForSeconds(2f);
                SceneManager.LoadScene(startMenuSceneName);
            }
        }
        else
        {
            Debug.LogError("❌ Ошибка захвата сертификата.");
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(startMenuSceneName);
        }
    }

    /// <summary>
    /// Захватывает изображение с камеры сертификата и сохраняет во временный файл
    /// </summary>
    /// <param name="savedPath">Путь к сохранённому файлу</param>
    /// <returns>True, если захват успешен</returns>
    private bool CaptureCertificateScreenshot(out string savedPath)
    {
        savedPath = null;

        if (certificateCamera == null)
        {
            Debug.LogError("❌ Камера не назначена!");
            return false;
        }

        try
        {
            // Создаём RenderTexture нужного размера
            RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
            certificateCamera.targetTexture = rt;

            // Рендерим кадр
            certificateCamera.Render();

            // Делаем RenderTexture активным для чтения
            RenderTexture.active = rt;

            // Создаём Texture2D из RenderTexture
            Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            texture.Apply();

            // Сбрасываем target и делаем RenderTexture неактивным
            certificateCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            // Кодируем текстуру в PNG
            byte[] bytes = texture.EncodeToPNG();

            // Формируем имя файла с датой
            string baseName = certificateFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) 
                ? certificateFileName.Substring(0, certificateFileName.Length - 4) 
                : certificateFileName;
            string fileName = $"{baseName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            savedPath = Path.Combine(Application.persistentDataPath, fileName);
            
            // Сохраняем файл
            File.WriteAllBytes(savedPath, bytes);

            // Освобождаем память
            Destroy(texture);
            bytes = null;

            Debug.Log($"💾 Сертификат сохранён: {savedPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Ошибка захвата изображения: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Копирует файл из временной папки в Downloads (Android) или возвращает путь (другие платформы)
    /// </summary>
    /// <param name="tempFilePath">Путь к временному файлу</param>
    /// <returns>True, если сохранение успешно</returns>
    private bool SaveToDownloads(string tempFilePath)
    {
        try
        {
#if UNITY_ANDROID
            // Android: копируем файл в публичную папку Downloads
            string fileName = Path.GetFileName(tempFilePath);
            string downloadsPath = Path.Combine(GetExternalFilesDir(), "Downloads", fileName);
            
            // Создаём директорию Downloads, если не существует
            string downloadsDir = Path.GetDirectoryName(downloadsPath);
            if (!Directory.Exists(downloadsDir))
            {
                Directory.CreateDirectory(downloadsDir);
            }

            // Копируем файл
            File.Copy(tempFilePath, downloadsPath, true);
            
            // Проверяем, что файл существует
            bool exists = File.Exists(downloadsPath);
            Debug.Log($"📥 Android: {(!exists ? "❌ Не удалось скопировать" : "✅ Скопировано в Downloads")}");
            return exists;
#elif UNITY_IOS
            // iOS: файл сохраняется в persistentDataPath
            // Для iOS нет прямой возможности показать "Поделиться" без Native Plugin
            Debug.Log("📱 iOS: Сертификат сохранён в папку приложения");
            return File.Exists(tempFilePath);
#else
            // Desktop (Windows/Mac): файл уже в persistentDataPath
            Debug.Log("💻 Desktop: Сертификат сохранён");
            return File.Exists(tempFilePath);
#endif
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Ошибка копирования файла: {e.Message}");
            return false;
        }
    }

#if UNITY_ANDROID
    /// <summary>
    /// Получает путь к External Files Dir через AndroidJava
    /// </summary>
    private string GetExternalFilesDir()
    {
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            {
                AndroidJavaObject filesDir = context.Call<AndroidJavaObject>("getExternalFilesDir", (string)null);
                return filesDir?.Call<string>("getPath");
            }
        }
        catch
        {
            return Application.persistentDataPath;
        }
    }
#endif
}