using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameFlow : MonoBehaviour
{
    public static GameFlow Instance { get; private set; }
    
    [SerializeField] private float _timerTime = 10f;
    [SerializeField] private float _faceMoveUpTime = 1.5f;
    [SerializeField] private float _faceShowTime = 2;
    [SerializeField] private float _progressShowTime = 10f;
    [SerializeField] private Timer _timer;
    [SerializeField] private Button _checkMaskButton;
    [SerializeField] private Transform _faceOutsidePositonTransform;
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private Image _progressImage;
    [SerializeField] private FacePartSpawner _facePartSpawner;
    [SerializeField] private TextureOverlap _textureOverlap;
    [SerializeField] private List<LuchadorView> _luchadors;
    [SerializeField] private Vector3 _luchardorsSpawnPosition;

    private Vector3 _initialMaskPosition;
    private GameStats _gameStats;
    
    // Test
    [Range(0,1)] [SerializeField]
    private float _testProgress = 0.8f;
    [SerializeField] private float _successThreshhold = 0.8f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _checkMaskButton?.onClick.AddListener(OnButtonClicked);
        
        _gameStats =  new GameStats
        {
            TotalLevels = _luchadors.Count
        };
        if (_gameStats.TotalLevels == 0)
        {
            Debug.LogError($"{nameof(GameStats)}: No Luchadors, abort == 0");
        }
    }

    private void Start()
    {
        StartLevel();
    }

    private LuchadorView _currentLuchador;
    private List<LuchadorView> _luchadorsSpawned = new();
    
    private void StartLevel()
    {
        if (_gameStats.TotalLevels == 0)
        {
            return;
        }

        if (_currentLuchador != null)
        {
            _currentLuchador.transform.position = new Vector3(100, 0, 0);
            DeleteFacePartsThatAreNotOnFace(_currentLuchador);
            _currentLuchador = null;
        }
        
        _gameStats.CurrentLevel++;

        if (_luchadors.Count < _gameStats.CurrentLevel)
        {
            Debug.LogError($"{nameof(GameStats)}: _luchadors.Count {_luchadors.Count} < _gameStats.CurrentLevel {_gameStats.CurrentLevel}");
        }

        //_facePartSpawner.DeleteFaceParts();

        var template = _luchadors[_gameStats.CurrentLevel - 1];
        _currentLuchador = Instantiate(template);
        _luchadorsSpawned.Add(_currentLuchador);
        _facePartSpawner.PartsParent = _currentLuchador.FacePartsContainer;
        _currentLuchador.transform.position = _luchardorsSpawnPosition;
        
        _checkMaskButton.gameObject.SetActive(true);
        
        _progressSlider.gameObject.SetActive(false);
        _progressImage.gameObject.SetActive(false);
        
        CursorController.Instance.CanDrag = true;

        DOVirtual.DelayedCall(_faceShowTime, () =>
        {
            _currentLuchador.Mask.DOMove(_faceOutsidePositonTransform.position, _faceMoveUpTime).SetEase(Ease.OutQuad);
            _timer.StartTimer(_timerTime, OnTimerDone);
        });
    }

    private void DeleteFacePartsThatAreNotOnFace(LuchadorView luchador)
    {
        Renderer maskRenderer = luchador.Mask.GetComponent<Renderer>();

        if (maskRenderer != null)
        {
            // Use the mask's world bounds as the "safe zone"
            Bounds maskBounds = maskRenderer.bounds;
        
            for (int i = luchador.FacePartsContainer.childCount - 1; i >= 0; i--)
            {
                Transform facePart = luchador.FacePartsContainer.GetChild(i);
            
                // 1. Get all renderers in the children (e.g., the base eye and the pupil)
                Renderer[] childRenderers = facePart.GetComponentsInChildren<Renderer>();

                if (childRenderers.Length > 0)
                {
                    // 2. Initialize the bounds with the first child's renderer
                    Bounds combinedBounds = childRenderers[0].bounds;

                    // 3. Expand the bounds to include all other child renderers
                    for (int j = 1; j < childRenderers.Length; j++)
                    {
                        combinedBounds.Encapsulate(childRenderers[j].bounds);
                    }

                    // 4. Check if the entire combined box is within the mask
                    if (!maskBounds.Contains(combinedBounds.min) || 
                        !maskBounds.Contains(combinedBounds.max))
                    {
                        Debug.Log($"Deleting {facePart.name} because it was outside bounds.");
                        Destroy(facePart.gameObject);
                    }
                }
            }
        }
    }
    
    private void OnButtonClicked()
    {
        _timer.StopTimer();
        OnTimerDone();
    }

    private void OnTimerDone()
    {
        _checkMaskButton.gameObject.SetActive(false);
        
        _progressSlider.gameObject.SetActive(true);
        _progressSlider.value = 0;
        _progressImage.gameObject.SetActive(true);
        _progressImage.color = Color.darkRed;

        CursorController.Instance.CanDrag = false;

        var successRate = _textureOverlap.PerformEvaluation();
        
        _currentLuchador.Mask.DOMove(_currentLuchador.FaceTarget.position, _progressShowTime).SetEase(Ease.OutQuad);

        _progressSlider.DOValue(successRate, _progressShowTime).SetEase(Ease.Linear).OnUpdate(() => {
            if (_progressSlider.value >= _successThreshhold) 
            {
                _progressImage.color = Color.green;
            }
            else
            {
                _progressImage.color = Color.red; 
            }
        }).OnComplete(() => OnSuccessCheckCompleted(_progressSlider.value >= _successThreshhold));
    }

    [SerializeField] private float _delayToNextLevel = 1.5f;
    
    private void OnSuccessCheckCompleted(bool levelWon)
    {
        if (levelWon)
        {
            _gameStats.RoundsWon++;
        }
        else
        {
            _gameStats.RoundsLost++;
        }
        
        if(_gameStats.CurrentLevel < _gameStats.TotalLevels)
            DOVirtual.DelayedCall(_delayToNextLevel, StartLevel);
    }
    
    private void OnDestroy()
    {
        _checkMaskButton?.onClick.AddListener(OnButtonClicked);
    }

    public class GameStats
    {
        public int RoundsWon { get; set; }
        public int RoundsLost  { get; set; }
        public int CurrentLevel  { get; set; }
        
        public int TotalLevels { get; set; }
    }
}
