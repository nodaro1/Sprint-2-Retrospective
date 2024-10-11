//C# Unity Tutorial Boilerplate Code

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class VRLevelSelector : MonoBehaviour
{
    [SerializeField] private GameObject levelSelectorPanel;
    [SerializeField] private Button[] levelButtons;
    [SerializeField] private Button viewResultsButton;
    [SerializeField] private XRRayInteractor rayInteractor;

    private void Start()
    {
        InitializeLevelButtons();
    }

    private void InitializeLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1;
            levelButtons[i].GetComponentInChildren<Text>().text = $"Level {levelIndex}";
            levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));
        }
    }

    private void InitializeOtherButtons()
    {
        // Add more buttons for games
        viewResultsButton.GetComponentInChildren<Text>().text = $"View Results";
        viewResultsButton.onClick.AddListener(() => ViewResults());
    }

    public void ShowLevelSelector()
    {
        levelSelectorPanel.SetActive(true);
    }

    public void HideLevelSelector()
    {
        levelSelectorPanel.SetActive(false);
        // Makes sure patient checks with therapist to understand the results due to brain trauma
        Debug.Log($"Consult With Therapist To Understand Progress");
    }

    private void LoadLevel(int levelIndex)
    {
        Debug.Log($"Loading Level {levelIndex}");
        // Add levels
        HideLevelSelector();
    }

    private void ViewResults()
    {
        Debug.Log($"View of Patient Results");
        // Add details once results have been calculated
    }
}
