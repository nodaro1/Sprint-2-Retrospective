//C# Unity Tutorial Boilerplate Code

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class VRLevelSelector : MonoBehaviour
{
    [SerializeField] private GameObject levelSelectorPanel;
    [SerializeField] private Button[] levelButtons;
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

    public void ShowLevelSelector()
    {
        levelSelectorPanel.SetActive(true);
    }

    public void HideLevelSelector()
    {
        levelSelectorPanel.SetActive(false);
    }

    private void LoadLevel(int levelIndex)
    {
        Debug.Log($"Loading Level {levelIndex}");
        // Add levels
        HideLevelSelector();
    }
}
