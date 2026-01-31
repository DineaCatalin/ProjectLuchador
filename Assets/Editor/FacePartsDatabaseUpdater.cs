using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class FacePartsDatabaseUpdater
{
    private const string PrefabsFolder = "Assets/Prefabs/FaceParts";
    private const string SoundsFolder = "Assets/Sounds";
    private const string DefaultAudioClipName = "correct-156911";
    private const int DefaultWeight = 10;

    [MenuItem("Tools/Face Parts/Update Databases From Prefabs")]
    public static void UpdateDatabasesFromPrefabs()
    {
        string[] databaseGuids = AssetDatabase.FindAssets("t:FacePartsDatabase");
        if (databaseGuids.Length == 0)
        {
            Debug.LogWarning("No FacePartsDatabase assets found.");
            return;
        }

        string[] prefabGuids = AssetDatabase.FindAssets("t:GameObject", new[] { PrefabsFolder });
        if (prefabGuids.Length == 0)
        {
            Debug.LogWarning($"No prefabs found in {PrefabsFolder}.");
            return;
        }

        List<GameObject> prefabs = new List<GameObject>(prefabGuids.Length);
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                prefabs.Add(prefab);
            }
        }

        foreach (string dbGuid in databaseGuids)
        {
            string dbPath = AssetDatabase.GUIDToAssetPath(dbGuid);
            FacePartsDatabase database = AssetDatabase.LoadAssetAtPath<FacePartsDatabase>(dbPath);
            if (database == null)
            {
                continue;
            }

            SerializedObject so = new SerializedObject(database);
            SerializedProperty listProp = so.FindProperty("faceParts");
            HashSet<string> existingIds = new HashSet<string>();
            AudioClip defaultClip = FindDefaultAudioClip();

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty element = listProp.GetArrayElementAtIndex(i);
                SerializedProperty idProp = element.FindPropertyRelative("id");
                if (idProp != null && !string.IsNullOrEmpty(idProp.stringValue))
                {
                    existingIds.Add(idProp.stringValue);
                }
            }

            int addedCount = 0;
            foreach (GameObject prefab in prefabs)
            {
                string id = prefab.name;
                if (existingIds.Contains(id))
                {
                    continue;
                }

                int newIndex = listProp.arraySize;
                listProp.InsertArrayElementAtIndex(newIndex);
                SerializedProperty newElement = listProp.GetArrayElementAtIndex(newIndex);
                newElement.FindPropertyRelative("id").stringValue = id;
                SerializedProperty prefabProp = newElement.FindPropertyRelative("facePartPrefab");
                if (prefabProp != null)
                {
                    prefabProp.objectReferenceValue = prefab;
                }
                newElement.FindPropertyRelative("weight").intValue = DefaultWeight;
                SerializedProperty audioProp = newElement.FindPropertyRelative("audioClip");
                if (audioProp != null && audioProp.objectReferenceValue == null && defaultClip != null)
                {
                    audioProp.objectReferenceValue = defaultClip;
                }
                SerializedProperty typeProp = newElement.FindPropertyRelative("type");
                if (typeProp != null)
                {
                    string typeName = GetTypeNameFromId(id);
                    int typeIndex = System.Array.IndexOf(typeProp.enumNames, typeName);
                    if (typeIndex < 0)
                    {
                        typeIndex = System.Array.IndexOf(typeProp.enumNames, "Mouth");
                    }

                    if (typeIndex >= 0)
                    {
                        typeProp.enumValueIndex = typeIndex;
                    }
                }

                addedCount++;
                existingIds.Add(id);
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(database);

            Debug.Log($"Updated {dbPath}: added {addedCount} entries.");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static AudioClip FindDefaultAudioClip()
    {
        string[] guids = AssetDatabase.FindAssets($"{DefaultAudioClipName} t:AudioClip", new[] { SoundsFolder });
        if (guids.Length == 0)
        {
            return null;
        }

        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    private static string GetTypeNameFromId(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return "Mouth";
        }

        string lower = id.ToLowerInvariant();
        if (lower.Contains("eye"))
        {
            return "Eyes";
        }

        if (lower.Contains("brow"))
        {
            return "Brows";
        }

        if (lower.Contains("nose"))
        {
            return "Nose";
        }

        if (lower.Contains("ear"))
        {
            return "Ears";
        }

        if (lower.Contains("hair"))
        {
            return "Hair";
        }

        if (lower.Contains("mouth") || lower.Contains("lip"))
        {
            return "Mouth";
        }

        return "Mouth";
    }
}
