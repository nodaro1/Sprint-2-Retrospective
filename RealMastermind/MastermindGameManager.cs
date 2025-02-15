using UnityEngine;
using System.Collections.Generic;

public class MastermindGameManager : MonoBehaviour
{
    public int sequenceLength = 4; // Number of colors per row
    public int maxAttempts = 5; // Maximum number of guesses
    private int currentAttempt = 0; // Tracks which row is currently being guessed

    public List<BlockColor> solutionColors = new List<BlockColor>(); // The correct sequence

    // Separate lists for each row of slots and feedback cubes
    public List<BoardSlot> row1Slots = new List<BoardSlot>();
    public List<BoardSlot> row2Slots = new List<BoardSlot>();
    public List<BoardSlot> row3Slots = new List<BoardSlot>();
    public List<BoardSlot> row4Slots = new List<BoardSlot>();
    public List<BoardSlot> row5Slots = new List<BoardSlot>();

    public List<GameObject> row1FeedbackCubes = new List<GameObject>();
    public List<GameObject> row2FeedbackCubes = new List<GameObject>();
    public List<GameObject> row3FeedbackCubes = new List<GameObject>();
    public List<GameObject> row4FeedbackCubes = new List<GameObject>();
    public List<GameObject> row5FeedbackCubes = new List<GameObject>();

    public Color correctPositionColor = Color.red;
    public Color correctColorWrongPositionColor = Color.white;
    public Color defaultColor = Color.gray;

    private void Start()
    {
        GenerateRandomSolution();
    }


    private void GenerateRandomSolution()
{
    solutionColors.Clear();

    BlockColor[] availableColors = (BlockColor[])System.Enum.GetValues(typeof(BlockColor));

    List<BlockColor> colorPool = new List<BlockColor>(availableColors);
    colorPool.Remove(BlockColor.None); // Ensure 'None' is not in the selection

    // Shuffle the list to ensure randomness
    for (int i = colorPool.Count - 1; i > 0; i--)
    {
        int randomIndex = Random.Range(0, i + 1);
        BlockColor temp = colorPool[i];
        colorPool[i] = colorPool[randomIndex];
        colorPool[randomIndex] = temp;
    }

    // Take the first `sequenceLength` (4) colors to form the solution
    for (int i = 0; i < sequenceLength; i++)
    {
        solutionColors.Add(colorPool[i]);
    }

    Debug.Log("Generated Unique Solution: " + string.Join(", ", solutionColors));
}

    /*private void GenerateRandomSolution()
    {
        solutionColors.Clear();
        BlockColor[] availableColors = (BlockColor[])System.Enum.GetValues(typeof(BlockColor));

        for (int i = 0; i < sequenceLength; i++)
        {
            BlockColor randomColor;
            do
            {
                randomColor = availableColors[Random.Range(1, availableColors.Length)]; // Skipping 'None'
            } while (randomColor == BlockColor.None); // Ensure 'None' is never chosen

            solutionColors.Add(randomColor);
        }

        Debug.Log("Generated Solution: " + string.Join(", ", solutionColors));
    }*/

    public void CheckGuess()
    {
        if (currentAttempt >= maxAttempts)
        {
            Debug.Log("No more guesses allowed! Game Over!");
            return;
        }

        List<BlockColor> guessColors = new List<BlockColor>();
        List<BoardSlot> currentRowSlots = GetCurrentRowSlots();
        List<GameObject> currentRowFeedbackCubes = GetCurrentRowFeedbackCubes();

        if (currentRowSlots == null || currentRowFeedbackCubes == null)
        {
            Debug.LogError("No slots or feedback cubes assigned for this row!");
            return;
        }

        // Read block colors from the current row
        foreach (BoardSlot slot in currentRowSlots)
        {
            if (slot.currentBlock != null)
            {
                guessColors.Add(slot.currentBlock.blockColor);
            }
            else
            {
                Debug.Log("You must fill all slots before checking the guess.");
                return; // Prevents checking until all slots are filled
            }
        }

        // Compare the guess with the solution
        var (correctPosition, correctColorWrongPosition) = EvaluateGuess(guessColors, solutionColors);

        // Update the feedback cubes for the current row
        AssignFeedback(correctPosition, correctColorWrongPosition, currentRowFeedbackCubes);

        // Check if the user has guessed correctly
        if (correctPosition == sequenceLength)
        {
            Debug.Log("You guessed the correct sequence! Game Over!");
            return;
        }

        // Move to the next row for the next guess
        currentAttempt++;
    }

    private List<BoardSlot> GetCurrentRowSlots()
    {
        switch (currentAttempt)
        {
            case 0: return row1Slots;
            case 1: return row2Slots;
            case 2: return row3Slots;
            case 3: return row4Slots;
            case 4: return row5Slots;
            default: return null;
        }
    }

    private List<GameObject> GetCurrentRowFeedbackCubes()
    {
        switch (currentAttempt)
        {
            case 0: return row1FeedbackCubes;
            case 1: return row2FeedbackCubes;
            case 2: return row3FeedbackCubes;
            case 3: return row4FeedbackCubes;
            case 4: return row5FeedbackCubes;
            default: return null;
        }
    }

    private (int correctPosition, int correctColorWrongPosition) EvaluateGuess(List<BlockColor> guess, List<BlockColor> solution)
    {
        int correctPosition = 0;
        int correctColorWrongPosition = 0;

        List<BlockColor> solutionCopy = new List<BlockColor>(solution);
        List<bool> matchedGuess = new List<bool>(new bool[guess.Count]);

        // First pass: Check exact matches (right color, right position)
        for (int i = 0; i < guess.Count; i++)
        {
            if (i < solutionCopy.Count && guess[i] == solutionCopy[i])
            {
                correctPosition++;
                matchedGuess[i] = true;
                solutionCopy[i] = BlockColor.None; // Mark it as used
            }
        }

        // Second pass: Check correct color but wrong position
        for (int i = 0; i < guess.Count; i++)
        {
            if (!matchedGuess[i] && solutionCopy.Contains(guess[i]) && guess[i] != BlockColor.None)
            {
                correctColorWrongPosition++;
                solutionCopy[solutionCopy.IndexOf(guess[i])] = BlockColor.None; // Mark as used
            }
        }

        return (correctPosition, correctColorWrongPosition);
    }


    private void AssignFeedback(int correctPosition, int correctColorWrongPosition, List<GameObject> feedbackCubes)
    {
        // Step 1: Reset all feedback cubes to default gray before applying colors
        foreach (var cube in feedbackCubes)
        {
            cube.GetComponent<MeshRenderer>().material.color = defaultColor;
        }

        int index = 0;

        // Step 2: Assign red for correct color & position
        for (int i = 0; i < correctPosition; i++)
        {
            if (index < feedbackCubes.Count)
            {
                feedbackCubes[index].GetComponent<MeshRenderer>().material.color = correctPositionColor;
                index++;
            }
        }

        // Step 3: Assign white for correct color in the wrong position
        for (int i = 0; i < correctColorWrongPosition; i++)
        {
            if (index < feedbackCubes.Count)
            {
                feedbackCubes[index].GetComponent<MeshRenderer>().material.color = correctColorWrongPositionColor;
                index++;
            }
        }

        // Step 4: Ensure all feedback cubes are accounted for
        while (index < feedbackCubes.Count)
        {
            feedbackCubes[index].GetComponent<MeshRenderer>().material.color = defaultColor; // Ensure remaining cubes stay gray
            index++;
        }
    }

    /*private void AssignFeedback(int correctPosition, int correctColorWrongPosition, List<GameObject> feedbackCubes)
    {
        foreach (var cube in feedbackCubes)
        {
            cube.GetComponent<MeshRenderer>().material.color = defaultColor;
        }

        int index = 0;

        // Assign red for correct color & position
        for (int i = 0; i < correctPosition; i++)
        {
            if (index < feedbackCubes.Count)
            {
                feedbackCubes[index].GetComponent<MeshRenderer>().material.color = correctPositionColor;
                index++;
            }
        }

        // Assign white for correct color in the wrong position
        for (int i = 0; i < correctColorWrongPosition; i++)
        {
            if (index < feedbackCubes.Count)
            {
                feedbackCubes[index].GetComponent<MeshRenderer>().material.color = correctColorWrongPositionColor;
                index++;
            }
        }
    }*/
}
