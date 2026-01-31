using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public static event Action<AudioClip> PlayRequested;

    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        PlayRequested += HandlePlayRequested;
    }

    private void OnDisable()
    {
        PlayRequested -= HandlePlayRequested;
    }

    public static void RequestPlay(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (Instance == null)
        {
            Debug.LogWarning("AudioManager.RequestPlay called but no AudioManager instance exists.");
            return;
        }

        PlayRequested?.Invoke(clip);
    }

    private void HandlePlayRequested(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }
}
