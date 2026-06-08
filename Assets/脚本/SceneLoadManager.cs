using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 场景切换管理：淡入淡出过渡动画
/// </summary>
public class SceneLoadManager : MonoBehaviour
{
    public string loadSceneName;
    public float fadeDuration = 0.5f;

    private static GameObject _fadeCanvas;
    private static CanvasGroup _fadeGroup;
    private static bool _isFading;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.transform.CompareTag("Player") && !_isFading)
        {
            StartCoroutine(FadeAndLoadScene());
        }
    }

    private IEnumerator FadeAndLoadScene()
    {
        _isFading = true;

        // 确保有淡入淡出画布
        EnsureFadeCanvas();

        // 淡出（黑屏渐入）
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _fadeGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        _fadeGroup.alpha = 1f;

        // 加载新场景
        SceneManager.LoadScene(loadSceneName);

        // 等一帧让场景加载完成
        yield return null;

        // 淡入（黑屏渐出）
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            _fadeGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        _fadeGroup.alpha = 0f;

        _isFading = false;
    }

    private void EnsureFadeCanvas()
    {
        if (_fadeCanvas != null) return;

        // 创建全屏黑幕
        _fadeCanvas = new GameObject("FadeOverlay");
        DontDestroyOnLoad(_fadeCanvas);

        var canvas = _fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        _fadeGroup = _fadeCanvas.AddComponent<CanvasGroup>();
        _fadeGroup.blocksRaycasts = true;

        // 黑色图片
        var imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(_fadeCanvas.transform, false);
        var img = imageObj.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.black;

        // 撑满屏幕
        var rectTransform = img.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;

        _fadeGroup.alpha = 0f;
    }
}
