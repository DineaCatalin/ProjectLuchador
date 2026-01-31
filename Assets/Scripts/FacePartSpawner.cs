using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class FacePartSpawner : MonoBehaviour
{
    [SerializeField] private FacePartsDatabase database;
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform partsParent;
    [SerializeField] private RectTransform spawnMoveTargetArea;
    [SerializeField] private float spawnMoveDuration = 0.35f;
    [SerializeField] private Ease spawnMoveEase = Ease.OutQuad;
    [SerializeField] private RectTransform previewArea;
    [SerializeField] private int previewCount = 20;
    [SerializeField] private bool spawnPreviewOnStart = true;
    [SerializeField] private bool ensurePreviewMask = true;
    [SerializeField] private Canvas rootCanvas;

    public void SpawnRandom(FacePartType type)
    {
        if (database == null)
        {
            Debug.LogWarning("FacePartSpawner has no database assigned.");
            return;
        }

        if (!database.TryGetRandom(type, out FacePart facePart))
        {
            Debug.LogWarning($"No face parts found for type {type}.");
            return;
        }

        Spawn(facePart);
    }

    public void SpawnAnyRandom()
    {
        if (database == null)
        {
            Debug.LogWarning("FacePartSpawner has no database assigned.");
            return;
        }

        if (!database.TryGetAnyRandom(out FacePart facePart))
        {
            Debug.LogWarning($"No face parts found");
            return;
        }

        Spawn(facePart);
    }
    

    public void SpawnRandomByIndex(int typeIndex)
    {
        if (typeIndex < 0 || typeIndex >= System.Enum.GetValues(typeof(FacePartType)).Length)
        {
            Debug.LogWarning($"Invalid FacePartType index {typeIndex}.");
            return;
        }

        SpawnRandom((FacePartType)typeIndex);
    }

    public void SpawnPreview(FacePartType type)
    {
        if (previewCount <= 0)
        {
            return;
        }

        if (previewArea == null)
        {
            Debug.LogWarning("FacePartSpawner has no previewArea assigned.");
            return;
        }

        ClearPreview();

        for (int i = 0; i < previewCount; i++)
        {
            if (database.TryGetRandom(type, out FacePart facePart))
            {
                Vector2 localPos = GetRandomLocalPoint(previewArea);
                Spawn(facePart, previewArea, localPos, false, false, null);
            }
        }
    }

    public void SpawnPreviewByIndex(int typeIndex)
    {
        if (typeIndex < 0 || typeIndex >= System.Enum.GetValues(typeof(FacePartType)).Length)
        {
            Debug.LogWarning($"Invalid FacePartType index {typeIndex}.");
            return;
        }

        SpawnPreview((FacePartType)typeIndex);
    }

    public void SpawnPreviewMixed()
    {
        if (previewCount <= 0)
        {
            return;
        }

        if (previewArea == null)
        {
            Debug.LogWarning("FacePartSpawner has no previewArea assigned.");
            return;
        }

        ClearPreview();

        for (int i = 0; i < previewCount; i++)
        {
            if (database.TryGetAnyRandom(out FacePart facePart))
            {
                Vector2 localPos = GetRandomLocalPoint(previewArea);
                Spawn(facePart, previewArea, localPos, false, false, null);
            }
        }
    }

    private void Start()
    {
        if (ensurePreviewMask && previewArea != null)
        {
            EnsurePreviewMask();
        }

        if (spawnPreviewOnStart)
        {
            SpawnPreviewMixed();
        }
    }

    private void Spawn(FacePart facePart)
    {
        if (facePart == null || facePart.Sprite == null)
        {
            return;
        }

        RectTransform parent = partsParent != null ? partsParent : spawnPoint;
        if (parent == null)
        {
            Debug.LogWarning("FacePartSpawner has no spawnPoint or partsParent assigned.");
            return;
        }

        Vector2 spawnPosition = spawnPoint != null ? spawnPoint.anchoredPosition : Vector2.zero;
        Spawn(facePart, parent, spawnPosition, true, true, spawnMoveTargetArea);
    }

    private void Spawn(
        FacePart facePart,
        RectTransform parent,
        Vector2 localPosition,
        bool playAudio,
        bool draggable,
        RectTransform moveTargetArea)
    {
        if (facePart == null || facePart.Sprite == null || parent == null)
        {
            return;
        }

        GameObject go = new GameObject($"FacePart_{facePart.Id}");
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = localPosition;

        Image image = go.AddComponent<Image>();
        image.sprite = facePart.Sprite;
        image.SetNativeSize();

        if (draggable)
        {
            Canvas canvasToUse = rootCanvas;
            if (canvasToUse == null)
            {
                canvasToUse = parent.GetComponentInParent<Canvas>();
            }

            FacePartDraggable draggableComponent = go.AddComponent<FacePartDraggable>();
            draggableComponent.Init(canvasToUse, parent, facePart.AudioClip);
        }

        if (moveTargetArea != null)
        {
            Vector2 targetPos = GetRandomLocalPointInParent(moveTargetArea, parent);
            rect.DOAnchorPos(targetPos, spawnMoveDuration)
                .SetEase(spawnMoveEase)
                .SetTarget(rect);
        }

        if (playAudio && facePart.AudioClip != null)
        {
            AudioManager.RequestPlay(facePart.AudioClip);
        }
    }

    private void ClearPreview()
    {
        for (int i = previewArea.childCount - 1; i >= 0; i--)
        {
            Destroy(previewArea.GetChild(i).gameObject);
        }
    }

    private static Vector2 GetRandomLocalPoint(RectTransform rectTransform)
    {
        Rect rect = rectTransform.rect;
        float x = Random.Range(rect.xMin, rect.xMax);
        float y = Random.Range(rect.yMin, rect.yMax);
        return new Vector2(x, y);
    }

    private static Vector2 GetRandomLocalPointInParent(RectTransform area, RectTransform parent)
    {
        Vector2 localPoint = GetRandomLocalPoint(area);
        Vector3 worldPoint = area.TransformPoint(localPoint);
        Vector3 parentLocal = parent.InverseTransformPoint(worldPoint);
        return new Vector2(parentLocal.x, parentLocal.y);
    }

    private void EnsurePreviewMask()
    {
        if (previewArea.GetComponent<RectMask2D>() == null)
        {
            previewArea.gameObject.AddComponent<RectMask2D>();
        }
    }
}
