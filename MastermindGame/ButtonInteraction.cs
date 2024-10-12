using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonInteraction : MonoBehaviour
{
    public GameManager gameManager;
    public Color buttonColor;
    
    private List<Color> playerGuess = new List<Color>();
    public List<GameObject> guessSpheres;  // If using 3D spheres

    public void OnSelectColor()
    {
        if (playerGuess.Count < gameManager.codeLength)
        {
            playerGuess.Add(buttonColor);

            UpdatePlayerGuessUI();
        }
        if (playerGuess.Count == gameManager.codeLength)
        {
            gameManager.SubmitGuess(playerGuess);
            playerGuess.Clear();  // Reset for the next round
        }
    }

    // Updates the UI or objects to show the player's current guess
    void UpdatePlayerGuessUI()
    {
        for (int i = 0; i < guessSlots.Count; i++)
        {
            
            if (i < playerGuess.Count)
            {
                Renderer renderer = guessSpheres[i].GetComponent<Renderer>();
                renderer.material.color = playerGuess[i];
            }
            else
            {
                // Reset sphere color to default (e.g., white or transparent)
                Renderer renderer = guessSpheres[i].GetComponent<Renderer>();
                renderer.material.color = Color.white;
            }
        }
    }
}
