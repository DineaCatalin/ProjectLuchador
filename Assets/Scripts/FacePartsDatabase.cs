using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Project Luchador/Face Parts Database", fileName = "FacePartsDatabase")]
public class FacePartsDatabase : ScriptableObject
{
    [SerializeField] private List<FacePart> faceParts = new List<FacePart>();

    public IReadOnlyList<FacePart> FaceParts => faceParts;

    public bool TryGetAnyRandom(out FacePart facePart)
    {
        facePart = null;
        if (faceParts == null || faceParts.Count == 0)
        {
            return false;
        }

        int totalWeight = 0;
        for (int i = 0; i < faceParts.Count; i++)
        {
            FacePart candidate = faceParts[i];
            if (candidate != null && candidate.Weight > 0)
            {
                totalWeight += candidate.Weight;
            }
        }

        if (totalWeight <= 0)
        {
            return false;
        }

        int roll = Random.Range(0, totalWeight);
        for (int i = 0; i < faceParts.Count; i++)
        {
            FacePart candidate = faceParts[i];
            if (candidate != null && candidate.Weight > 0)
            {
                if (roll < candidate.Weight)
                {
                    facePart = candidate;
                    return true;
                }

                roll -= candidate.Weight;
            }
        }

        return false;
    }

    public bool TryGetRandom(FacePartType type, out FacePart facePart)
    {
        facePart = null;
        if (faceParts == null || faceParts.Count == 0)
        {
            return false;
        }

        int totalWeight = 0;
        for (int i = 0; i < faceParts.Count; i++)
        {
            FacePart candidate = faceParts[i];
            if (candidate != null && candidate.Type == type && candidate.Weight > 0)
            {
                totalWeight += candidate.Weight;
            }
        }

        if (totalWeight <= 0)
        {
            return false;
        }

        int roll = Random.Range(0, totalWeight);
        for (int i = 0; i < faceParts.Count; i++)
        {
            FacePart candidate = faceParts[i];
            if (candidate != null && candidate.Type == type && candidate.Weight > 0)
            {
                if (roll < candidate.Weight)
                {
                    facePart = candidate;
                    return true;
                }

                roll -= candidate.Weight;
            }
        }

        return false;
    }
}
