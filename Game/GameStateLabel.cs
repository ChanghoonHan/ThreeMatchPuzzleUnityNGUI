using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateLabel : MonoBehaviour
{
    [Header("Set in Insptertor")]
    public float readyLabelTime;
    public float goLabelTime;
    public Board gameBoard;

    [Header("Set Dynamically")]
    private UILabel thisLabel;
    private bool bPause;
    private bool bStart;
    private float deltaTime;

    private static GameStateLabel S = null;

    public static GameStateLabel GetInstance()
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
        thisLabel = GetComponent<UILabel>();
        thisLabel.text = "READY";
    }

    // Update is called once per frame
    void Update()
    {
        if (bPause == true)
        {
            return;
        }

        if (bStart == false)
        {
            deltaTime += Time.deltaTime;
            if (readyLabelTime < deltaTime)
            {
                thisLabel.text = "GO!!";
            }

            if (readyLabelTime + goLabelTime < deltaTime)
            {
                gameBoard.SetGameStart();
                this.gameObject.SetActive(false);
            }
        }
    }

    public void Pause()
    {
        bPause = true;
    }

    public void Playback()
    {
        bPause = false;
    }
}
