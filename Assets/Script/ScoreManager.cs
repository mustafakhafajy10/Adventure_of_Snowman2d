using UnityEngine;

// Tracks the current run score and draws it in the corner during gameplay.
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private Vector2 scorePosition = new Vector2(20f, 20f);
    [SerializeField] private Vector2 scoreSize = new Vector2(180f, 40f);
    [SerializeField] private int fontSize = 22;

    private GUIStyle scoreStyle;
    private bool scoreSaved;
    private int score;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        score = 0;
        scoreSaved = false;
    }

    public void IncrementScore()
    {
        // Each balloon pop gives 1 point.
        score += 1;
    }

    public int GetScore()
    {
        return score;
    }

    public void SaveScoreForCurrentPlayer()
    {
        if (scoreSaved)
        {
            return;
        }

        // Save this result once when the run ends.
        ScoreSaveSystem.AddScore(MainMenuController.CurrentPlayerName, score);
        scoreSaved = true;
    }

    private void OnGUI()
    {
        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMenuOpen)
        {
            return;
        }

        if (scoreStyle == null)
        {
            scoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                fontStyle = FontStyle.Bold
            };
            scoreStyle.normal.textColor = Color.black;
        }

        Rect shadowRect = new Rect(scorePosition.x + 2f, scorePosition.y + 2f, scoreSize.x, scoreSize.y);
        GUI.color = new Color(1f, 1f, 1f, 0.75f);
        GUI.Box(new Rect(scorePosition.x - 8f, scorePosition.y - 6f, scoreSize.x, scoreSize.y), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(shadowRect, $"Score: {score}", scoreStyle);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
