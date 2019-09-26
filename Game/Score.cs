using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Score : MonoBehaviour
{
    [Header("Set in Inspector")]
    public int bonusTimeScore;
    public float bonusAddTime;

    [Header("Set Dynamically")]
    private int score;
    private int bonusTimeCheckNum;
    private UILabel thisLabel;

    private static Score S = null;

    public static Score GetInstance()
    {
        return S;
    }

    private void Awake()
    {
        S = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        bonusTimeCheckNum = 1;
        thisLabel = GetComponent<UILabel>();
        thisLabel.text = string.Format("{0:D8}", score);
    }

    public void AddScore(int addScore)
    {
        score += addScore;

        if (score >= bonusTimeCheckNum * bonusTimeScore)
        {
            bonusTimeCheckNum++;
            Timer.GetInstance().AddTime(bonusAddTime);
        }

        SetScoreUI();
    }

    void SetScoreUI()
    {
        thisLabel.text = string.Format("{0:D8}", score);
    }

    public int GetScore()
    {
        return score;
    }
}
