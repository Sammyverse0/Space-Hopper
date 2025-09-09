using UnityEngine;
using TMPro; // Use this if you have TextMeshPro, otherwise use UnityEngine.UI for standard Text

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public TextMeshProUGUI scoreText; // Use 'Text' if not using TextMeshPro

    [Header("Scoring")]
    public float scoreMultiplier = 1f;

    public float score;
    private float startingZ;

    // A static instance to make it easy for other scripts to access
    public static ScoreManager instance;

    void Awake()
    {
        // Set up the singleton instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Store the player's starting position so the score begins at 0
        startingZ = playerTransform.position.z;
        scoreText.text = "Score: 0";
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // Calculate score based on the distance traveled from the start
            float distance = playerTransform.position.z - startingZ;
            score = Mathf.Max(0, distance * scoreMultiplier); // Ensure score doesn't go below 0
            scoreText.text = "Score: " + ((int)score).ToString();
        }
    }

    public void OnGameOver()
    {
        // Save the high score when the game ends
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if ((int)score > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", (int)score);
        }

        PlayerPrefs.SetInt("LastScore", (int)score);
    }
}
