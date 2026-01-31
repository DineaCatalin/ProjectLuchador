using UnityEngine;
using UnityEngine.UI;

public class FacePartSpawner : MonoBehaviour
{
    [SerializeField] private FacePartsDatabase database;
    [SerializeField] private RectTransform spawnPoint;
    [SerializeField] private RectTransform partsParent;

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

    public void SpawnRandomByIndex(int typeIndex)
    {
        if (typeIndex < 0 || typeIndex >= System.Enum.GetValues(typeof(FacePartType)).Length)
        {
            Debug.LogWarning($"Invalid FacePartType index {typeIndex}.");
            return;
        }

        SpawnRandom((FacePartType)typeIndex);
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

        GameObject go = new GameObject($"FacePart_{facePart.Id}");
        go.transform.SetParent(parent, false);

        RectTransform rect = go.AddComponent<RectTransform>();
        if (spawnPoint != null)
        {
            rect.anchoredPosition = spawnPoint.anchoredPosition;
            rect.localRotation = spawnPoint.localRotation;
            rect.localScale = spawnPoint.localScale;
        }

        Image image = go.AddComponent<Image>();
        image.sprite = facePart.Sprite;
        image.SetNativeSize();

        if (facePart.AudioClip != null)
        {
            AudioManager.RequestPlay(facePart.AudioClip);
        }
    }
}
