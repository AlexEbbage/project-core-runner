using System;
using DG.Tweening;
using UnityEngine;

public static class UiMotion
{
    public const float PanelEnterDuration = 0.22f;
    public const float PanelExitDuration = 0.16f;
    public const float PageTransitionDuration = 0.24f;
    public const float SelectionPulseDuration = 0.18f;
    public const float PanelEnterScale = 0.965f;
    public const float SelectionPulseScale = 1.06f;
    public const float BadgePulseScale = 1.12f;

    public static CanvasGroup EnsureCanvasGroup(GameObject root)
    {
        if (root == null)
            return null;

        if (!root.TryGetComponent(out CanvasGroup group))
            group = root.AddComponent<CanvasGroup>();

        return group;
    }

    public static Tween ShowPanel(GameObject panel, float duration = PanelEnterDuration, float startScale = PanelEnterScale, Action onComplete = null)
    {
        if (panel == null)
        {
            onComplete?.Invoke();
            return null;
        }

        Transform root = panel.transform;
        CanvasGroup group = EnsureCanvasGroup(panel);

        DOTween.Kill(root);
        panel.SetActive(true);

        if (group != null)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        if (root is RectTransform rectTransform)
            rectTransform.localScale = Vector3.one * startScale;

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetTarget(root);
        if (group != null)
            sequence.Join(group.DOFade(1f, duration).SetEase(Ease.OutQuad));
        if (root is RectTransform panelRect)
            sequence.Join(panelRect.DOScale(1f, duration).SetEase(Ease.OutBack));

        sequence.OnComplete(() =>
        {
            if (group != null)
            {
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }

            onComplete?.Invoke();
        });

        return sequence;
    }

    public static Tween HidePanel(GameObject panel, float duration = PanelExitDuration, float endScale = PanelEnterScale, bool deactivateOnComplete = true, Action onComplete = null)
    {
        if (panel == null)
        {
            onComplete?.Invoke();
            return null;
        }

        Transform root = panel.transform;
        CanvasGroup group = EnsureCanvasGroup(panel);

        DOTween.Kill(root);

        if (group != null)
        {
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetTarget(root);
        if (group != null)
            sequence.Join(group.DOFade(0f, duration).SetEase(Ease.OutQuad));
        if (root is RectTransform panelRect)
            sequence.Join(panelRect.DOScale(endScale, duration).SetEase(Ease.InQuad));

        sequence.OnComplete(() =>
        {
            if (deactivateOnComplete)
                panel.SetActive(false);

            if (group != null)
            {
                group.alpha = 1f;
                group.interactable = true;
                group.blocksRaycasts = true;
            }

            if (root is RectTransform rectTransform)
                rectTransform.localScale = Vector3.one;

            onComplete?.Invoke();
        });

        return sequence;
    }

    public static Tween TransitionPages(RectTransform current, RectTransform target, float duration = PageTransitionDuration, Action onComplete = null)
    {
        if (target == null)
        {
            onComplete?.Invoke();
            return null;
        }

        if (current == target)
        {
            onComplete?.Invoke();
            return null;
        }

        CanvasGroup currentGroup = current != null ? EnsureCanvasGroup(current.gameObject) : null;
        CanvasGroup targetGroup = EnsureCanvasGroup(target.gameObject);

        if (current != null)
            DOTween.Kill(current);

        DOTween.Kill(target);

        Vector2 incomingStart = new(Screen.width * 0.9f, 0f);
        Vector2 outgoingEnd = new(-Screen.width * 0.35f, 0f);

        if (current != null)
        {
            current.gameObject.SetActive(true);
            current.anchoredPosition = Vector2.zero;
            if (currentGroup != null)
            {
                currentGroup.alpha = 1f;
                currentGroup.interactable = false;
                currentGroup.blocksRaycasts = false;
            }
        }

        target.gameObject.SetActive(true);
        target.anchoredPosition = incomingStart;
        if (targetGroup != null)
        {
            targetGroup.alpha = 0f;
            targetGroup.interactable = false;
            targetGroup.blocksRaycasts = false;
        }

        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetTarget(target);

        if (current != null)
        {
            sequence.Join(current.DOAnchorPos(outgoingEnd, duration).SetEase(Ease.InOutCubic));
            if (currentGroup != null)
                sequence.Join(currentGroup.DOFade(0f, duration * 0.85f).SetEase(Ease.OutQuad));
        }

        sequence.Join(target.DOAnchorPos(Vector2.zero, duration).SetEase(Ease.OutCubic));
        if (targetGroup != null)
            sequence.Join(targetGroup.DOFade(1f, duration * 0.9f).SetEase(Ease.OutQuad));

        sequence.OnComplete(() =>
        {
            if (current != null)
            {
                current.anchoredPosition = Vector2.zero;
                current.gameObject.SetActive(false);
            }

            if (currentGroup != null)
            {
                currentGroup.alpha = 1f;
                currentGroup.interactable = true;
                currentGroup.blocksRaycasts = true;
            }

            target.anchoredPosition = Vector2.zero;
            if (targetGroup != null)
            {
                targetGroup.alpha = 1f;
                targetGroup.interactable = true;
                targetGroup.blocksRaycasts = true;
            }

            onComplete?.Invoke();
        });

        return sequence;
    }

    public static Tween PulseScale(Transform target, float pulseScale = SelectionPulseScale, float duration = SelectionPulseDuration)
    {
        if (target == null)
            return null;

        DOTween.Kill(target);

        Vector3 baseScale = target.localScale;
        Sequence sequence = DOTween.Sequence().SetUpdate(true).SetTarget(target);
        sequence.Append(target.DOScale(baseScale * pulseScale, duration * 0.45f).SetEase(Ease.OutQuad));
        sequence.Append(target.DOScale(baseScale, duration * 0.55f).SetEase(Ease.OutQuad));
        return sequence;
    }

    public static Tween PulseBadge(Transform target)
    {
        return PulseScale(target, BadgePulseScale, SelectionPulseDuration);
    }
}
