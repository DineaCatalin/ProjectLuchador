using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private Image _timerImage;

    public void StartTimer()
    {
        _timerImage.fillAmount = 0;
    }
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
