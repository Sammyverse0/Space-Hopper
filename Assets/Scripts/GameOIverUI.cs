using UnityEngine;
using TMPro; // Or use UnityEngine.UI for standard Text

public class GameOverUI : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    void Start()
    {
        // Get the scores that were saved by the ScoreManager
        int lastScore = PlayerPrefs.GetInt("LastScore", 0);
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        // Update the text elements on the screen
        finalScoreText.text = "Your Score: " + lastScore.ToString();
        highScoreText.text = "High Score: " + highScore.ToString();
    }
}
