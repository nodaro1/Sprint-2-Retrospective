// Creating a very basic mastermind game while we await the Meta Quest 3
// Need to insure to integrate the Oculus SDK for VR functionality

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MastermindManager : MonoBehaviour
{
    public int codeLength = 4;  
    private List<Color> correctCode;  // the correct sequence for the user to win the game  
    public List<Color> colorChoices;  // colors that players have to choose from
    private List<Color> playerGuess;  // the user's current guess

    public GameObject feedbackPanel;

    void Start()
    {
        GenerateCode();
    }

    // Creates a unique and random color sequence
    void GenerateCode()
    {
        correctCode = new List<Color>();
        for (int i = 0; i < codeLength; i++)
        {
            Color randomColor = colorChoices[Random.Range(0, colorChoices.Count)];
            correctCode.Add(randomColor);
        }
    }

    // This function is called when a user puts in a guess
    public void SubmitGuess(List<Color> guess)
    {
        playerGuess = guess;
        CheckGuess();
    }

    // Compares the submitted guess with the correct color sequence and provides the feedback
    void CheckGuess()
    {
        int correctColors = 0;
        int correctPositions = 0;

        List<Color> remainingColors = new List<Color>(correctCode);

        for (int i = 0; i < codeLength; i++)
        {
            if (playerGuess[i] == correctCode[i])
            {
                correctPositions++;
                remainingColors.Remove(playerGuess[i]);  // Remove matched colors
            }
        }

        for (int i = 0; i < codeLength; i++)
        {
            if (playerGuess[i] != correctCode[i] && remainingColors.Contains(playerGuess[i]))
            {
                correctColors++;
                remainingColors.Remove(playerGuess[i]);  // Remove to prevent double counting
            }
        }

        DisplayFeedback(correctPositions, correctColors);

        // Check if the player has won
        if (correctPositions == codeLength)
        {
            Debug.Log("You Win!");
        }
    } 

    // Displays the feedback back to the user
    void DisplayFeedback(int correctPositions, int correctColors)
    {
        feedbackPanel.GetComponent<UnityEngine.UI.Text>().text = $"Correct Positions: {correctPositions}, Correct Colors: {correctColors}";
    }
}
