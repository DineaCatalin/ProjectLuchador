using UnityEngine;
using UnityEngine.UI;

public class GameFlow : MonoBehaviour
{
    [SerializeField] private float _timerTime = 10f;
    [SerializeField] private Timer _timer;
    [SerializeField] private Button _checkMaskButton;
    
    private void Awake()
    {
        _checkMaskButton?.onClick.AddListener(OnButtonClicked);
    }

    private void Start()
    {
        _timer.StartTimer(_timerTime);
    }
    
    private void OnButtonClicked()
    {
        _timer.StopTimer();
    }
    
    // Update is called once per frame
    private void Update()
    {
        
    }
    
    private void OnDestroy()
    {
        _checkMaskButton?.onClick.AddListener(OnButtonClicked);
    }
}
