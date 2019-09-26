using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RankManager : MonoBehaviour
{
    readonly string RANK_HASH_STR = "ThreeMatchGameRank";

    private static RankManager S;

    [Header ("Set in Inspector")]
    public int rankSize = 20;

    public static RankManager GetInstance()
    {
        return S;
    }

    private void Awake()
    {
        S = this;
    }

    public void SetRank(int score)
    {
        string rankString = PlayerPrefs.GetString(RANK_HASH_STR);
        List<int> rankIntList = new List<int>();

        if (rankString == "")
        {
            rankIntList.Add(score);
            for (int i = 1; i < rankSize; i++)
            {
                rankIntList.Add(0);
            }

            rankString = ChageIntListToRankString(rankIntList);
            PlayerPrefs.SetString(RANK_HASH_STR, rankString);
            return;
        }

        rankIntList = ChangeRankStringToIntList(rankString);
        int rankIdx = 0;
        while (true)
        {
            if (score > rankIntList[rankIdx])
            {
                break;
            }
            rankIdx++;
        }

        rankIntList.Insert(rankIdx, score);
        rankIntList.RemoveAt(rankSize);
        rankString = ChageIntListToRankString(rankIntList);
        PlayerPrefs.SetString(RANK_HASH_STR, rankString);
    }

    public List<int> GetRank()
    {
        string rankString = PlayerPrefs.GetString(RANK_HASH_STR);
        List<int> rankIntList = new List<int>();
        rankIntList.Capacity = rankSize;

        if (rankString == null)
        {
            return rankIntList;
        }

        return ChangeRankStringToIntList(rankString);
    }

    List<int> ChangeRankStringToIntList(string rankStr)
    {
        string[] tempRankScoreStrArray = rankStr.Split(',');
        List<int> result = new List<int>();

        foreach (var scoreStr in tempRankScoreStrArray)
        {
            result.Add(int.Parse(scoreStr));
        }


        if (result.Count < rankSize)
        {
            int countAddRank = rankSize - result.Count;
            for (int i = 0; i < countAddRank; i++)
            {
                result.Add(0);
            }
        }

        return result;
    }

    string ChageIntListToRankString(List<int> scoreList)
    {
        string result = "";

        for (int i = 0; i < rankSize; i++)
        {
            if (i == rankSize - 1)
            {
                result += scoreList[i];
                continue;
            }

            result += scoreList[i] + ",";
        }

        return result;
    }
}
