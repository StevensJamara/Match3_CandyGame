using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlankGoal
{
    public int numberToGoal;
    public int numberHasScored;
    public Sprite goalSprite;
    public string MatchValue;
}

public class GoalManager : MonoBehaviour
{
    public BlankGoal[] levelGoals;

    [SerializeField]
    private GameObject goalPrefab;
    [SerializeField]
    private GameObject goalIntroParent;
    [SerializeField]
    private GameObject goalGameParent;

    void Start()
    {
        SetupIntroGoalGame();
    }

    void SetupIntroGoalGame()
    {
        for (int i = 0; i < levelGoals.Length; i++)
        {
            //Create a new Panel Goal at the GoalIntro Panel position
            GameObject goal = Instantiate(goalPrefab, goalIntroParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalIntroParent.transform, false);

            //Create new Goal Panel at the Goal Game Parent position
            GameObject gameGoal = Instantiate(goalPrefab, goalGameParent.transform.position, Quaternion.identity);
            goal.transform.SetParent(goalGameParent.transform, false);
        }
    }
    
}
