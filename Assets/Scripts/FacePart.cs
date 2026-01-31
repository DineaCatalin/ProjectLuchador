using System;
using UnityEngine;

[Serializable]
public class FacePart
{
    [SerializeField] private string id;
    [SerializeField] private FacePartType type;
    [SerializeField] private GameObject facePartPrefab;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private int weight = 1;

    public string Id => id;
    public FacePartType Type => type;
    public GameObject FacePartPrefab => facePartPrefab;
    public AudioClip AudioClip => audioClip;
    public int Weight => weight;
}
