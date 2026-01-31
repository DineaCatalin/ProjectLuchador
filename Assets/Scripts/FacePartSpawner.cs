using System.Collections.Generic;
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
    [SerializeField] private Transform previewArea;
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

    private void Start()
    {
        manualSpawnCount = 0;
        UpdateManualSpawnUI();
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

    private List<GameObject> _faceParts = new();

    public void DeleteFaceParts()
    {
        foreach (var face in _faceParts)
        {
            Destroy(face.gameObject);
        }
        _faceParts.Clear();
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

        var go = Instantiate(facePart.FacePartPrefab, parent);
        _faceParts.Add(go);
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

        if (playAudio && facePart.AudioClip != null)
        {
            AudioManager.RequestPlay(facePart.AudioClip);
        }
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
            return rectTransform.TransformPoint(Vector3.zero);
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
