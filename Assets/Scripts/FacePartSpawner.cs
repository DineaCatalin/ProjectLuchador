using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class FacePartSpawner : MonoBehaviour
{
    [SerializeField] private FacePartsDatabase database;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform partsParent;
    [SerializeField] private Transform spawnMoveTargetArea;
    [SerializeField] private float spawnMoveDuration = 0.35f;
    [SerializeField] private Ease spawnMoveEase = Ease.OutQuad;
    [SerializeField] private float spawnScale = 1f;
    [SerializeField] private RectTransform previewArea;
    [SerializeField] private int previewCount = 20;
    [SerializeField] private bool spawnPreviewOnStart = true;
    [SerializeField] private bool ensurePreviewMask = true;
    [SerializeField] private int manualSpawnLimit = 0;
    [SerializeField] private TMP_Text manualSpawnRemainingText;
    [SerializeField] private string manualSpawnFormat = "{0}/{1}";
    [SerializeField] private string manualSpawnUnlimitedText = "∞";
    [SerializeField] private AudioClip manualSpawnErrorClip;

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
        if (facePart == null || facePart.FacePartPrefab == null)
        {
            return;
        }

        manualSpawnCount++;
        UpdateManualSpawnUI();

        Transform parent = partsParent != null ? partsParent : spawnPoint;
        if (parent == null)
        {
            Debug.LogWarning("FacePartSpawner has no partsParent or spawnPoint assigned.");
            return;
        }

        Vector2 spawnPosition = spawnPoint != null
            ? new Vector2(spawnPoint.position.x, spawnPoint.position.y)
            : Vector2.zero;
        Spawn(facePart, parent, spawnPosition, true, true, spawnMoveTargetArea);
    }

    private void Spawn(
        FacePart facePart,
        Transform parent,
        Vector2 localPosition,
        bool playAudio,
        bool draggable,
        Transform moveTargetArea)
    {
        if (facePart == null || facePart.FacePartPrefab == null || parent == null)
        {
            return;
        }

        GameObject go = Instantiate(facePart.FacePartPrefab, parent);
        go.name = $"FacePart_{facePart.Id}";
        if (spawnScale != 1f)
        {
            go.transform.localScale = Vector3.one * spawnScale;
        }

        RectTransform rect = go.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = localPosition;
        }
        else
        {
            go.transform.position = new Vector3(localPosition.x, localPosition.y, go.transform.position.z);
        }

        if (moveTargetArea != null)
        {
            Vector3 targetPos = GetRandomWorldPoint(moveTargetArea);
            if (rect != null)
            {
                rect.DOMove(targetPos, spawnMoveDuration)
                    .SetEase(spawnMoveEase)
                    .SetTarget(rect);
            }
            else
            {
                go.transform.DOMove(targetPos, spawnMoveDuration)
                    .SetEase(spawnMoveEase)
                    .SetTarget(go.transform);
            }
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

    private static Vector3 GetRandomWorldPoint(Transform area)
    {
        if (area == null)
        {
            return Vector3.zero;
        }

        RectTransform rectTransform = area as RectTransform;
        if (rectTransform != null)
        {
            Vector2 localPoint = GetRandomLocalPoint(rectTransform);
            return rectTransform.TransformPoint(localPoint);
        }

        Collider2D collider2d = area.GetComponent<Collider2D>();
        if (collider2d != null)
        {
            Bounds bounds = collider2d.bounds;
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            return new Vector3(x, y, area.position.z);
        }

        Renderer renderer = area.GetComponent<Renderer>();
        if (renderer != null)
        {
            Bounds bounds = renderer.bounds;
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            return new Vector3(x, y, area.position.z);
        }

        return area.position;
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
        if (manualSpawnLimit <= 0)
        {
            return true;
        }

        return manualSpawnCount < manualSpawnLimit;
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
