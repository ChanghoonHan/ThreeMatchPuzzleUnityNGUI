using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float maxTime;
    public UILabel timerLabel;
    public Board gameBoard;

    [Header("Set Dynamically")]
    [SerializeField]
    private float time;
    [SerializeField]
    private UISlider timeSlider;
    private bool bStart;
    private bool bPause;

    private static Timer S = null;

    public static Timer GetInstance()
    {
        return S;
    }

    private void Awake()
    {
        S = this;
        bPause = false;
        bStart = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        time = maxTime;
        timerLabel.text = "TIME " + string.Format("{0:00.00}", time);
        timeSlider = GetComponent<UISlider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (bStart == false || bPause == true)
        {
            return;
        }

        time -= Time.deltaTime;
        if (time < 0)
        {
            gameBoard.SetGameOver();
            time = 0;
        }

        SetTimeUI();
    }

    void SetTimeUI()
    {
        timerLabel.text = "TIME " + string.Format("{0:00.00}", time);
        timeSlider.value = time / maxTime;
    }

    public void AddTime(float addTime)
    {
        if (time == 0)
        {
            return;
        }

        time += addTime;
        if (time > maxTime)
        {
            time = maxTime;
        }

        SetTimeUI();
    }

    public void StartTimer()
    {
        bStart = true;
    }

    public void PauseTimer()
    {
        bPause = true;
    }

    public void PlaybackTimer()
    {
        bPause = false;
    }
}
