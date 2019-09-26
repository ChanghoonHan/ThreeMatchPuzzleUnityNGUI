using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public enum EBoardState
    {
        ready,
        canInput,
        movingJewel,
        gameOver,
        gameOvered,
    }

    [Header("Set in Inspector")]
    public Vector3 jewelPivot;
    public float padding;
    public int arraySize;
    public int reservJewelSetCount;
    public GameObject jewelPrefab;
    public Transform panelTrans;
    public int baseAddScore;
    public int baseComboBonus;
    public GameObject gameOverPopUpBoard;
    public GameObject popUpTouchBlocker;
    public UILabel popUpScoreLabel;

    [Header("Set Dynamically")]
    private int curAddScore;
    private int curComboBonus;
    private GameObject[,] jewelArray;
    private int gameBoardMinY;
    private int gameBoardMaxY;
    private int gameBoardMinX;
    private int gameBoardMaxX;
    private int swapJewelCount;
    private EDir swapPrevDir;
    private List<GameObject> swapJewelTempList = null;
    private List<GameObject> willDestroyJewelList = null;
    private Queue<GameObject> destroyedJewelQueue = null;
    private EBoardState boardState;
    private float destroyInvokeTime;
    private bool bGameOver;

    // Start is called before the first frame update
    void Start()
    {
        bGameOver = false;
        curAddScore = baseAddScore;
        curComboBonus = baseComboBonus;
        destroyInvokeTime = 0.3f;

        swapJewelTempList = new List<GameObject>();
        willDestroyJewelList = new List<GameObject>();
        destroyedJewelQueue = new Queue<GameObject>();
        jewelArray = new GameObject[arraySize * (reservJewelSetCount + 1), arraySize];

        GameObject tempJewel;
        Jewel tempJewelScript;
        jewelPivot.y = jewelPivot.y + (jewelPivot.y * 2 + padding)  * reservJewelSetCount ;

        swapJewelCount = 0;
        gameBoardMinX = 0;
        gameBoardMaxX = arraySize;
        gameBoardMinY = arraySize * reservJewelSetCount;
        gameBoardMaxY = arraySize * (reservJewelSetCount + 1);

        for (int x = 0; x < gameBoardMaxX; x++)
        {
            for (int y = 0; y < gameBoardMaxY; y++)
            {
                tempJewel = Instantiate(jewelPrefab);
                tempJewelScript = tempJewel.GetComponent<Jewel>();
                tempJewelScript.SetJewel();
                tempJewelScript.SetIdxOnBoard(x, y);
                tempJewelScript.SetBoard(this);
                tempJewel.transform.parent = panelTrans;
                tempJewel.transform.localScale = Vector3.one;
                tempJewel.transform.localPosition = new Vector3(jewelPivot.x + padding * x, jewelPivot.y - padding * y, 0);
                jewelArray[y, x] = tempJewel;
            }
        }

        CheckBoardAfterFirstCreate();
        boardState = EBoardState.ready;
    }

    // Update is called once per frame
    void Update()
    {
        if (bGameOver && boardState != EBoardState.movingJewel && boardState != EBoardState.gameOvered)
        {
            boardState = EBoardState.gameOvered;
            Invoke("OnGameOverPopUp", 0.5f);
        }
    }

    public void SetGameStart()
    {
        boardState = EBoardState.canInput;
        Timer.GetInstance().StartTimer();
    }

    public void OnGameOverPopUp()
    {
        int score = Score.GetInstance().GetScore();
        popUpScoreLabel.text = string.Format("{0:D8}", score);
        gameOverPopUpBoard.SetActive(true);
        popUpTouchBlocker.SetActive(true);
        RankManager.GetInstance().SetRank(score);
    }

    public void SetGameOver()
    {
        bGameOver = true;
    }

    public void RequestSwapWithAnotherJewelFromJewel(GameObject jewel, EDir dir)
    {
        if (boardState != EBoardState.canInput)
        {
            return;
        }

        Vector2 idxOnBoardJewel = jewel.GetComponent<Jewel>().GetIdxOnBoard();
        bool bCanSwap = false;

        switch (dir)
        {
            case EDir.left:
                if (idxOnBoardJewel.x - 1 < gameBoardMinX)
                {
                    return;
                }

                bCanSwap = true;
                idxOnBoardJewel.x = idxOnBoardJewel.x - 1;
                break;
            case EDir.right:
                if (idxOnBoardJewel.x + 1 >= gameBoardMaxX)
                {
                    return;
                }

                bCanSwap = true;
                idxOnBoardJewel.x = idxOnBoardJewel.x + 1;
                break;
            case EDir.up:
                if (idxOnBoardJewel.y - 1 < gameBoardMinY)
                {
                    return;
                }

                bCanSwap = true;
                idxOnBoardJewel.y = idxOnBoardJewel.y - 1;
                break;
            case EDir.down:
                if (idxOnBoardJewel.y + 1 >= gameBoardMaxY)
                {
                    return;
                }

                bCanSwap = true;
                idxOnBoardJewel.y = idxOnBoardJewel.y + 1;
                break;
            default:
                break;
        }

        if (bCanSwap)
        {
            boardState = EBoardState.movingJewel;
            SwapJewel(jewel, jewelArray[(int)idxOnBoardJewel.y, (int)idxOnBoardJewel.x], dir);
        }
    }

    public void RequestEndSwapMoveAnimationFromJewel(GameObject jewel, bool bReverse)
    {
        swapJewelCount--;
        swapJewelTempList.Add(jewel);

        if (swapJewelCount != 0)
        {
            return;
        }

        if (bReverse == true)
        {
            if (bGameOver == true)
            {
                boardState = EBoardState.gameOver;
                return;
            }

            boardState = EBoardState.canInput;
            return;
        }

        bool threeMatchMainJewel = CheckThreeMatchAfterSwap(swapJewelTempList[0]);
        bool threeMatchSubJewel = CheckThreeMatchAfterSwap(swapJewelTempList[1]);

        if (threeMatchMainJewel || threeMatchSubJewel)
        {
            DestroyJewel();
        }
        else
        {
            ReverseSwapJewel(swapJewelTempList[0], swapJewelTempList[1], swapPrevDir);
        }
    }

    public void RequestEndMoveAnimationFromJewel()
    {
        swapJewelCount--;

        if (swapJewelCount != 0)
        {
            return;
        }

        if (CheckThreeMatchAfterDestroyed() == false)
        {
            if (bGameOver == true)
            {
                boardState = EBoardState.gameOver;
                return;
            }

            curAddScore = baseAddScore;
            curComboBonus = baseComboBonus;
            boardState = EBoardState.canInput;
        }
        else
        {
            ChangeColorWillDestroyJewel();
            Invoke("DestroyJewel", destroyInvokeTime);
        }
    }

    public void ChangeColorWillDestroyJewel()
    {
        foreach (var willDestroyJewel in willDestroyJewelList)
        {
            willDestroyJewel.GetComponent<UISprite>().color = Color.red;
        }
    }

    void SwapJewel(GameObject jewel1, GameObject jewel2, EDir dir, bool bReverse = false)
    {
        swapJewelCount = 2;
        swapJewelTempList.Clear();
        swapPrevDir = dir;

        Vector2 idxOnBoardJewel1 = jewel1.GetComponent<Jewel>().GetIdxOnBoard();
        Vector2 idxOnBoardJewel2 = jewel2.GetComponent<Jewel>().GetIdxOnBoard();

        GameObject tempJewel = jewelArray[(int)idxOnBoardJewel1.y, (int)idxOnBoardJewel1.x];
        jewelArray[(int)idxOnBoardJewel1.y, (int)idxOnBoardJewel1.x] = jewelArray[(int)idxOnBoardJewel2.y, (int)idxOnBoardJewel2.x];
        jewelArray[(int)idxOnBoardJewel2.y, (int)idxOnBoardJewel2.x] = tempJewel;

        jewel1.GetComponent<Jewel>().SetIdxOnBoard((int)idxOnBoardJewel2.x, (int)idxOnBoardJewel2.y);
        jewel2.GetComponent<Jewel>().SetIdxOnBoard((int)idxOnBoardJewel1.x, (int)idxOnBoardJewel1.y);

        Vector3 tempPos = jewel1.transform.localPosition;

        jewel1.GetComponent<Jewel>().MoveJewelForSwap(jewel2.transform.localPosition, dir, bReverse);
        switch (dir)
        {
            case EDir.left:
                jewel2.GetComponent<Jewel>().MoveJewelForSwap(tempPos, EDir.right, bReverse);
                break;
            case EDir.right:
                jewel2.GetComponent<Jewel>().MoveJewelForSwap(tempPos, EDir.left, bReverse);
                break;
            case EDir.up:
                jewel2.GetComponent<Jewel>().MoveJewelForSwap(tempPos, EDir.down, bReverse);
                break;
            case EDir.down:
                jewel2.GetComponent<Jewel>().MoveJewelForSwap(tempPos, EDir.up, bReverse);
                break;
            default:
                break;
        }
    }

    void ReverseSwapJewel(GameObject jewel1, GameObject jewel2, EDir dir)
    {
        switch (dir)
        {
            case EDir.left:
                dir = EDir.right;
                break;
            case EDir.right:
                dir = EDir.left;
                break;
            case EDir.up:
                dir = EDir.down;
                break;
            case EDir.down:
                dir = EDir.up;
                break;
            default:
                break;
        }

        SwapJewel(jewel1, jewel2, dir, true);
    }

    bool CheckThreeMatchAfterSwap(GameObject threeMatchBaseJewel)
    {
        Jewel jewelScript = threeMatchBaseJewel.GetComponent<Jewel>();
        Vector2 idxOnBoardJewel = jewelScript.GetIdxOnBoard();
        Jewel.EJewelType baseJewelType = jewelScript.GetJewelType();
        int threeMatchCountHor = 0;
        int threeMatchCountVer = 0;
        bool bHaveThreeMatch = false;

        List<GameObject> willDestroyHorJewelList = new List<GameObject>();
        List<GameObject> willDestroyVerJewelList = new List<GameObject>();

        int addIdx = 1;

        while ((int)idxOnBoardJewel.y + addIdx < gameBoardMaxY &&
            jewelArray[(int)idxOnBoardJewel.y + addIdx, (int)idxOnBoardJewel.x].GetComponent<Jewel>().GetJewelType() == baseJewelType)
        {
            willDestroyVerJewelList.Add(jewelArray[(int)idxOnBoardJewel.y + addIdx, (int)idxOnBoardJewel.x]);
            addIdx++;
            threeMatchCountVer++;
        }

        addIdx = 1;
        while ((int)idxOnBoardJewel.y - addIdx >= gameBoardMinY &&
            jewelArray[(int)idxOnBoardJewel.y - addIdx, (int)idxOnBoardJewel.x].GetComponent<Jewel>().GetJewelType() == baseJewelType)
        {
            willDestroyVerJewelList.Add(jewelArray[(int)idxOnBoardJewel.y - addIdx, (int)idxOnBoardJewel.x]);
            addIdx++;
            threeMatchCountVer++;
        }

        addIdx = 1;
        while ((int)idxOnBoardJewel.x + addIdx < gameBoardMaxX &&
            jewelArray[(int)idxOnBoardJewel.y, (int)idxOnBoardJewel.x + addIdx].GetComponent<Jewel>().GetJewelType() == baseJewelType)
        {
            willDestroyHorJewelList.Add(jewelArray[(int)idxOnBoardJewel.y, (int)idxOnBoardJewel.x + addIdx]);
            addIdx++;
            threeMatchCountHor++;
        }

        addIdx = 1;
        while ((int)idxOnBoardJewel.x - addIdx >= gameBoardMinX &&
            jewelArray[(int)idxOnBoardJewel.y, (int)idxOnBoardJewel.x - addIdx].GetComponent<Jewel>().GetJewelType() == baseJewelType)
        {
            willDestroyHorJewelList.Add(jewelArray[(int)idxOnBoardJewel.y, (int)idxOnBoardJewel.x - addIdx]);
            addIdx++;
            threeMatchCountHor++;
        }

        if (threeMatchCountHor >= 2 || threeMatchCountVer >= 2)
        {
            bHaveThreeMatch = true;
            willDestroyJewelList.Add(threeMatchBaseJewel);
        }

        if (threeMatchCountHor >= 2)
        {
            foreach (var jewel in willDestroyHorJewelList)
            {
                willDestroyJewelList.Add(jewel);
            }
        }

        if (threeMatchCountVer >= 2)
        {
            foreach (var jewel in willDestroyVerJewelList)
            {
                willDestroyJewelList.Add(jewel);
            }
        }

        return bHaveThreeMatch;
    }

    bool CheckThreeMatchAfterDestroyed()
    {
        bool bHasThreeMAtch = false;

        GameObject tempJewelGO;
        Jewel tempJewelScript;
        Jewel.EJewelType baseJewelType;

        int addIdx = 1;
        int threeMatchCount = 0;

        willDestroyJewelList.Clear();

        for (int y = gameBoardMinY; y < gameBoardMaxY; y++)
        {
            for (int x = gameBoardMinX; x < gameBoardMaxX; x++)
            {
                tempJewelGO = jewelArray[y, x];
                tempJewelScript = tempJewelGO.GetComponent<Jewel>();
                baseJewelType = tempJewelScript.GetJewelType();

                addIdx = 1;
                threeMatchCount = 0;
                while (x + addIdx < gameBoardMaxX && jewelArray[y, x + addIdx].GetComponent<Jewel>().GetJewelType() == baseJewelType)
                {
                    addIdx++;
                    threeMatchCount++;
                }

                addIdx = 1;
                while (x - addIdx >= gameBoardMinX && jewelArray[y, x - addIdx].GetComponent<Jewel>().GetJewelType() == baseJewelType)
                {
                    addIdx++;
                    threeMatchCount++;
                }

                if (threeMatchCount >= 2)
                {
                    bHasThreeMAtch = true;
                    willDestroyJewelList.Add(tempJewelGO);
                    continue;
                }

                addIdx = 1;
                threeMatchCount = 0;
                while (y + addIdx < gameBoardMaxY && jewelArray[y + addIdx, x].GetComponent<Jewel>().GetJewelType() == baseJewelType)
                {
                    addIdx++;
                    threeMatchCount++;
                }

                addIdx = 1;
                while (y - addIdx >= gameBoardMinY && jewelArray[y - addIdx, x].GetComponent<Jewel>().GetJewelType() == baseJewelType)
                {
                    addIdx++;
                    threeMatchCount++;
                }

                if (threeMatchCount >= 2)
                {
                    bHasThreeMAtch = true;
                    willDestroyJewelList.Add(tempJewelGO);
                    continue;
                }
            }
        }

        return bHasThreeMAtch;
    }

    void DestroyJewel()
    {
        destroyedJewelQueue.Clear();
        Jewel tempJewelScript;
        Vector2 tempJewelIdxOnBoard;

        foreach (var jewel in willDestroyJewelList)
        {
            jewel.GetComponent<UISprite>().color = Color.white;
            tempJewelScript = jewel.GetComponent<Jewel>();
            tempJewelIdxOnBoard = tempJewelScript.GetIdxOnBoard();
            jewelArray[(int)tempJewelIdxOnBoard.y, (int)tempJewelIdxOnBoard.x] = null;
            tempJewelScript.DestroyJewel();
            destroyedJewelQueue.Enqueue(jewel);
            Score.GetInstance().AddScore(curAddScore);
            curAddScore += curComboBonus;
            curComboBonus += baseComboBonus;
        }

        willDestroyJewelList.Clear();
        SetMoveJewelAfterDestroyed();
    }

    void CheckBoardAfterFirstCreate()
    {
        Jewel.EJewelType curJewelType = Jewel.EJewelType.jewelTypeEnd;
        Jewel.EJewelType JewelTypeRight = Jewel.EJewelType.jewelTypeEnd;
        Jewel.EJewelType JewelTypeLeft = Jewel.EJewelType.jewelTypeEnd;
        Jewel.EJewelType JewelTypeDown = Jewel.EJewelType.jewelTypeEnd;
        Jewel.EJewelType JewelTypeUp = Jewel.EJewelType.jewelTypeEnd;
        bool haveThreeMatchRight = false;
        bool haveThreeMatchDown = false;

        for (int y = gameBoardMinY; y < gameBoardMaxY; y++)
        {
            for (int x = gameBoardMinX; x < gameBoardMaxX; x++)
            {
                haveThreeMatchRight = false;
                haveThreeMatchDown = false;
                curJewelType = jewelArray[y, x].GetComponent<Jewel>().GetJewelType();

                for (int addIdx = 1; addIdx < 3; addIdx++)
                {
                    if (x + addIdx < gameBoardMaxX)
                    {
                        JewelTypeRight = jewelArray[y, x + addIdx].GetComponent<Jewel>().GetJewelType();
                        if (curJewelType == JewelTypeRight)
                        {
                            haveThreeMatchRight = true;
                        }
                        else
                        {
                            haveThreeMatchRight = false;
                        }
                    }
                    else
                    {
                        JewelTypeRight = Jewel.EJewelType.jewelTypeEnd;
                        haveThreeMatchRight = false;
                    }

                    if (y + addIdx < gameBoardMaxY)
                    {
                        JewelTypeDown = jewelArray[y + addIdx, x].GetComponent<Jewel>().GetJewelType();
                        if (curJewelType == JewelTypeDown)
                        {
                            haveThreeMatchDown = true;
                        }
                        else
                        {
                            haveThreeMatchDown = false;
                        }
                    }
                    else
                    {
                        JewelTypeDown = Jewel.EJewelType.jewelTypeEnd;
                        haveThreeMatchDown = false;
                    }

                    if (haveThreeMatchRight || haveThreeMatchDown)
                    {
                        if (0 <= x - 1)
                        {
                            JewelTypeLeft = jewelArray[y, x - 1].GetComponent<Jewel>().GetJewelType();
                        }
                        else
                        {
                            JewelTypeLeft = Jewel.EJewelType.jewelTypeEnd;
                        }

                        if (reservJewelSetCount * arraySize <= y - 1)
                        {
                            JewelTypeUp = jewelArray[y - 1, x].GetComponent<Jewel>().GetJewelType();
                        }
                        else
                        {
                            JewelTypeLeft = Jewel.EJewelType.jewelTypeEnd;
                        }


                        jewelArray[y, x].GetComponent<Jewel>().SetJewelWithExceptionType(JewelTypeLeft, JewelTypeRight, JewelTypeUp, JewelTypeDown);
                    }
                }
            }
        }
    }

    void FillJewelAtBoard()
    {
        GameObject tempJewelGO;
        Jewel tempJewelScript;

        for (int x = 0; x < gameBoardMaxX; x++)
        {
            for (int y = 0; y < gameBoardMaxY; y++)
            {
                if (jewelArray[y, x] != null)
                {
                    continue;
                }

                tempJewelGO = destroyedJewelQueue.Dequeue();
                tempJewelGO.transform.localPosition = new Vector3(jewelPivot.x + padding * x, jewelPivot.y - padding * y, 0);
                tempJewelScript = tempJewelGO.GetComponent<Jewel>();
                tempJewelScript.SetJewel();
                tempJewelScript.SetIdxOnBoard(x, y);
                jewelArray[y, x] = tempJewelGO;

                if (destroyedJewelQueue.Count == 0)
                {
                    return;
                }
            }
        }
    }

    void SetMoveJewelAfterDestroyed()
    {
        GameObject tempJewelGO;
        Jewel tempJewelScript;
        int blankCount = 0;

        for (int x = 0; x < gameBoardMaxX; x++)
        {
            for (int y = gameBoardMaxY - 1; y >= 0; y--)
            {
                if (jewelArray[y, x] == null)
                {
                    blankCount++;
                    continue;
                }

                if (blankCount == 0)
                {
                    continue;
                }

                swapJewelCount++;
                tempJewelGO = jewelArray[y, x];
                jewelArray[y, x] = null;

                tempJewelScript = tempJewelGO.GetComponent<Jewel>();
                tempJewelScript.SetIdxOnBoard(x, y + blankCount);
                tempJewelScript.MoveJewel(new Vector3(jewelPivot.x + padding * x, jewelPivot.y - padding * (y + blankCount), 0), EDir.down);
                jewelArray[y + blankCount, x] = tempJewelGO;
            }

            blankCount = 0;
        }

        FillJewelAtBoard();
    }
}
