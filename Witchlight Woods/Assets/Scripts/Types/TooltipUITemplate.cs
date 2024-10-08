using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using WitchlightWoods.Utility;

namespace WitchlightWoods
{
    [RequireComponent(typeof(CanvasGroup))]
    public class TooltipUITemplate : MonoBehaviour
    {
        public float animationTime = 0.3f;
        [NotNull] public RectTransform RectTransform => (RectTransform)transform;
        private CanvasGroup _canvasGroup;
        public ITooltipProvider Source;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show(ITooltipProvider source, Vector2 mousePosition, Camera camera)
        {
            Source = source;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)RectTransform.parent!.transform, mousePosition, null, out var point);
            RectTransform.anchoredPosition = point;
            gameObject.SetActive(true);
            DOTween.Kill("tooltip");
            _canvasGroup
                .DOFade(1f, animationTime)
                .SetEase(Ease.InCubic)
                .SetLink(gameObject, LinkBehaviour.CompleteOnDisable)
                .SetId("tooltip");
        }

        public virtual void Hide()
        {
            DOTween.Kill("tooltip");
            _canvasGroup
                .DOFade(0f, animationTime)
                .SetEase(Ease.OutCubic)
                .OnComplete(() => gameObject.SetActive(false))
                .SetLink(gameObject, LinkBehaviour.CompleteOnDisable)
                .SetId("tooltip");
        }
    }
}