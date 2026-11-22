using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DownloadCertificate : MonoBehaviour
{
    #region Configuration

    [Header("Настройки сцены")]
    [Tooltip("Имя сцены для перехода после сохранения сертификата")]
    [SerializeField] private string startMenuSceneName = "StartMenu";
    
    [Header("Настройки файла")]
    [Tooltip("Имя файла сертификата")]
    [SerializeField] private string certificateFileName = "Сертификат успеха.png";
    
    [Header("Настройки камеры")]
    [Tooltip("Камера, с которой делать скриншот сертификата")]
    [SerializeField] private Camera certificateCamera;

    [Header("Ссылка на кнопку")]
    [Tooltip("После скачивания кнопка исчезает")]
    [SerializeField] private GameObject downloadButton;

    #endregion

    #region Download Logic

    public void DownloadAndReturnToMenu()
    {
        Time.timeScale = 1f;

        Debug.Log("📜 Подготовка сертификата...");

        bool captureSuccess = CaptureCertificateScreenshot(out string savedPath);

        if (captureSuccess && !string.IsNullOrEmpty(savedPath))
        {
            Debug.Log("✅ Сертификат захвачен. Сохранение на устройство...");

            bool saveSuccess = savedPath != null && File.Exists(savedPath);

            if (saveSuccess)
            {
                Debug.Log("✅ Сертификат сохранён. Переход в меню...");
                SceneManager.LoadScene(startMenuSceneName);
            }
            else
            {
                Debug.LogError("❌ Ошибка сохранения сертификата на устройство.");
                SceneManager.LoadScene(startMenuSceneName);
            }
        }
        else
        {
            Debug.LogError("❌ Ошибка захвата сертификата.");
            SceneManager.LoadScene(startMenuSceneName);
        }
    }

    private bool CaptureCertificateScreenshot(out string savedPath)
    {
        savedPath = null;
        downloadButton.SetActive(false);

        if (certificateCamera == null)
        {
            Debug.LogError("❌ Камера не назначена!");
            return false;
        }

        try
        {
            int imageWidth = certificateCamera.pixelWidth;
            int imageHeight = certificateCamera.pixelHeight;
            RenderTexture rt = new RenderTexture(imageWidth, imageHeight, 24);
            certificateCamera.targetTexture = rt;
            certificateCamera.Render();
            RenderTexture.active = rt;
            Texture2D texture = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
            texture.Apply();
            certificateCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            byte[] bytes = texture.EncodeToPNG();
            string baseName = certificateFileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) 
                ? certificateFileName.Substring(0, certificateFileName.Length - 4) 
                : certificateFileName;
            string fileName = $"{baseName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            savedPath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(savedPath, bytes);
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

    #endregion
}