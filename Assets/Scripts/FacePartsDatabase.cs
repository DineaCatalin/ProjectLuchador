using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Project Luchador/Face Parts Database", fileName = "FacePartsDatabase")]
public class FacePartsDatabase : ScriptableObject
{
    [SerializeField] private List<FacePart> faceParts = new List<FacePart>();

    public IReadOnlyList<FacePart> FaceParts => faceParts;

    public bool TryGetRandom(FacePartType type, out FacePart facePart)
    {
        facePart = null;
        if (faceParts == null || faceParts.Count == 0)
        {
            return false;
        }

        int matchCount = 0;
        for (int i = 0; i < faceParts.Count; i++)
        {
            if (faceParts[i] != null && faceParts[i].Type == type)
            {
                matchCount++;
            }
        }

        if (matchCount == 0)
        {
            return false;
        }

        int targetIndex = Random.Range(0, matchCount);
        for (int i = 0; i < faceParts.Count; i++)
        {
            FacePart candidate = faceParts[i];
            if (candidate != null && candidate.Type == type)
            {
                if (targetIndex == 0)
                {
                    facePart = candidate;
                    return true;
                }

                targetIndex--;
            }
        }

        return false;
    }
}
