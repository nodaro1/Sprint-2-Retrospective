using UnityEngine;
using TMPro;
using System.Collections; // Required for Coroutine
using System.Collections.Generic;
using System.Linq;
// using UnityEngine.EventSystems; // Removed as we are not checking EventSystem anymore

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI References")]
    public Canvas menuCanvas;
    public Canvas uiCanvas;
    public Canvas attemptsCanvas;
    public Canvas previewCanvas; // <<< ADDED: Reference for the Preview Canvas

    public TextMeshProUGUI timerText;
    public TextMeshProUGUI completionText; // Assumed child of uiCanvas or its own simple setup
    public TextMeshProUGUI attemptsText;   // Assumed child of attemptsCanvas
    public TextMeshProUGUI previewIndicatorText; // <<< KEPT: Optional text (e.g., on previewCanvas)

    [Header("Timer Settings")]
    private float timer;
    private bool timerRunning = false;

    [Header("Completion Settings")]
    private bool completionMessageDisplayed = false;
    [Tooltip("Tag assigned to the Cone prefab (e.g., Finish).")]
    public string coneTag = "Finish";
    [Tooltip("Tag assigned to the Block/Cube prefab (e.g., Respawn).")]
    public string blockTag = "Respawn";
    [Tooltip("Tag assigned to the Cylinder prefab (e.g., EditorOnly).")]
    public string cylinderTag = "EditorOnly";

    [Header("Stacking Settings")]
    public List<GameObject> shapesToStack = new List<GameObject>();
    public float stackTolerance = 0.01f;
    private float stationaryTime = 0f;
    public float requiredStationaryTime = 2f;
    public float movementThreshold = 0.001f;
    private Dictionary<GameObject, Vector3> lastPositions = new Dictionary<GameObject, Vector3>();

    [Header("Spawner Reference")]
    public BlockSpawner blockSpawner; // Assumes BlockSpawner has SpawnSpecificShapes method

    [Header("Attempt Times")]
    private List<float> attemptTimes = new List<float>();
    public List<float> AttemptTimes => attemptTimes;

    [Header("Preview Settings")]
    public Transform previewSpawnPoint;
    public float previewDuration = 10f;
    public float previewGap = 0.02f;
    public float previewScaleMultiplier = 1.0f;

    private List<GameObject> previewObjects = new List<GameObject>();
    private List<GameObject> currentTargetStackDefinition = new List<GameObject>();
    private Coroutine previewCoroutine = null; // Only coroutine for the preview itself


    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        // Validation checks
        if (string.IsNullOrEmpty(coneTag)) Debug.LogError("Cone Tag missing!", this);
        if (string.IsNullOrEmpty(blockTag)) Debug.LogError("Block Tag missing!", this);
        if (string.IsNullOrEmpty(cylinderTag)) Debug.LogError("Cylinder Tag missing!", this);
        if (blockSpawner == null) Debug.LogError("BlockSpawner not assigned!", this);
        if (previewSpawnPoint == null) Debug.LogError("Preview Spawn Point not assigned!", this);
        if (blockSpawner != null && (blockSpawner.blockPrefab == null || blockSpawner.cylinderPrefab == null || blockSpawner.conePrefab == null))
            Debug.LogError("Shape prefabs missing in BlockSpawner!", blockSpawner);
        if (previewCanvas == null) Debug.LogWarning("Preview Canvas reference is not set in the Inspector.", this); // Added check

        // Initialize UI state using simple SetActive
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true);
        if (uiCanvas != null) uiCanvas.gameObject.SetActive(false);
        if (attemptsCanvas != null) attemptsCanvas.gameObject.SetActive(false);
        if (previewCanvas != null) previewCanvas.gameObject.SetActive(false); // Ensure preview canvas starts hidden
        HideCompletionMessage();
        // Keep previewIndicatorText management if you still want it (e.g., as child of previewCanvas)
        if (previewIndicatorText != null) previewIndicatorText.gameObject.SetActive(false);
    }

    void Update()
    {
        // (Update logic for timer and movement check - Unchanged from your NEW CODE base)
        if (timerRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerDisplay();
            if (shapesToStack.Count > 0)
            {
                CheckMovementAndStacking();
            }
        }
    }

    void CheckMovementAndStacking()
    {
        // (Movement checking logic - Unchanged from your NEW CODE base)
       bool anyShapeMoved = false;
       List<GameObject> currentShapes = new List<GameObject>(shapesToStack);
       List<GameObject> objectsToRemove = new List<GameObject>();
       foreach (var obj in currentShapes) {
            if (obj == null) { objectsToRemove.Add(obj); continue; };
            Vector3 currentPos = obj.transform.position;
            if (lastPositions.TryGetValue(obj, out Vector3 lastPos)) {
                if (Vector3.Distance(currentPos, lastPos) > movementThreshold) { anyShapeMoved = true; lastPositions[obj] = currentPos; }
                else { lastPositions[obj] = currentPos; }
            } else { lastPositions[obj] = currentPos; anyShapeMoved = true; }
       }
       foreach (var objToRemove in objectsToRemove) { shapesToStack.Remove(objToRemove); lastPositions.Remove(objToRemove); }

       if (anyShapeMoved) { stationaryTime = 0f; }
       else { stationaryTime += Time.deltaTime; }

       if (stationaryTime >= requiredStationaryTime && !completionMessageDisplayed && shapesToStack.Count >= currentTargetStackDefinition.Count && shapesToStack.Count > 0) { CheckStacking(); }
       else if (stationaryTime >= requiredStationaryTime && !completionMessageDisplayed && shapesToStack.Count == 1 && currentTargetStackDefinition.Count == 1) { CheckStacking(); }
    }

    // --- MODIFIED OnStartButtonClicked ---
    public void OnStartButtonClicked()
    {
        // Validation
        if (blockSpawner == null || previewSpawnPoint == null || blockSpawner.blockPrefab == null || blockSpawner.cylinderPrefab == null || blockSpawner.conePrefab == null) {
           Debug.LogError("Cannot start game. Check assignments/Prefabs."); return;
        }

        // Stop previous preview coroutine if running
        if (previewCoroutine != null) {
            StopCoroutine(previewCoroutine);
            previewCoroutine = null;
            ClearPreview(); // Clean up 3D objects if stopped mid-preview
        }

        // 1. Determine Target
        currentTargetStackDefinition = DetermineTargetStackOrder();

        // 2. Prepare UI: Hide others, show Preview Canvas
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
        if (uiCanvas != null) uiCanvas.gameObject.SetActive(false);
        if (attemptsCanvas != null) attemptsCanvas.gameObject.SetActive(false);
        if (previewCanvas != null) previewCanvas.gameObject.SetActive(true); // <<< SHOW PREVIEW CANVAS
        // Optional: Handle preview indicator text
        if (previewIndicatorText != null) {
            previewIndicatorText.text = "Previewing Target Stack..."; // Example text
            previewIndicatorText.gameObject.SetActive(true);
        }

        // 3. Start Preview Coroutine
        Debug.Log("Starting Preview Coroutine...");
        previewCoroutine = StartCoroutine(ShowPreviewAndStartGame(currentTargetStackDefinition));
    }


    private List<GameObject> DetermineTargetStackOrder()
    {
        // (Logic for determining stack structure - Unchanged from your NEW CODE base)
       List<GameObject> targetOrder = new List<GameObject>();
       int totalShapes = blockSpawner.totalNumberOfShapes;
       bool addCone = Random.value < blockSpawner.spawnConeChance && totalShapes >= 1;
       int baseShapesCount = addCone ? totalShapes - 1 : totalShapes;
       if (baseShapesCount < 0) baseShapesCount = 0;
       for (int i = 0; i < baseShapesCount; i++) { targetOrder.Add(Random.value < 0.5f ? blockSpawner.blockPrefab : blockSpawner.cylinderPrefab); }
       if (addCone) { targetOrder.Add(blockSpawner.conePrefab); }
       Debug.Log($"[GameManager] Target stack: {string.Join(", ", targetOrder.Select(p => p.name))}");
       return targetOrder;
    }

    // --- MODIFIED ShowPreviewAndStartGame Coroutine ---
    private IEnumerator ShowPreviewAndStartGame(List<GameObject> targetOrder)
    {
        // 1. Display 3D Preview Objects
        DisplayStackPreview(targetOrder); // (This method remains unchanged)

        // 2. Wait for duration
        yield return new WaitForSeconds(previewDuration);

        Debug.Log("Preview duration finished.");

        // 3. Clean up 3D Preview Objects
        ClearPreview(); // (This method remains unchanged)

        // 4. Switch Canvases: Hide Preview, Show Game UI
        if (previewCanvas != null) previewCanvas.gameObject.SetActive(false); // <<< HIDE PREVIEW CANVAS
        if (previewIndicatorText != null) previewIndicatorText.gameObject.SetActive(false); // Hide text too
        if (uiCanvas != null) uiCanvas.gameObject.SetActive(true); // <<< SHOW GAME UI CANVAS

        // 5. Spawn Shapes for Player
        if (blockSpawner != null) {
           // Assumes BlockSpawner has SpawnSpecificShapes correctly implemented
           List<GameObject> spawnedPlayerShapes = blockSpawner.SpawnSpecificShapes(targetOrder);

            // Handle potential spawn failure
           if (spawnedPlayerShapes == null || spawnedPlayerShapes.Count == 0) {
                Debug.LogError("BlockSpawner failed to spawn specific shapes. Returning to menu.");
                // Need to ensure menu is visible if spawning fails
                 if (uiCanvas != null) uiCanvas.gameObject.SetActive(false); // Hide UI canvas again
                 ReturnToMenu(); // Go back to menu state
                 yield break; // Exit coroutine
           }

           // 6. Start the actual attempt
           StartNewAttempt(spawnedPlayerShapes); // (This method remains unchanged)

        } else {
            Debug.LogError("BlockSpawner reference missing after preview!");
             if (uiCanvas != null) uiCanvas.gameObject.SetActive(false); // Hide UI canvas again
             ReturnToMenu(); // Go back to menu state if spawner is missing
        }

        previewCoroutine = null; // Mark coroutine as finished
    }


    private void DisplayStackPreview(List<GameObject> targetOrder) { /* (Unchanged) */ ClearPreview(); if (previewSpawnPoint == null) { Debug.LogError("Preview Spawn Point is null."); return; } if (targetOrder == null || targetOrder.Count == 0) { Debug.LogWarning("Target order empty for preview."); return; } float currentY = previewSpawnPoint.position.y; Vector3 basePosition = previewSpawnPoint.position; for (int i = 0; i < targetOrder.Count; i++) { GameObject prefab = targetOrder[i]; if (prefab == null) continue; float objectHeight = GetObjectHeight(prefab); objectHeight *= previewScaleMultiplier; float yOffset = objectHeight / 2f; Vector3 spawnPosition = new Vector3(basePosition.x, currentY + yOffset, basePosition.z); GameObject previewInstance = Instantiate(prefab, spawnPosition, previewSpawnPoint.rotation); previewInstance.transform.localScale = prefab.transform.localScale * previewScaleMultiplier; Rigidbody rb = previewInstance.GetComponent<Rigidbody>(); if (rb != null) { rb.useGravity = false; rb.isKinematic = true; } else { Debug.LogWarning($"Preview {previewInstance.name} missing Rigidbody.", previewInstance); } previewObjects.Add(previewInstance); currentY += objectHeight + previewGap; } }
    private float GetObjectHeight(GameObject obj) { /* (Unchanged) */ if (obj == null) return 0f; float h=0f; Collider c=obj.GetComponent<Collider>(); if (c != null) { Vector3 s=obj.transform.localScale; if(c is BoxCollider b)h=b.size.y*s.y; else if(c is CapsuleCollider p)h=p.height*s.y; else if(c is SphereCollider r)h=r.radius*2*s.y; else if(c is MeshCollider m && m.sharedMesh!=null)h=m.sharedMesh.bounds.size.y*s.y; else h=0.1f; } else { h=0.1f; Debug.LogWarning($"{obj.name} no Collider."); } return h; }
    private void ClearPreview() { /* (Unchanged) */ foreach (GameObject obj in previewObjects) { if (obj != null) Destroy(obj); } previewObjects.Clear(); }

    public void StartNewAttempt(List<GameObject> spawnedObjects) { /* (Unchanged - Initializes game state) */ if (previewObjects.Count > 0) ClearPreview(); shapesToStack = new List<GameObject>(spawnedObjects.Where(o => o != null).ToList()); timer = 0f; timerRunning = true; UpdateTimerDisplay(); HideCompletionMessage(); lastPositions.Clear(); stationaryTime = 0f; if (shapesToStack != null) { foreach (var obj in shapesToStack) { lastPositions[obj] = obj.transform.position; Rigidbody rb = obj.GetComponent<Rigidbody>(); if (rb != null) { rb.isKinematic = false; rb.useGravity = true; } } } Debug.Log($"[GameManager] Starting attempt: {shapesToStack.Count} shapes."); }

    // --- MODIFIED StopTimerAndComplete ---
    public void StopTimerAndComplete()
    {
        if (!timerRunning) return; // Prevent multiple calls
        timerRunning = false;
        Debug.Log("Stacking Complete! Time: " + timer.ToString("F2"));
        // Ensure list is clean before adding time
        shapesToStack.RemoveAll(item => item == null);
        attemptTimes.Add(timer);
        ShowCompletionMessage(); // Show message first

        // --- Directly return to menu, no delay ---
        ReturnToMenu();
    }


    void UpdateTimerDisplay() { /* (Unchanged) */ if (timerText != null) timerText.text = "Time: " + timer.ToString("F2") + "s"; }

    void CheckStacking() { /* (Unchanged - Relies on helpers) */ shapesToStack.RemoveAll(item=>item==null); if(shapesToStack.Count==0 || currentTargetStackDefinition.Count==0) return; if(shapesToStack.Count==1 && currentTargetStackDefinition.Count==1){if(DoObjectsMatch(shapesToStack[0], currentTargetStackDefinition[0])){Debug.Log("Single object OK."); StopTimerAndComplete();} return;} if(shapesToStack.Count<2) return; var sorted=shapesToStack.OrderBy(b=>b.transform.position.y).ToList(); bool pairOK=true; for(int i=0;i<sorted.Count-1;i++){GameObject l=sorted[i]; GameObject u=sorted[i+1]; if(l==null||u==null){pairOK=false; break;} Collider lc=l.GetComponent<Collider>(); Collider uc=u.GetComponent<Collider>(); if(lc==null||uc==null){Debug.LogError($"Collider missing.", l??u); pairOK=false; break;} float ly=lc.bounds.max.y; float uy=uc.bounds.min.y; Vector3 lcx=lc.bounds.center; Vector3 ucx=uc.bounds.center; if(Mathf.Abs(lcx.x-ucx.x)>stackTolerance || Mathf.Abs(lcx.z-ucx.z)>stackTolerance){pairOK=false; break;} if(Mathf.Abs(uy-ly)>stackTolerance){pairOK=false; break;}} if(!pairOK) return; bool structureOK = CheckStackStructureAgainstTarget(sorted); if(pairOK && structureOK){Debug.Log("Stack OK!"); StopTimerAndComplete();} }
    private bool CheckStackStructureAgainstTarget(List<GameObject> actual) { /* (Unchanged) */ if (actual.Count != currentTargetStackDefinition.Count) return false; for (int i = 0; i < actual.Count; i++) { if (!DoObjectsMatch(actual[i], currentTargetStackDefinition[i])) return false; } return true; }
    private bool DoObjectsMatch(GameObject instance, GameObject targetPrefab) { /* (Unchanged - Uses Tags) */ if (instance == null || targetPrefab == null) return false; if (targetPrefab == blockSpawner.blockPrefab) return instance.CompareTag(blockTag); else if (targetPrefab == blockSpawner.cylinderPrefab) return instance.CompareTag(cylinderTag); else if (targetPrefab == blockSpawner.conePrefab) return instance.CompareTag(coneTag); else { Debug.LogWarning($"Unknown prefab type: {targetPrefab.name}"); return false; } }

    void ShowCompletionMessage() { /* (Unchanged - Simple text activation) */ if (completionText != null) { completionText.text = $"Stack Complete!\nTime: {timer:F2}s"; completionText.gameObject.SetActive(true); completionMessageDisplayed = true; Debug.Log("Completion Message Shown."); } else Debug.LogWarning("Completion Text not assigned."); }
    void HideCompletionMessage() { /* (Unchanged - Simple text deactivation) */ if (completionText != null) { completionText.gameObject.SetActive(false); } completionMessageDisplayed = false; } // Always reset flag

    // --- MODIFIED ReturnToMenu ---
    public void ReturnToMenu()
    {
        Debug.Log("ReturnToMenu called.");
        // Stop game logic/state
        timerRunning = false;
        // If ReturnToMenu is called while preview is running, stop it
        if (previewCoroutine != null) {
             StopCoroutine(previewCoroutine);
             previewCoroutine = null;
             ClearPreview(); // Clean up 3D objects
        }

        // Cleanup game objects
        if (blockSpawner != null) blockSpawner.ClearSpawnedObjects();
        shapesToStack.Clear();
        lastPositions.Clear();
        currentTargetStackDefinition.Clear();

        // Reset UI state using simple SetActive
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true); // <<< SHOW MENU
        if (uiCanvas != null) uiCanvas.gameObject.SetActive(false);
        if (attemptsCanvas != null) attemptsCanvas.gameObject.SetActive(false);
        if (previewCanvas != null) previewCanvas.gameObject.SetActive(false); // <<< HIDE PREVIEW

        // Ensure completion message is hidden
        HideCompletionMessage();
        if (previewIndicatorText != null) previewIndicatorText.gameObject.SetActive(false); // Hide text too

        // Reset state flags
        completionMessageDisplayed = false;
        stationaryTime = 0f;
        Debug.Log("ReturnToMenu finished.");
    }

    // --- MODIFIED OnViewAttemptsButtonClicked ---
    public void OnViewAttemptsButtonClicked()
    {
       Debug.Log("OnViewAttemptsButtonClicked called.");
       // Switch canvases using simple SetActive
       if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
       if (uiCanvas != null) uiCanvas.gameObject.SetActive(false);
       if (previewCanvas != null) previewCanvas.gameObject.SetActive(false); // Ensure preview hidden
       if (attemptsCanvas != null) attemptsCanvas.gameObject.SetActive(true); // <<< SHOW ATTEMPTS

        // Hide completion message just in case
        HideCompletionMessage();
        if (previewIndicatorText != null) previewIndicatorText.gameObject.SetActive(false);


       // Update attempts text (existing logic)
       if (attemptsText != null) {
            string attemptsDisplay = "Previous Attempts:\n";
            if (attemptTimes.Count == 0) { attemptsDisplay += "No attempts recorded yet."; }
            else { for (int i = 0; i < attemptTimes.Count; i++) { attemptsDisplay += $"{i + 1}. {attemptTimes[i]:F2} seconds\n"; } }
            attemptsText.text = attemptsDisplay;
       } else Debug.LogWarning("Attempts Text is not assigned in the GameManager.");
    }

    // --- MODIFIED OnBackFromAttemptsButtonClicked ---
    public void OnBackFromAttemptsButtonClicked()
    {
        Debug.Log("OnBackFromAttemptsButtonClicked called.");
        // No complex logic needed, just return to the menu state
        ReturnToMenu();
    }
}
