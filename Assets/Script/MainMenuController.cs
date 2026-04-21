using UnityEngine;
using System.IO;

// Draws the main menu and high score panel using IMGUI.
public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }
    public static string CurrentPlayerName { get; private set; } = "Player";

    [SerializeField] private Texture2D landingPageTexture;
    [SerializeField] private Rect menuRect = new Rect(235f, 115f, 170f, 290f);
    [SerializeField] private float fieldHeight = 32f;
    [SerializeField] private float buttonHeight = 34f;
    [SerializeField] private float elementSpacing = 20f;

    private GUIStyle buttonStyle;
    private GUIStyle textFieldStyle;
    private GUIStyle panelLabelStyle;
    private Texture2D buttonTexture;
    private Texture2D buttonHoverTexture;
    private Texture2D textFieldTexture;
    private string playerName = "";
    private bool showHighScores;

    public bool IsMenuOpen { get; private set; } = true;

    private void Awake()
    {
        // Keep a single active menu controller so other systems can query menu state safely.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        playerName = PlayerPrefs.GetString("PurlyPlayerName", "");
        CurrentPlayerName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
        Time.timeScale = 0f;
    }

    public void OpenMenu()
    {
        // Reopen the menu and pause gameplay time.
        showHighScores = false;
        IsMenuOpen = true;
        Time.timeScale = 0f;
    }

    private void CloseMenu()
    {
        // Save the entered name before handing control back to gameplay.
        RegisterPlayerName();
        showHighScores = false;
        IsMenuOpen = false;
        Time.timeScale = 1f;
    }

    private void StartNewGame()
    {
        CloseMenu();
    }

    private void ResumeSavedGame()
    {
        CloseMenu();
    }

    private void ToggleHighScores()
    {
        showHighScores = !showHighScores;
    }

    private void ExitGame()
    {
        Application.Quit();
    }

    private void OnGUI()
    {
        if (!IsMenuOpen)
        {
            return;
        }

        EnsureStyles();
        DrawBackground();
        DrawMenu();
    }

    private void DrawBackground()
    {
        if (landingPageTexture != null)
        {
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), landingPageTexture, ScaleMode.StretchToFill);
            return;
        }

        GUI.color = new Color(0.82f, 0.9f, 0.98f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = new Color(0.55f, 0.73f, 0.93f, 1f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height * 0.45f), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

    private void DrawMenu()
    {
        float currentY = menuRect.y;
        Rect nameRect = new Rect(menuRect.x, currentY, menuRect.width, fieldHeight);
        playerName = GUI.TextField(nameRect, playerName, 32, textFieldStyle);
        if (string.IsNullOrWhiteSpace(playerName))
        {
            GUI.Label(nameRect, "Enter Name", panelLabelStyle);
        }

        currentY += fieldHeight + elementSpacing;
        if (GUI.Button(new Rect(menuRect.x, currentY, menuRect.width, buttonHeight), "New Game", buttonStyle))
        {
            StartNewGame();
        }

        currentY += buttonHeight + elementSpacing;
        if (GUI.Button(new Rect(menuRect.x, currentY, menuRect.width, buttonHeight), "Saved Game", buttonStyle))
        {
            ResumeSavedGame();
        }

        currentY += buttonHeight + elementSpacing;
        if (GUI.Button(new Rect(menuRect.x, currentY, menuRect.width, buttonHeight), "High Scores", buttonStyle))
        {
            ToggleHighScores();
        }

        currentY += buttonHeight + elementSpacing;
        if (GUI.Button(new Rect(menuRect.x, currentY, menuRect.width, buttonHeight), "Exit", buttonStyle))
        {
            ExitGame();
        }

        if (showHighScores)
        {
            // Load the saved player names and scores into the High Scores panel.
            string[] highScoreLines = ScoreSaveSystem.GetRecentScoreLines(5);
            Rect scorePanel = new Rect(menuRect.x + menuRect.width + 24f, menuRect.y + 70f, 220f, 150f);
            GUI.color = new Color(1f, 1f, 1f, 0.88f);
            GUI.Box(scorePanel, GUIContent.none);
            GUI.color = Color.white;

            GUI.Label(new Rect(scorePanel.x + 12f, scorePanel.y + 10f, scorePanel.width - 24f, 22f), "High Scores");

            for (int i = 0; i < highScoreLines.Length; i++)
            {
                GUI.Label(new Rect(scorePanel.x + 12f, scorePanel.y + 36f + (i * 22f), scorePanel.width - 24f, 22f), highScoreLines[i]);
            }
        }
    }

    private void EnsureStyles()
    {
        // Lazily build textures/styles so they are created only when the menu is actually drawn.
        if (landingPageTexture == null)
        {
            string pngPath = Path.Combine(Application.dataPath, "Texture", "landingPage.png");
            if (File.Exists(pngPath))
            {
                Texture2D loadedTexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                loadedTexture.LoadImage(File.ReadAllBytes(pngPath));
                landingPageTexture = loadedTexture;
            }
        }

        if (buttonStyle == null)
        {
            buttonTexture = CreateColorTexture(new Color(0.70f, 0.82f, 0.95f, 0.96f));
            buttonHoverTexture = CreateColorTexture(new Color(0.77f, 0.87f, 0.98f, 0.98f));
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            buttonStyle.normal.background = buttonTexture;
            buttonStyle.hover.background = buttonHoverTexture;
            buttonStyle.active.background = buttonHoverTexture;
            buttonStyle.border = new RectOffset(1, 1, 1, 1);
            buttonStyle.normal.textColor = Color.black;
            buttonStyle.hover.textColor = Color.black;
            buttonStyle.active.textColor = Color.black;
        }

        if (textFieldStyle == null)
        {
            textFieldTexture = CreateColorTexture(new Color(1f, 1f, 1f, 0.96f));
            textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            textFieldStyle.normal.background = textFieldTexture;
            textFieldStyle.focused.background = textFieldTexture;
            textFieldStyle.normal.textColor = Color.black;
            textFieldStyle.focused.textColor = Color.black;
        }

        if (panelLabelStyle == null)
        {
            panelLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            panelLabelStyle.normal.textColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        Time.timeScale = 1f;
    }

    private static Texture2D CreateColorTexture(Color color)
    {
        // Small helper texture used as a flat background for IMGUI controls.
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private void RegisterPlayerName()
    {
        // Save the player name so the score file can store it with each result.
        CurrentPlayerName = string.IsNullOrWhiteSpace(playerName) ? "Player" : playerName.Trim();
        PlayerPrefs.SetString("PurlyPlayerName", CurrentPlayerName);
        PlayerPrefs.Save();
    }
}
