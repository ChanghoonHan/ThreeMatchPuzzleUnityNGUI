using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomWarpContent : UIWrapContent
{
    [Header("Set Dynamically")]
    private List<int> rankList = null;

    private void Awake()
    {
        this.transform.GetComponentInParent<UIScrollView>().ResetPosition();

        onInitializeItem += OnInitializeItemMine;
    }

    protected override void Start()
    {
        minIndex = -1 * (RankManager.GetInstance().rankSize-1);
        rankList = RankManager.GetInstance().GetRank();
        base.Start();
    }

    protected override void UpdateItem(Transform item, int index)
    {
        if (onInitializeItem != null)
        {
            int realIndex = Mathf.RoundToInt(item.localPosition.y / itemSize);
            realIndex = Mathf.Abs(realIndex);
            onInitializeItem(item.gameObject, index, realIndex);
        }
    }

    void OnInitializeItemMine(GameObject item, int index, int realIndex)
    {
        string RankString = "";

        if (rankList == null)
        {
            RankString = "#" + (realIndex + 1) + "  " + string.Format("{0:D8}", realIndex);
        }
        else
        {
            RankString = "#" + (realIndex + 1) + "  " + string.Format("{0:D8}", rankList[realIndex]);
        }

        item.GetComponentInChildren<UILabel>().text = RankString;

        return;
    }
}
