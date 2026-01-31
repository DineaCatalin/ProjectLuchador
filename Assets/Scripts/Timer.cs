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

    public void StartTimer(float seconds, Action onTimerDone)
    {
        _timerImage.fillAmount = 0;
        
        _timerSequence = _timerImage.DOFillAmount(1f, seconds).OnComplete(() =>
        {
            onTimerDone?.Invoke();
        });
    }

    public void StopTimer()
    {
        _timerSequence?.Kill();
    }
}
