using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class FacePartDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private RectTransform dragParent;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private AudioClip dragReleaseClip;
    private Timer dragTimer;
    private bool dragAllowed;

    public void Init(Canvas canvas, RectTransform parent, AudioClip onReleaseClip, Timer timer)
    {
        rootCanvas = canvas;
        dragParent = parent;
        dragReleaseClip = onReleaseClip;
        dragTimer = timer;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragAllowed = dragTimer == null || dragTimer.IsRunning;
        if (!dragAllowed)
        {
            return;
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragAllowed)
        {
            return;
        }

        if (dragParent == null)
        {
            return;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragParent,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragAllowed)
        {
            return;
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
        }

        if (dragReleaseClip != null)
        {
            AudioManager.RequestPlay(dragReleaseClip);
        }
    }
}
