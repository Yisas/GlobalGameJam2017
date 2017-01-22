using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ScoreManager : MonoBehaviour {
    public Text pointsLabel;
    public static ScoreManager Instance;
    public int currentScore;

	// Use this for initialization
	void Start () {
        Instance = this;
	}

    public void addScore(int score)
    {
        currentScore += score;
        pointsLabel.text = currentScore.ToString();
    }
    // Update is called once per frame
    void Update() {
        //to use the score manager just call this method, remember to click display 2
        //duplicate the labels if necessary
        if (Input.GetKeyDown(KeyCode.Backspace)) //cheats
        {
            ScoreManager.Instance.addScore(1);
        }

        int scoreToAdd = (int)(Time.deltaTime * 100);
        ScoreManager.Instance.addScore(scoreToAdd);
    }
}
