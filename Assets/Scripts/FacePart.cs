using System;
using UnityEngine;

[Serializable]
public class FacePart
{
    [SerializeField] private string id;
    [SerializeField] private FacePartType type;
    [SerializeField] private Sprite sprite;
    [SerializeField] private AudioClip audioClip;

    public string Id => id;
    public FacePartType Type => type;
    public Sprite Sprite => sprite;
    public AudioClip AudioClip => audioClip;
}
