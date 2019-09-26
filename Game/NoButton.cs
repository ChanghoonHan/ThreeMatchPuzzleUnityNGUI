using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoButton : MonoBehaviour
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
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnClick()
    {
        Timer.GetInstance().PlaybackTimer();
        GameStateLabel.GetInstance().Playback();
        popUpBoard.SetActive(false);
        popUpTouchBlocker.SetActive(false);
    }
}
