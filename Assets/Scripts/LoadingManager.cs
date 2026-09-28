using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    private static LoadingManager instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float minLoadingTime = 1.0f; // 最低ロード表示時間（一瞬で終わるのを防ぐ）
    [SerializeField] private float barFillSpeed = 2.0f;   // プログレスバーの補間速度

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            loadingCanvas.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 外部から呼び出すロード開始メソッド
    /// </summary>
    /// <param name="targetSceneName">遷移先のシーン名</param>
    public static void LoadScene(string targetSceneName)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.LoadSceneRoutine(targetSceneName));
        }
        else
        {
            Debug.LogError("LoadingManager インスタンスが存在しません。");
        }
    }

    private IEnumerator LoadSceneRoutine(string targetSceneName)
    {
        loadingCanvas.SetActive(true);
        canvasGroup.alpha = 0f;

        // 1. フェードイン (UIを表示)
        yield return StartCoroutine(FadeRoutine(1f));

        // 2. メモリ解放処理
        yield return System.GC.Collect();
        yield return Resources.UnloadUnusedAssets();

        // 3. 非同期読み込み開始
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false; // 90%でロード停止させて演出待機させる

        float displayedProgress = 0f;
        float timer = 0f;

        while (!asyncLoad.isDone)
        {
            timer += Time.deltaTime;

            // 0.0 ~ 0.9 の値を 0.0 ~ 1.0 に換算
            float targetProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // バーの表示をなめらかに補間
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, Time.deltaTime * barFillSpeed);

            if (progressBar != null) progressBar.value = displayedProgress;
            if (progressText != null) progressText.text = $"{(displayedProgress * 100f):F0}%";

            // ロードが実質完了(90%到達) かつ 最低演出時間を満たしたらシーン切り替え
            if (asyncLoad.progress >= 0.9f && displayedProgress >= 0.99f && timer >= minLoadingTime)
            {
                if (progressBar != null) progressBar.value = 1f;
                if (progressText != null) progressText.text = "100%";

                // シーンのアクティベートを許可
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        // 4. フェードアウト (UIを非表示)
        yield return StartCoroutine(FadeRoutine(0f));
        loadingCanvas.SetActive(false);
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}