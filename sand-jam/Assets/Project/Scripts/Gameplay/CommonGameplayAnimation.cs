
using DG.Tweening;
using System;
using UnityEngine;

public static class CommonGameplayAnimation
{
    //public static void MoveCurvedWithScale(BlockElement block, Transform holder, Vector3 endLocalPos, float endScale, float maxScale, float duration, Action onComplete = null)
    //{
    //    duration *= 0.35f;

    //    // Tạo callback để xử lý tuần tự
    //    Action<BlockElement> moveAction = (b) =>
    //    {
    //        b.TurnOnOffCastShadow(false);
    //        b.transform.SetParent(holder);

    //        Vector3 startLocalPos = b.transform.localPosition;
    //        Vector3 startScale = b.transform.localScale;
    //        Vector3 midPoint = (startLocalPos + endLocalPos) / 2f;
    //        midPoint.z = Const.MAX_ANIM_HEIGHT;

    //        DOVirtual.Float(0f, 1f, duration, angle =>
    //        {
    //            Vector3 m1 = Vector3.Lerp(startLocalPos, midPoint, angle);
    //            Vector3 m2 = Vector3.Lerp(midPoint, endLocalPos, angle);
    //            b.transform.localPosition = Vector3.Lerp(m1, m2, angle);

    //            float t = 0.5f - Mathf.Abs(0.5f - angle);
    //            float currentScale = angle <= 0.5f
    //                ? Mathf.Lerp(1f, maxScale, angle * 2f)
    //                : Mathf.Lerp(maxScale, endScale, (angle - 0.5f) * 2f);

    //            b.transform.localScale = Vector3.one * currentScale;
    //            b.Shape.Pivot.eulerAngles = Static.InverseLerp(0.5f, 1f, angle) * 360f * Vector3.forward;
    //        })
    //        .SetEase(Ease.OutQuad)
    //        .OnComplete(() =>
    //        {
    //            b.transform.localScale = Vector3.one * endScale;
    //            b.TurnOnOffCastShadow(false);
    //            onComplete?.Invoke();
    //        });
    //    };

    //    // Thêm vào sequence theo 1 trong 2 cách:

    //    // CÁCH 1: Sử dụng callback tuần tự
    //    if (block.CurrentCurvedSequence == null || !block.CurrentCurvedSequence.IsActive())
    //    {
    //        block.CurrentCurvedSequence = DOTween.Sequence();
    //        block.CurrentCurvedSequence.AppendCallback(() => moveAction(block));
    //    }
    //    else
    //    {
    //        block.CurrentCurvedSequence.AppendCallback(() => moveAction(block));
    //    }

    //}

    public static Sequence CreateShakeEffect(Transform target, float duration = 0.5f, float strength = 0.4f, int vibrato = 20, float randomness = 90f)
    {
        Vector3 originalPosition = target.localPosition;
        Sequence shakeSequence = DOTween.Sequence();

        shakeSequence.Append(target.DOShakePosition(duration, strength, vibrato, randomness, false, true));

        // Có thể thêm shake rotation nếu muốn
        // shakeSequence.Join(target.DOShakeRotation(duration, strength * 10, vibrato, randomness));

        shakeSequence.OnComplete(() =>
        {
            target.localPosition = originalPosition;
            // target.localRotation = Quaternion.identity; 
        });

        return shakeSequence;
    }

    public static Tween ScaleTo(Transform transform, Vector3 targetScale, float duration, Ease ease = Ease.Linear)
    {
        Tween tween = transform.transform.DOScale(targetScale, duration).SetEase(ease);
        return tween;
    }

    public static Tween ScaleToZeroBouncy(Transform transform, Vector3 maxScale, float duration, Action onFinish = null, Ease ease = Ease.Linear)
    {
        Tween tween = transform.DOScale(maxScale, duration / 2)
        .SetEase(ease)
        .OnComplete(() =>
        {
            transform.DOScale(Vector3.zero, duration / 2).SetEase(ease).OnComplete(() => onFinish?.Invoke());
        });

        return tween;
    }

    public static Tween ScaleToOneBouncy(Transform transform, Vector3 maxScale, float duration, Action onFinish = null, Ease ease = Ease.Linear)
    {
        Tween tween = transform.DOScale(maxScale, duration / 2)
        .SetEase(ease)
        .OnComplete(() =>
        {
            transform.DOScale(Vector3.one, duration / 2).SetEase(ease).OnComplete(() => onFinish?.Invoke());
        });

        return tween;
    }

    public static Tween BlinkingEffect(SpriteRenderer spriteRenderer, Color startColor, Color endColor, float duration)
    {
        spriteRenderer.color = startColor;

        return spriteRenderer.DOColor(endColor, duration / 2f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                spriteRenderer.DOColor(startColor, duration / 2f).SetEase(Ease.Linear);
            });
    }

    public static Tween LoopBlinkingEffect(SpriteRenderer spriteRenderer, Color startColor, Color endColor, float duration)
    {
        spriteRenderer.color = startColor;

        return spriteRenderer.DOColor(endColor, duration / 2f)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public static Sequence CreateBounceAnimation(Transform target, Vector3 originalPoint, float duration = 0.5f, float bounceStrength = 0.3f)
    {
        Vector3 originalScale = target.localScale;

        target.localPosition = originalPoint;

        Sequence bounceSequence = DOTween.Sequence();

        bounceSequence.Append(
            target.DOLocalMoveY(originalPoint.y - bounceStrength, duration * 0.3f)
            .SetEase(Ease.OutQuad)
        );

        bounceSequence.Append(
            target.DOLocalMoveY(originalPoint.y, duration * 0.7f)
            .SetEase(Ease.OutBounce)
        );

        bounceSequence.OnComplete(() =>
        {
            target.localPosition = originalPoint;
            target.localScale = originalScale;
        });

        return bounceSequence;
    }

    public static Tween MoveTo(Transform target, Transform destination, float duration, Action onComplete = null, Ease ease = Ease.Linear)
    {
        return target.DOMove(destination.position, duration).SetEase(ease).OnComplete(() => onComplete?.Invoke());
    }

    public static Tween MoveTo(Transform target, Vector3 destination, float duration, Action onComplete = null, Ease ease = Ease.Linear)
    {
        return target.DOMove(destination, duration).SetEase(ease).OnComplete(() => onComplete?.Invoke());
    }

    public static Tween MoveTo(Transform target, Func<Vector3> getDestination, float duration, Action onComplete = null, Ease ease = Ease.Linear)
    {
        return DOTween.To(
            () => target.position,
            x => target.position = x,
            getDestination(),
            duration)
            .SetEase(ease)
            .OnComplete(() => onComplete?.Invoke());
    }

    public static Tween MoveToLocal(Transform target, Vector3 destination, float duration)
    {
        return target.DOLocalMove(destination, duration).SetEase(Ease.Linear);
    }

    public static Tween MoveFromTo(Transform target, Vector3 startPoint, Vector3 endPoint, float duration,
                                    Ease easeType = Ease.Linear, Action onComplete = null)
    {
        // Đặt vị trí ban đầu
        target.position = startPoint;

        // Tạo tween di chuyển
        return target.DOMove(endPoint, duration)
            .SetEase(easeType)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
            })
            // Tự động hủy nếu object bị destroy
            .SetLink(target.gameObject);
    }
}
