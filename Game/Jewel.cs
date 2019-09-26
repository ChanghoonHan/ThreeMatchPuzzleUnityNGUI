using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jewel : MonoBehaviour
{
    public enum EJewelType
    {
        jewel1,
        jewel2,
        jewel3,
        jewel4,
        jewel5,
        jewelTypeEnd,
        jewel6,
        jewel7,
    }

    private enum EJewelState
    {
        Stay,
        Destroyed,
    }

    struct MoveInfo
    {
        public Vector3 destPos;
        public EDir dir;
        public bool bReverse;
        public bool bForSwap;
    }

    [Header("Set in Inspector")]
    public float moveSpeedPerSec = 1;
    public List<Color> particleColorList;

    [Header("Set Dynamically")]
    [SerializeField]
    private ParticleManager particleManager;
    private EJewelState jewelState;
    private EJewelType jewelType;
    private UISprite thisSprite;
    [SerializeField]
    private bool isClicked;
    private UIEventTrigger thisEventTrigger;
    [SerializeField]
    private Vector2 idxOnBoard;
    private Vector3 jewelScreenPos;
    private Board parentBoard;
    private Camera uiCamera;

    private void Awake()
    {
        isClicked = false;
        jewelType = EJewelType.jewel1;
        jewelState = EJewelState.Stay;
        thisSprite = GetComponent<UISprite>();
        thisEventTrigger = GetComponent<UIEventTrigger>();
        thisEventTrigger.onPress.Add(new EventDelegate(OnPress));
        thisEventTrigger.onRelease.Add(new EventDelegate(OnRelease));
        thisEventTrigger.onDragOut.Add(new EventDelegate(DragOut));
        uiCamera = UICamera.mainCamera;

        Color tempColor;

        for (int i = 0; i < particleColorList.Count; i++)
        {
            tempColor = particleColorList[i];
            tempColor.a = 1;
            particleColorList[i] = tempColor;
        }
    }

    private void Start()
    {
        particleManager = GameObject.Find("ParticleManager").GetComponent<ParticleManager>();
    }

    private void Update()
    {

    }

    IEnumerator MoveJewelCoroutine(MoveInfo moveInfo)
    {
        Vector3 movEDir = Vector3.zero;
        bool bMove = true;

        switch (moveInfo.dir)
        {
            case EDir.left:
                movEDir.x = -1;
                break;
            case EDir.right:
                movEDir.x = 1;
                break;
            case EDir.up:
                movEDir.y = 1;
                break;
            case EDir.down:
                movEDir.y = -1;
                break;
            default:
                break;
        }

        while (bMove)
        {
            this.transform.Translate(movEDir * moveSpeedPerSec * Time.fixedDeltaTime);

            switch (moveInfo.dir)
            {
                case EDir.left:
                    if (this.transform.localPosition.x < moveInfo.destPos.x)
                    {
                        bMove = false;
                    }
                    break;
                case EDir.right:
                    if (this.transform.localPosition.x > moveInfo.destPos.x)
                    {
                        bMove = false;
                    }
                    break;
                case EDir.up:
                    if (this.transform.localPosition.y > moveInfo.destPos.y)
                    {
                        bMove = false;
                    }
                    break;
                case EDir.down:
                    if (this.transform.localPosition.y < moveInfo.destPos.y)
                    {
                        bMove = false;
                    }
                    break;
                default:
                    break;
            }

            yield return new WaitForFixedUpdate();
        }

        this.transform.localPosition = moveInfo.destPos;
        if (moveInfo.bForSwap)
        {
            parentBoard.RequestEndSwapMoveAnimationFromJewel(this.gameObject, moveInfo.bReverse);
        }
        else
        {
            parentBoard.RequestEndMoveAnimationFromJewel();
        }
    }

    public void MoveJewelForSwap(Vector3 destPos, EDir dir, bool bReverse)
    {
        MoveInfo moveInfo;
        moveInfo.destPos = destPos;
        moveInfo.dir = dir;
        moveInfo.bReverse = bReverse;
        moveInfo.bForSwap = true;

        StartCoroutine("MoveJewelCoroutine", moveInfo);
    }

    public void MoveJewel(Vector3 destPos, EDir dir)
    {
        MoveInfo moveInfo;
        moveInfo.destPos = destPos;
        moveInfo.dir = dir;
        moveInfo.bReverse = false;
        moveInfo.bForSwap = false;

        StartCoroutine("MoveJewelCoroutine", moveInfo);
    }

    public void SetJewel()
    {
        SetVisible(true);
        jewelState = EJewelState.Stay;
        int jewelNum = Random.Range(0, (int)EJewelType.jewelTypeEnd);
        jewelType = (EJewelType)jewelNum;
        thisSprite.spriteName = "jewel" + (jewelNum + 1);
    }

    public void SetJewelWithExceptionType(EJewelType jewelTypeLeft, EJewelType jewelTypeRight, EJewelType jewelTypeUp, EJewelType jewelTypeDown)
    {
        SetVisible(true);
        jewelState = EJewelState.Stay;
        int jewelNum = (int)EJewelType.jewelTypeEnd;
        while (jewelNum == (int)jewelTypeLeft ||
            jewelNum == (int)jewelTypeRight ||
            jewelNum == (int)jewelTypeUp ||
            jewelNum == (int)jewelTypeDown ||
            jewelNum == (int)EJewelType.jewelTypeEnd)
        {
            jewelNum = Random.Range(0, (int)EJewelType.jewelTypeEnd);
        }

        jewelType = (EJewelType)jewelNum;
        thisSprite.spriteName = "jewel" + (jewelNum + 1);
    }

    public EJewelType GetJewelType()
    {
        return jewelType;
    }

    public void SetIdxOnBoard(int x, int y)
    {
        idxOnBoard.x = x;
        idxOnBoard.y = y;
    }

    public Vector2 GetIdxOnBoard()
    {
        return idxOnBoard;
    }

    public void SetBoard(Board board)
    {
        parentBoard = board;
    }

    public void DestroyJewel()
    {
        SetVisible(false);
        particleManager.PlayParticle(this.transform.localPosition, particleColorList[(int)jewelType]);
        jewelState = EJewelState.Destroyed;
    }

    private void OnPress()
    {
        if (jewelState != EJewelState.Stay)
        {
            return;
        }

        jewelScreenPos = uiCamera.WorldToScreenPoint(this.transform.position);
        isClicked = true;
    }

    private void OnRelease()
    {
        isClicked = false;
    }

    private void DragOut()
    {
        if (jewelState != EJewelState.Stay || isClicked == false)
        {
            return;
        }
        
        isClicked = false;

        float deltaX = Input.mousePosition.x - jewelScreenPos.x;
        float deltaY = Input.mousePosition.y - jewelScreenPos.y;

        EDir tempDir;

        if (Mathf.Abs(deltaY) < Mathf.Abs(deltaX))
        {
            if (0 < deltaX)
            {
                tempDir = EDir.right;
            }
            else
            {
                tempDir = EDir.left;
            }
        }
        else
        {
            if(0 < deltaY)
            {
                tempDir = EDir.up;
            }
            else
            {
                tempDir = EDir.down;
            }
        }

        parentBoard.RequestSwapWithAnotherJewelFromJewel(this.gameObject, tempDir);
    }

    private void SetVisible(bool visible)
    {
        if (visible)
        {
            thisSprite.alpha = 1;
            return;
        }

        thisSprite.alpha = 0;
    }
}
