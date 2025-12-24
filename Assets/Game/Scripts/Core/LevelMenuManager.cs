using UnityEngine;

public class LevelMenuManager : MonoBehaviour
{
    public static LevelMenuManager Instance;
    public LevelButton[] levelButtons; // Drag all 12 buttons here in order
    public GameObject levelPanel;      // The UI Panel
    public GridManager gridManager;    // Reference to your GridManager

    void Awake() => Instance = this;

    void Start() => RefreshButtons();

    public void RefreshButtons()
    {
        // Get progress from PlayerPrefs (0 is the first level)
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 0);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            // Level is locked if its index is greater than our reached level
            bool isLocked = i > reachedLevel;
            levelButtons[i].Setup(i, isLocked);
        }
    }

    public void PlayLevel(int index)
    {
        levelPanel.SetActive(false); // Hide the menu
        gridManager.LoadLevel(index); // Start the game
    }
}