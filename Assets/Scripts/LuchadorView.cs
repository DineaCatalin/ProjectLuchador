using UnityEngine;

public class LuchadorView : MonoBehaviour
{
    [Header("References")]
    public Transform Mask;
    public Transform FaceTarget;

    [Header("Game Settings")]
    [Tooltip("The total number of face parts the player can spawn.")]
    public int MaxAmountOfParts = 10;

    [Tooltip("How much time (in seconds) the player has to complete the mask.")]
    public float TimeLimit = 60f;

    [Range(0f, 1)]
    [Tooltip("The percentage of the mask holes that must be filled to succeed.")]
    public float OverlapThreshold = 0.8f;
}