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
    
    private void StartLevel()
    {
        if (_gameStats.TotalLevels == 0)
        {
            return;
        }

        if (_currentLuchador != null)
        {
            Destroy(_currentLuchador.gameObject);
            _currentLuchador = null;
        }
        
        _gameStats.CurrentLevel++;

        if (_luchadors.Count < _gameStats.CurrentLevel)
        {
            Debug.LogError($"{nameof(GameStats)}: _luchadors.Count {_luchadors.Count} < _gameStats.CurrentLevel {_gameStats.CurrentLevel}");
        }

        _facePartSpawner.DeleteFaceParts();

        var template = _luchadors[_gameStats.CurrentLevel - 1];
        _currentLuchador = Instantiate(template);
        _currentLuchador.transform.position = _luchardorsSpawnPosition;
        
        _checkMaskButton.gameObject.SetActive(true);
        
        _progressSlider.gameObject.SetActive(false);
        _progressImage.gameObject.SetActive(false);
        
        CursorController.Instance.CanDrag = true;

        //_facePartSpawner?.ResetManualState();
        DOVirtual.DelayedCall(_faceShowTime, () =>
        {
            _currentLuchador.Mask.DOMove(_faceOutsidePositonTransform.position, _faceMoveUpTime).SetEase(Ease.OutQuad);
            _timer.StartTimer(_timerTime, OnTimerDone);
        });
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
