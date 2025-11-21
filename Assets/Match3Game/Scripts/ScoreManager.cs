using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    private Board board;    
    public TextMeshProUGUI scoreText;
    public int score;
    public Image scoreBar;

    // Start is called before the first frame update
    void Start()
    {
        board = FindObjectOfType<Board>();
    }   

    // Update is called once per frame
    void Update()
    {
        scoreText.text = " " + score;
    }

    public void IncreaseScore(int scoreIncrease)
    {
        score += scoreIncrease;
        if (board != null && scoreBar != null)
        {
            int lengthScore = board.scoreGoals.Length;
            scoreBar.fillAmount = (float) score / (float) board.scoreGoals[lengthScore - 1];
        }
    }
}
