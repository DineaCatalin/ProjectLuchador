using System;
using UnityEngine;

[Serializable]
public class FacePart
{
    [SerializeField] private string id;
    [SerializeField] private FacePartType type;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Sprite maskSprite;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private int weight = 1;

    public string Id => id;
    public FacePartType Type => type;
    public Sprite Sprite => sprite;
    public Sprite MaskSprite => maskSprite;
    public AudioClip AudioClip => audioClip;
    public int Weight => weight;
}
