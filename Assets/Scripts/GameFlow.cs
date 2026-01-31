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
    [SerializeField] private Transform _maskTransform;
    [SerializeField] private Transform _faceTransform;
    [SerializeField] private Transform _faceOutsidePositonTransform;
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private Image _progressImage;
    [SerializeField] private FacePartSpawner _facePartSpawner;
    [SerializeField] private TextureOverlap _textureOverlap;

    private Vector3 _initialMaskPosition;
    
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
        _initialMaskPosition = _maskTransform.position;
    }

    private void Start()
    {
        RestartLevel();
    }

    private void RestartLevel()
    {
        _maskTransform.position = _initialMaskPosition;
        _checkMaskButton.gameObject.SetActive(true);
        
        _progressSlider.gameObject.SetActive(false);
        _progressImage.gameObject.SetActive(false);

        //_facePartSpawner?.ResetManualState();
        DOVirtual.DelayedCall(_faceShowTime, () =>
        {
            _maskTransform.DOMove(_faceOutsidePositonTransform.position, _faceMoveUpTime).SetEase(Ease.OutQuad);
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

        var successRate = _textureOverlap.PerformEvaluation();
        
        _maskTransform.DOMove(_faceTransform.position, _progressShowTime).SetEase(Ease.OutQuad);

        _progressSlider.DOValue(successRate, _progressShowTime).SetEase(Ease.Linear).OnUpdate(() => {
            if (_progressSlider.value >= _successThreshhold) 
            {
                _progressImage.color = Color.green;
            }
            else
            {
                _progressImage.color = Color.red; 
            }
        }).OnComplete(OnSuccessCheckCompleted);
    }

    [SerializeField] private float _delayToNextLevel = 1.5f;
    
    private void OnSuccessCheckCompleted()
    {
        DOVirtual.DelayedCall(_delayToNextLevel, RestartLevel);
        
        //RestartLevel();
    }
    
    private void OnDestroy()
    {
        _checkMaskButton?.onClick.AddListener(OnButtonClicked);
    }
}
