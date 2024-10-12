using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class VRLevelSelector : MonoBehaviour
{
    [SerializeField] private GameObject levelSelectorPanel;
    [SerializeField] private Button[] levelButtons; // assign these in inspector
    [SerializeField] private Button viewResultsButton;
    [SerializeField] private XRRayInteractor rayInteractor;

    private void Start()
    {
        InitializeLevelButtons();
        InitializeOtherButtons();
    }

    private void InitializeLevelButtons()
    {
        string[] gameNames = { "Mastermind", "Spot the difference", "Stacking Challenge" };

        for (int i = 0; i < levelButtons.Length && i < gameNames.Length; i++)
        {
            // set button text to the game name
            levelButtons[i].GetComponentInChildren<Text>().text = gameNames[i];

            int index = i; // retrieve index for the closure
            levelButtons[i].onClick.AddListener(() => LoadLevel(gameNames[index]));
        }
    }

    private void InitializeOtherButtons()
    {
        // initializing "View Results" button
        viewResultsButton.GetComponentInChildren<Text>().text = "View Results";
        viewResultsButton.onClick.AddListener(ViewResults);
    }

    public void ShowLevelSelector()
    {
        levelSelectorPanel.SetActive(true);
    }

    public void HideLevelSelector()
    {
        levelSelectorPanel.SetActive(false);
        Debug.Log("Consult With Therapist To Understand Progress");
    }

    private void LoadLevel(string gameName)
    {
        Debug.Log($"Loading {gameName}");
        // code to load the specific game scene or start the game logic
        HideLevelSelector();
    }

    private void ViewResults()
    {
        Debug.Log("Viewing Patient Results");
        // code to display results
    }
}
