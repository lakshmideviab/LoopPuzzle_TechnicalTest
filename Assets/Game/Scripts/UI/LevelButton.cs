using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    public int levelIndex; // Set this to 0 for Button 1, 1 for Button 2, etc.
    public Button button;
    public TextMeshProUGUI levelText;
    public GameObject lockIcon;

    public void Setup(int index, bool isLocked)
    {
        levelIndex = index;
        levelText.text = (index + 1).ToString();

        // Disable button if locked
        button.interactable = !isLocked;
        lockIcon.SetActive(isLocked);

        // Set up the click event
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => LevelMenuManager.Instance.PlayLevel(levelIndex));
    }
}