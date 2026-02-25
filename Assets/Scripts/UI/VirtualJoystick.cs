using UnityEngine;
using UnityEngine.EventSystems;

namespace SurvivorMaster.UI
{
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform handle;
        [SerializeField] private float movementRange = 70f;

        private RectTransform _rectTransform;

        public Vector2 Input { get; private set; }

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rectTransform == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                return;
            }

            Vector2 normalized = localPoint / movementRange;
            Input = Vector2.ClampMagnitude(normalized, 1f);

            if (handle != null)
            {
                handle.anchoredPosition = Input * movementRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Input = Vector2.zero;
            if (handle != null)
            {
                handle.anchoredPosition = Vector2.zero;
            }
        }
    }
}
