using UnityEngine;
using UnityEngine.SceneManagement;

// Provides a simple pause/resume/restart overlay using IMGUI buttons.
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private Texture2D pauseButtonTexture;
    [SerializeField] private Texture2D restartButtonTexture;
    [SerializeField] private Texture2D resumeButtonTexture;
    [SerializeField] private Vector2 buttonSize = new Vector2(72f, 72f);
    [SerializeField] private Vector2 buttonSpacing = new Vector2(16f, 16f);
    [SerializeField] private Vector2 buttonStart = new Vector2(20f, 70f);

    private GUIStyle pauseLabelStyle;
    private bool isPaused;

    public void Pause()
    {
        // Stop gameplay time and show the pause state.
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        // Resume normal gameplay time.
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        // Reload the current scene to restart the level.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnGUI()
    {
        // Do not draw pause controls while the main menu is covering the screen.
        if (MainMenuController.Instance != null && MainMenuController.Instance.IsMenuOpen)
        {
            return;
        }

        if (pauseLabelStyle == null)
        {
            pauseLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            pauseLabelStyle.normal.textColor = Color.white;
        }

        float buttonWidth = buttonSize.x;
        float buttonHeight = buttonSize.y;
        Rect firstButtonRect = new Rect(buttonStart.x, buttonStart.y, buttonWidth, buttonHeight);

        if (!isPaused)
        {
            DrawIconButton(firstButtonRect, pauseButtonTexture, Pause);
        }
        else
        {
            GUI.color = new Color(0f, 0f, 0f, 0.55f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect((Screen.width * 0.5f) - 120f, 20f, 240f, 40f), "Game Paused", pauseLabelStyle);
            DrawIconButton(firstButtonRect, resumeButtonTexture, Resume);
        }

        Rect restartRect = new Rect(buttonStart.x, buttonStart.y + buttonHeight + buttonSpacing.y, buttonWidth, buttonHeight);
        DrawIconButton(restartRect, restartButtonTexture, Restart);
    }

    private void DrawIconButton(Rect rect, Texture2D texture, System.Action onClick)
    {
        // Fall back to text buttons if icon textures were not assigned in the inspector.
        if (texture == null)
        {
            if (GUI.Button(rect, rect.y == buttonStart.y && !isPaused ? "Pause" : "Resume"))
            {
                onClick();
            }

            return;
        }

        if (GUI.Button(rect, texture, GUIStyle.none))
        {
            onClick();
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }
}
