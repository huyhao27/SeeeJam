using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // để chuyển scene
using DG.Tweening;
using TMPro; // dùng DOTween

public class CutSceneView : MonoBehaviour
{
    public Image[] cutscenes;
    public TextMeshProUGUI[] texts;
    public float fadeDuration = 1f;
    public float displayDuration = 5f;
    public string nextSceneName; // tên scene sẽ load sau cutscene

    private int currentIndex = -1;

    void Start()
    {
        // Tắt hết cutscenes lúc đầu
        foreach (var img in cutscenes)
        {
            if (img != null)
            {
                Color c = img.color;
                c.a = 0;
                img.color = c;
            }
        }

        // Bắt đầu slideshow
        StartCoroutine(PlayCutScenes());
    }

    private IEnumerator PlayCutScenes()
    {
        for (int i = 0; i < cutscenes.Length; i++)
        {
            // Tắt cái trước (nếu có)
            if (currentIndex >= 0 && currentIndex < cutscenes.Length)
            {
                var prevImg = cutscenes[currentIndex];
                prevImg.DOFade(0, fadeDuration).OnComplete(() =>
                {
                    if (prevImg != null && prevImg.transform.parent != null)
                    {
                        prevImg.transform.parent.gameObject.SetActive(false); // ẩn parent
                    }
                });

                // Nếu có text tương ứng, fade-out luôn
                if (texts != null && currentIndex < texts.Length && texts[currentIndex] != null)
                {
                    texts[currentIndex].DOFade(0, fadeDuration);
                }
            }

            currentIndex = i;
            var currentImg = cutscenes[currentIndex];

            // Bật lại parent trước khi fade-in
            if (currentImg.transform.parent != null)
            {
                currentImg.transform.parent.gameObject.SetActive(true);
            }

            // Reset alpha = 0 trước khi fade-in
            currentImg.color = new Color(currentImg.color.r, currentImg.color.g, currentImg.color.b, 0);

            // Fade-in cutscene mới
            currentImg.DOFade(1, fadeDuration);

            // Fade-in text nếu có
            if (texts != null && currentIndex < texts.Length && texts[currentIndex] != null)
            {
                texts[currentIndex].color = new Color(texts[currentIndex].color.r, texts[currentIndex].color.g, texts[currentIndex].color.b, 0);
                texts[currentIndex].DOFade(1, fadeDuration);
            }

            // Chờ thời gian hiển thị
            yield return new WaitForSeconds(displayDuration);
        }

        // Sau khi hết cutscenes, fade-out cái cuối cùng
        var lastImg = cutscenes[currentIndex];
        var lastText = (texts != null && currentIndex < texts.Length) ? texts[currentIndex] : null;

        lastImg.DOFade(0, fadeDuration).OnComplete(() =>
        {
            if (lastImg != null && lastImg.transform.parent != null)
                lastImg.transform.parent.gameObject.SetActive(false);
        });

        if (lastText != null)
            lastText.DOFade(0, fadeDuration);

        // Chờ 3s trước khi load scene mới
        yield return new WaitForSeconds(3f);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            #if UNITY_EDITOR
            #endif
            SceneManager.LoadScene(nextSceneName);
        }
    }
}