using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ResourceBar : MonoBehaviour
{
    [SerializeField] private RectTransform barMask;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image shadowGlow;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 0.3f;

    private readonly float maxBarHeight = 110f;

    public void SetValue(float percent, bool instant = false)
    {
        float clamped = Mathf.Clamp01(percent);
        float targetHeight = clamped * maxBarHeight;

        // Stop any current animation to prevent jitter
        DOTween.Kill(barMask);

        if (instant)
        {
            barMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
        }
        else
        {
            // Smoothly change the height of the window (the mask)
            DOTween.To(() => barMask.rect.height,
                       x => barMask.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, x),
                       targetHeight, duration)
                   .SetEase(Ease.OutQuad);
        }
    }

    public void SetVisibility(bool visible, float duration = 0.2f)
    {
        if (canvasGroup == null) return;

        float targetAlpha = visible ? 1f : 0f;

        if (duration == 0)
        {
            canvasGroup.alpha = targetAlpha;
        }
        else
        {
            // Smoothly fade
            canvasGroup.DOFade(targetAlpha, duration);
        }
    }
}