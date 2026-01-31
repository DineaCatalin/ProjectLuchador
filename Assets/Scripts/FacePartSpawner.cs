using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class FacePartSpawner : MonoBehaviour
{
    [SerializeField] private FacePartsDatabase database;
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform partsParent;
    [SerializeField] private RectTransform placedPartsParent;
    [SerializeField] private RectTransform spawnMoveTargetArea;
    [SerializeField] private float spawnMoveDuration = 0.35f;
    [SerializeField] private Ease spawnMoveEase = Ease.OutQuad;
    [SerializeField] private RectTransform previewArea;
    [SerializeField] private int previewCount = 20;
    [SerializeField] private bool spawnPreviewOnStart = true;
    [SerializeField] private bool ensurePreviewMask = true;
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private int manualSpawnLimit = 0;
    [SerializeField] private TMP_Text manualSpawnRemainingText;
    [SerializeField] private string manualSpawnFormat = "{0}/{1}";
    [SerializeField] private string manualSpawnUnlimitedText = "∞";
    [SerializeField] private AudioClip manualSpawnErrorClip;
    [SerializeField] private Timer manualSpawnTimer;

    private int manualSpawnCount;

    public void SpawnRandom(FacePartType type)
    {
        if (!CanManualSpawn())
        {
            PlayManualSpawnError();
            return;
        }

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

    public void SpawnRandomByIndex(int typeIndex)
    {
        if (typeIndex < 0 || typeIndex >= System.Enum.GetValues(typeof(FacePartType)).Length)
        {
            Debug.LogWarning($"Invalid FacePartType index {typeIndex}.");
            return;
        }

        SpawnRandom((FacePartType)typeIndex);
    }

    public void SpawnAnyRandom()
    {
        if (!CanManualSpawn())
        {
            PlayManualSpawnError();
            return;
        }

        if (database == null)
        {
            Debug.LogWarning("FacePartSpawner has no database assigned.");
            return;
        }

        if (!database.TryGetAnyRandom(out FacePart facePart))
        {
            Debug.LogWarning("No face parts found for any type.");
            return;
        }

        Spawn(facePart);
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
        manualSpawnCount = 0;
        UpdateManualSpawnUI();

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

        RectTransform parent = placedPartsParent != null
            ? placedPartsParent
            : (partsParent != null ? partsParent : spawnPoint);
        if (parent == null)
        {
            Debug.LogWarning("FacePartSpawner has no spawnPoint or partsParent assigned.");
            return;
        }

        manualSpawnCount++;
        UpdateManualSpawnUI();

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

        if (facePart.MaskSprite != null)
        {
            SetLayerIfExists(go, "Face");

            Image maskImage = go.AddComponent<Image>();
            maskImage.sprite = facePart.MaskSprite;
            maskImage.SetNativeSize();
            maskImage.raycastTarget = true;

            Image faceImage = EnsureImageChild(go.transform, "Face", facePart.Sprite, false);
            if (faceImage != null)
            {
                faceImage.SetNativeSize();
            }
        }
        else
        {
            Image image = go.AddComponent<Image>();
            image.sprite = facePart.Sprite;
            image.SetNativeSize();
            image.raycastTarget = true;
        }

        if (draggable)
        {
            Canvas canvasToUse = rootCanvas;
            if (canvasToUse == null)
            {
                canvasToUse = parent.GetComponentInParent<Canvas>();
            }

        FacePartDraggable draggableComponent = go.AddComponent<FacePartDraggable>();
        draggableComponent.Init(canvasToUse, parent, facePart.AudioClip, manualSpawnTimer);
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

    private static Image EnsureImageChild(Transform parent, string name, Sprite sprite, bool raycastTarget)
    {
        if (parent == null || sprite == null)
        {
            return null;
        }

        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);

        RectTransform rect = child.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        Image image = child.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = raycastTarget;
        return image;
    }

    private static void SetLayerIfExists(GameObject gameObject, string layerName)
    {
        if (gameObject == null)
        {
            return;
        }

        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            Debug.LogWarning($"Layer '{layerName}' not found. Create it in Project Settings > Tags and Layers.");
            return;
        }

        gameObject.layer = layer;
    }

    private void EnsurePreviewMask()
    {
        if (previewArea.GetComponent<RectMask2D>() == null)
        {
            previewArea.gameObject.AddComponent<RectMask2D>();
        }
    }

    private bool CanManualSpawn()
    {
        if (manualSpawnTimer != null && !manualSpawnTimer.IsRunning)
        {
            return false;
        }

        if (manualSpawnLimit <= 0)
        {
            return true;
        }

        return manualSpawnCount < manualSpawnLimit;
    }

    public void ResetManualState()
    {
        manualSpawnCount = 0;
        UpdateManualSpawnUI();
        ClearPlacedParts();
    }

    public void ClearPlacedParts()
    {
        RectTransform parent = placedPartsParent != null
            ? placedPartsParent
            : (partsParent != null ? partsParent : spawnPoint);
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void UpdateManualSpawnUI()
    {
        if (manualSpawnRemainingText == null)
        {
            return;
        }

        string limitText = manualSpawnLimit <= 0 ? manualSpawnUnlimitedText : manualSpawnLimit.ToString();
        manualSpawnRemainingText.text = string.Format(manualSpawnFormat, manualSpawnCount, limitText);
    }

    private void PlayManualSpawnError()
    {
        if (manualSpawnErrorClip != null)
        {
            AudioManager.RequestPlay(manualSpawnErrorClip);
        }
    }
}
