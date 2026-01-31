using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image _timerImage;
    
    private TweenerCore<float,float,FloatOptions> _timerSequence;
    private bool _isRunning;

    public bool IsRunning => _isRunning;

    public void StartTimer(float seconds, Action onTimerDone)
    {
        _timerSequence?.Kill();
        _timerImage.fillAmount = 0;
        _isRunning = true;
        
        _timerSequence = _timerImage.DOFillAmount(1f, seconds).OnComplete(() =>
        {
            _isRunning = false;
            onTimerDone?.Invoke();
        });
    }

    public void StopTimer()
    {
        _timerSequence?.Kill();
        _isRunning = false;
    }
}
