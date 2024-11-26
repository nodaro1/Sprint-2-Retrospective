using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Timer Settings")]
    public TextMeshProUGUI timerText;
    private float timer;
    private bool timerRunning = false;

    [Header("Completion Settings")]
    public TextMeshProUGUI completionText;
    private bool completionMessageDisplayed = false;

    [Header("Stacking Settings")]
    public List<GameObject> blocks = new List<GameObject>();
    public float stackTolerance = 0.1f;

    void Awake()
    {
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
        HideCompletionMessage();
    }

    void Update()
    {
        if (timerRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerDisplay();
            CheckStacking();
        }
    }

    public void StartTimer(List<GameObject> spawnedBlocks)
    {
        blocks = spawnedBlocks;
        timer = 0f;
        timerRunning = true;
        UpdateTimerDisplay();
        HideCompletionMessage();
    }

    public void StopTimer()
    {
        timerRunning = false;
        Debug.Log("Timer Stopped! Time: " + timer.ToString("F2") + " seconds");
        ShowCompletionMessage();
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + timer.ToString("F2") + "s";
        }
        else
        {
            Debug.LogWarning("Timer Text is not assigned in the GameManager.");
        }
    }

    void CheckStacking()
    {
        if (blocks.Count < 3)
            return;

        var sortedBlocks = blocks.OrderBy(b => b.transform.position.y).ToList();

        bool allStacked = true;

        for (int i = 0; i < sortedBlocks.Count - 1; i++)
        {
            Vector3 lower = sortedBlocks[i].transform.position;
            Vector3 upper = sortedBlocks[i + 1].transform.position;

            if (Mathf.Abs(lower.x - upper.x) > stackTolerance || Mathf.Abs(lower.z - upper.z) > stackTolerance)
            {
                allStacked = false;
                break;
            }

            float expectedY = lower.y + sortedBlocks[i].GetComponent<Collider>().bounds.size.y;
            if (Mathf.Abs(upper.y - expectedY) > stackTolerance)
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
}
