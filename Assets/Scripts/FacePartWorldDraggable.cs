using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class FacePartWorldDraggable : MonoBehaviour
{
    private Camera mainCamera;
    private Timer dragTimer;
    private Vector3 offset;
    private bool dragging;
    private AudioClip releaseClip;

    public void Init(Timer timer, AudioClip onReleaseClip)
    {
        dragTimer = timer;
        releaseClip = onReleaseClip;
    }

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        if (dragTimer == null || !dragTimer.IsRunning)
        {
            dragging = false;
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Vector3 mouseWorld = GetMouseWorld();
        offset = transform.position - mouseWorld;
        dragging = true;
    }

    private void OnMouseDrag()
    {
        if (!dragging)
        {
            return;
        }

        if (dragTimer == null || !dragTimer.IsRunning)
        {
            dragging = false;
            return;
        }

        Vector3 mouseWorld = GetMouseWorld();
        transform.position = new Vector3(mouseWorld.x + offset.x, mouseWorld.y + offset.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        Debug.Log("On Mouse up", releaseClip);
        if (dragging && releaseClip != null)
        {
            AudioManager.RequestPlay(releaseClip);
        }

        dragging = false;
    }

    private Vector3 GetMouseWorld()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Vector3 screen = Input.mousePosition;
        screen.z = Mathf.Abs(mainCamera.transform.position.z - transform.position.z);
        return mainCamera.ScreenToWorldPoint(screen);
    }
}
