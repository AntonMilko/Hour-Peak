using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingScreen;          // Панель экрана загрузки (фон)
    public TextMeshProUGUI loadingText;        // Текст: сюда мы впишем «Загрузка»
    public Image fadeImage;                    // Чёрный Image для затемнения экрана

    [Header("Settings")]
    public string defaultText = "Загрузка";    // Текст, который будет всегда
    public float fadeDuration = 1f;            // Длительность затемнения/осветления

    private Coroutine currentLoad;

    // Публичный метод, который вызывает LevelMenuManager
    public void LoadLevel(string sceneName)
    {
        if (currentLoad != null)
            StopCoroutine(currentLoad);

        currentLoad = StartCoroutine(LoadWithFadeAndText(sceneName));
    }

    private IEnumerator LoadWithFadeAndText(string sceneName)
    {
        // --- ШАГ 1: Подготовка и показ текста «Загрузка» ---
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        if (loadingText != null)
        {
            loadingText.text = defaultText;     // Гарантированно ставим «Загрузка»
            loadingText.gameObject.SetActive(true);
        }

        // Если вдруг у тебя Text не в loadingScreen, а отдельно — можно дополнительно активировать его родителя и т.п.
        
        // --- ШАГ 2: Затемнение экрана (Fade Out) ---
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = Color.black; // На всякий случай фиксируем чёрный цвет

        // --- ШАГ 3: Асинхронная загрузка сцены ---
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            // Можно обновлять прогресс-бар, если есть
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            if (op.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.1f);
                op.allowSceneActivation = true;
            }
            yield return null;
        }

        // --- ШАГ 4: Осветление экрана (Fade In) на новой сцене ---
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = 1 - Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.color = Color.clear;

        // --- ШАГ 5: Скрыть экран загрузки и текст ---
        if (loadingText != null)
            loadingText.gameObject.SetActive(false);

        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
}