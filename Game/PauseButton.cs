using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseButton : MonoBehaviour
{
    [Header("Set in Inspector")]
    public GameObject popUpBoard;
    public GameObject popUpTouchBlocker;

    [Header("Set Dynamically")]
    private UIButton thisUIButton;

    // Start is called before the first frame update
    void Start()
    {
        thisUIButton = GetComponent<UIButton>();
        thisUIButton.onClick.Add(new EventDelegate(OnClick));

        popUpBoard.SetActive(false);
        popUpTouchBlocker.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnClick()
    {
        Timer.GetInstance().PauseTimer();
        GameStateLabel.GetInstance().Pause();
        popUpBoard.SetActive(true);
        popUpTouchBlocker.SetActive(true);
    }
}
