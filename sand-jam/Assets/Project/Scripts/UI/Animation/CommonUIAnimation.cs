using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommonUIAnimation 
{
    private static Dictionary<RectTransform, Sequence> _activeBreathingAnimations = new Dictionary<RectTransform, Sequence>();

    public static void BreathingAnimation(RectTransform rectTransform, Vector2 startScale, Vector2 endScale, float duration = 0.5f)
    {
        // Dừng animation cũ nếu đang tồn tại
        StopBreathingAnimation(rectTransform);

        Vector3 start = new Vector3(startScale.x, startScale.y, 1f);
        Vector3 end = new Vector3(endScale.x, endScale.y, 1f);

        rectTransform.localScale = start;

        Sequence breathingSequence = DOTween.Sequence()
            .Append(rectTransform.DOScale(end, duration).SetEase(Ease.InOutSine))
            .Append(rectTransform.DOScale(start, duration).SetEase(Ease.InOutSine))
            .SetLoops(-1, LoopType.Restart);

        // Lưu lại reference animation
        _activeBreathingAnimations[rectTransform] = breathingSequence;
    }

    public static void StopBreathingAnimation(RectTransform rectTransform)
    {
        if (_activeBreathingAnimations.TryGetValue(rectTransform, out var sequence))
        {
            sequence?.Kill();
            _activeBreathingAnimations.Remove(rectTransform);

            // Reset về scale mặc định
            rectTransform.localScale = Vector3.one;
        }
    }

    public static void StopAllBreathingAnimations()
    {
        foreach (var kvp in _activeBreathingAnimations)
        {
            kvp.Value?.Kill();
            kvp.Key.localScale = Vector3.one;
        }
        _activeBreathingAnimations.Clear();
    }

    public static void ScaleTo(RectTransform rectTransform, Vector2 startScale, Vector2 endScale, float duration = 0.5f, Action onComplete = null)
    {
        rectTransform.localScale = startScale;
        rectTransform.DOScale(endScale, duration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => onComplete?.Invoke());
    }

    public static void OpenPopUp(RectTransform rectTransform, Vector2 startScale, Vector2 maxScale, Vector2 endScale, float duration = 0.5f, Action action = null)
    {
        rectTransform.gameObject.SetActive(true);
        rectTransform.localScale = startScale;

        Sequence openSequence = DOTween.Sequence();

        openSequence.Append(rectTransform.DOScale(maxScale, duration * 0.6f)
                   .SetEase(Ease.OutQuad));

        openSequence.Append(rectTransform.DOScale(endScale, duration * 0.4f)
                   .SetEase(Ease.OutBack))
                   .OnComplete(() => action?.Invoke());
    }

    public static void ClosePopUp(RectTransform rectTransform, Vector2 startScale, Vector2 maxScale, Vector2 endScale, float duration = 0.5f, Action action = null)
    {
        rectTransform.localScale = startScale;

        Sequence closeSequence = DOTween.Sequence();

        closeSequence.Append(rectTransform.DOScale(maxScale, duration * 0.4f)
                    .SetEase(Ease.OutQuad));

        closeSequence.Append(rectTransform.DOScale(endScale, duration * 0.6f))
                    .SetEase(Ease.Linear)
                    .OnComplete(() => {
                        rectTransform.gameObject.SetActive(false);
                        action?.Invoke();
                    });
    }

    public static void OpenThenClosePopUp(RectTransform rectTransform, Vector2 startScale, Vector2 endScale, float duration = 0.5f, Action action = null)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => rectTransform.gameObject.SetActive(true));
        sequence.Append(rectTransform.DOScale(endScale, duration).SetEase(Ease.OutBack));
        sequence.AppendInterval(2f);
        sequence.Append(rectTransform.DOScale(startScale, duration).SetEase(Ease.InBack));
        sequence.AppendCallback(() => {
            rectTransform.gameObject.SetActive(false);
            action?.Invoke();
        });
    }

    public static void MoveThroughTargets(RectTransform mover, List<RectTransform> targets, float moveDuration = 0.5f, Action onComplete = null)
    {
        if (targets == null || targets.Count == 0 || mover == null)
            return;

        Transform moverParent = mover.parent;
        Sequence moveSequence = DOTween.Sequence();

        foreach (RectTransform target in targets)
        {
            Vector3 worldPos = target.position;
            Vector3 localPos = moverParent.InverseTransformPoint(worldPos);

            moveSequence.Append(mover.DOLocalMove(localPos, moveDuration)).SetEase(Ease.InOutSine);
        }

        moveSequence.OnComplete(() => onComplete?.Invoke());
    }

    public static void ScaleUpThenDown(RectTransform rectTransform, Vector2 scaleTo, float scaleDuration = 0.3f, Action onComplete = null)
    {
        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = new Vector3(scaleTo.x, scaleTo.y, originalScale.z);

        Sequence scaleSequence = DOTween.Sequence()
            .Append(rectTransform.DOScale(targetScale, scaleDuration).SetEase(Ease.OutBack))
            .Append(rectTransform.DOScale(originalScale, scaleDuration).SetEase(Ease.InOutSine)
            .OnComplete(() => onComplete?.Invoke()));
    }

    // Hủy tất cả animation đang chạy trên RectTransform
    public static void StopAllAnimations(RectTransform rectTransform)
    {
        rectTransform.DOKill();
    }
}