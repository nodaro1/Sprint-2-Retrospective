using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public Canvas menuCanvas;    // Assign your MenuCanvas
    public Canvas uiCanvas;      // Assign your UICanvas
    public Canvas attemptsCanvas; // Assign your AttemptsCanvas

    [Header("Timer Settings")]
    public TextMeshProUGUI timerText;
    private float timer;
    private bool timerRunning = false;

    [Header("Completion Settings")]
    public TextMeshProUGUI completionText;
    private bool completionMessageDisplayed = false;

    [Header("Stacking Settings")]
    public List<GameObject> blocks = new List<GameObject>();
    // this is crucial for integration with other games lol. rn your blocks are scaled to 0.1
    public float stackTolerance = 0.001f;

        // How long the stack has been stationary
    private float stationaryTime = 0f;
    // Time required for the stack to be considered stationary
    public float requiredStationaryTime = 2f; 
    // How much a block can move in a frame before we consider it 'moving'
    public float movementThreshold = 0.001f; 
    // Dictionary to store last known positions of blocks
    private Dictionary<GameObject, Vector3> lastPositions = new Dictionary<GameObject, Vector3>();


    [Header("Spawner Reference")]
    public BlockSpawner blockSpawner;  // Drag your BlockSpawner object here in the Inspector.

    [Header("Attempt Times")]
    // List to store each successful attempt's time. (CREATE A BETTER DATA STRUCTURE IN THE FUTURE. FIND A WAY TO SAVE TO DISK)
    private List<float> attemptTimes = new List<float>();
    // Public property to access attemptTimes from other scripts if needed.
    public List<float> AttemptTimes { get { return attemptTimes; } }

    [Header("Attempts UI")]
    // The TextMeshProUGUI component that will display the list of attempt times.
    public TextMeshProUGUI attemptsText;

    void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ensure the completion message is hidden
        HideCompletionMessage();
        // Hide the attempts canvas at startup.
        if (attemptsCanvas != null)
            attemptsCanvas.gameObject.SetActive(false);

        // Show the menu, hide the game UI at the beginning
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(true);
        if (uiCanvas != null)
            uiCanvas.gameObject.SetActive(false);
    }

      void Update()
{
    if (timerRunning)
    {
        timer += Time.deltaTime;
        UpdateTimerDisplay();

        bool anyBlockMoved = false;
        foreach (var block in blocks)
        {
            Vector3 currentPos = block.transform.position;
            if (lastPositions.TryGetValue(block, out Vector3 lastPos))
            {
                if (Vector3.Distance(currentPos, lastPos) > movementThreshold)
                {
                    anyBlockMoved = true;
                }
            }
            lastPositions[block] = currentPos;
        }

        if (anyBlockMoved)
        {
            stationaryTime = 0f;
        }
        else
        {
            stationaryTime += Time.deltaTime;
        }

        // Only check stacking if the stack has been stationary for the required time.
        if (stationaryTime >= requiredStationaryTime)
        {
            CheckStacking();
        }
    }
}



    // --- START BUTTON HANDLER ---
    public void OnStartButtonClicked()
    {
        // Hide Menu and Attempts canvas, then show the game UI.
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);
        if (uiCanvas != null)
            uiCanvas.gameObject.SetActive(true);
        if (attemptsCanvas != null)
            attemptsCanvas.gameObject.SetActive(false);

        // Spawn blocks
        if (blockSpawner != null)
        {
            // Get the newly spawned blocks
            var spawnedBlocks = blockSpawner.SpawnBlocks();
            // Start the timer for these blocks
            StartTimer(spawnedBlocks);
        }
        else
        {
            Debug.LogError("BlockSpawner not assigned in GameManager!");
        }
    }

    // Start the timer and prepare for a new attempt.
   public void StartTimer(List<GameObject> spawnedBlocks)
{
    blocks = spawnedBlocks;
    timer = 0f;
    timerRunning = true;
    UpdateTimerDisplay();
    HideCompletionMessage();

    lastPositions.Clear();
    foreach (var block in blocks)
    {
        lastPositions[block] = block.transform.position;
    }
    stationaryTime = 0f;
}


    

    // Stop the timer when stacking is successful.
    // Save the attempt time and reset the UI.
    public void StopTimer()
    {
        timerRunning = false;
        Debug.Log("Timer Stopped! Time: " + timer.ToString("F2") + " seconds");

        // Save this attempt's time to the list.
        attemptTimes.Add(timer);

        ShowCompletionMessage();

        // Return to the menu after the attempt.
        ReturnToMenu();
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
            timerText.text = "Time: " + timer.ToString("F2") + "s";
        else
            Debug.LogWarning("Timer Text is not assigned in the GameManager.");
    }

    // Check if all blocks are stacked correctly.
    void CheckStacking()
{
    if (blocks.Count < 3)
        return;

    // Sort blocks by their center Y value (lowest to highest)
    var sortedBlocks = blocks.OrderBy(b => b.transform.position.y).ToList();
    bool allStacked = true;

    for (int i = 0; i < sortedBlocks.Count - 1; i++)
    {
        GameObject lowerBlock = sortedBlocks[i];
        GameObject upperBlock = sortedBlocks[i + 1];

        // Get the collider's world size (should be ~0.1 if scale is 0.1)
        float lowerHeight = lowerBlock.GetComponent<Collider>().bounds.size.y;
        float upperHeight = upperBlock.GetComponent<Collider>().bounds.size.y;

        // Calculate the top face of the lower block and the bottom face of the upper block
        float lowerTop = lowerBlock.transform.position.y + lowerHeight * 0.5f;
        float upperBottom = upperBlock.transform.position.y - upperHeight * 0.5f;

        // Check horizontal alignment (using centers, since our cubes are small)
        Vector3 lowerPos = lowerBlock.transform.position;
        Vector3 upperPos = upperBlock.transform.position;
        if (Mathf.Abs(lowerPos.x - upperPos.x) > stackTolerance ||
            Mathf.Abs(lowerPos.z - upperPos.z) > stackTolerance)
        {
            allStacked = false;
            break;
        }

        // Check if the upper block is exactly touching the lower block (within tolerance)
        if (Mathf.Abs(upperBottom - lowerTop) > stackTolerance)
        {
            allStacked = false;
            break;
        }
    }

    if (allStacked && !completionMessageDisplayed)
    {
        StopTimer();
    }
}




    void ShowCompletionMessage()
    {
        if (completionText != null)
        {
            Color currentColor = completionText.color;
            currentColor.a = 1f;
            completionText.color = currentColor;
            completionMessageDisplayed = true;
        }
        else
        {
            Debug.LogWarning("Completion Text is not assigned in the GameManager.");
        }
    }

    void HideCompletionMessage()
    {
        if (completionText != null)
        {
            Color currentColor = completionText.color;
            currentColor.a = 0f;
            completionText.color = currentColor;
            completionMessageDisplayed = false;
        }
        else
        {
            Debug.LogWarning("Completion Text is not assigned in the GameManager.");
        }
    }

    // Method to revert the UI to the main menu.
    private void ReturnToMenu()
    {
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(true);
        if (uiCanvas != null)
            uiCanvas.gameObject.SetActive(false);
    }

    // --- VIEW ATTEMPTS HANDLER ---
    // Called when the user clicks the "ViewAttempts" button.
    public void OnViewAttemptsButtonClicked()
    {
        // Activate the Attempts Canvas.
        if (attemptsCanvas != null)
            attemptsCanvas.gameObject.SetActive(true);

        
        //hide menu canvas and ui canvas
        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(false);
        if (uiCanvas != null)
            uiCanvas.gameObject.SetActive(false);

        // Build the list of attempts as a string.
        string attemptsDisplay = "Previous Attempts:\n";
        for (int i = 0; i < attemptTimes.Count; i++)
        {
            attemptsDisplay += (i + 1) + ". " + attemptTimes[i].ToString("F2") + " seconds\n";
        }

        // Display the string in the AttemptsText field.
        if (attemptsText != null)
            attemptsText.text = attemptsDisplay;
        else
            Debug.LogWarning("Attempts Text is not assigned in the GameManager.");
    }

    // Called by a "Back" button on your Attempts Canvas to hide it.
    public void OnBackFromAttemptsButtonClicked()
    {
        if (attemptsCanvas != null)
            attemptsCanvas.gameObject.SetActive(false);

        if (menuCanvas != null)
            menuCanvas.gameObject.SetActive(true);
            // // hide ui canvas if needed (should be off though)
            // if (uiCanvas != null)
            // uiCanvas.gameObject.SetActive(false);

    }
}
