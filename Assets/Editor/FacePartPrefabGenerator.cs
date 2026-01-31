using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
public static class FacePartPrefabGenerator
{
    private const string SpritesFolder = "Assets/Images/FaceParts";
    private const string OutputFolder = "Assets/Prefabs/FaceParts";
    private const string MaskSuffix = "_white";
    private const string FaceLayerName = "Face";
    private const string ClickableLayerName = "Clickable";

    [MenuItem("Tools/Face Parts/Create Prefabs From Sprites")]
    public static void CreatePrefabsFromSprites()
    {
        if (!AssetDatabase.IsValidFolder(SpritesFolder))
        {
            Debug.LogError($"Sprites folder not found: {SpritesFolder}");
            return;
        }

        EnsureFolder(OutputFolder);

        Dictionary<string, Sprite> normalSprites = new Dictionary<string, Sprite>();
        Dictionary<string, Sprite> maskSprites = new Dictionary<string, Sprite>();

        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { SpritesFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(path);
            string key = fileName;
            if (fileName.EndsWith(MaskSuffix, System.StringComparison.OrdinalIgnoreCase))
            {
                string baseName = fileName.Substring(0, fileName.Length - MaskSuffix.Length);
                maskSprites[baseName.ToLowerInvariant()] = sprite;
            }
            else
            {
                normalSprites[key.ToLowerInvariant()] = sprite;
            }
        }

        int createdCount = 0;
        foreach (KeyValuePair<string, Sprite> pair in normalSprites)
        {
            string baseName = pair.Key;
            Sprite normal = pair.Value;
            maskSprites.TryGetValue(baseName, out Sprite mask);

            if (normal == null)
            {
                continue;
            }

            if (mask == null)
            {
                Debug.LogWarning($"Mask sprite missing for {baseName}{MaskSuffix}. Prefab will be created without mask.");
            }

            GameObject root = new GameObject(baseName);
            CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;

            GameObject normalChild = new GameObject("Normal");
            normalChild.transform.SetParent(root.transform, false);
            SpriteRenderer normalRenderer = normalChild.AddComponent<SpriteRenderer>();
            normalRenderer.sprite = normal;

            if (mask != null)
            {
                GameObject maskChild = new GameObject("Mask");
                maskChild.transform.SetParent(root.transform, false);
                SpriteRenderer maskRenderer = maskChild.AddComponent<SpriteRenderer>();
                maskRenderer.sprite = mask;
                SetLayerIfExists(maskChild, FaceLayerName);
            }

            SetLayerIfExists(root, ClickableLayerName);

            string prefabPath = Path.Combine(OutputFolder, $"{baseName}.prefab").Replace("\\", "/");
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            createdCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Created/updated {createdCount} face part prefabs in {OutputFolder}.");
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string[] parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
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
            return;
        }

        gameObject.layer = layer;
    }
}
