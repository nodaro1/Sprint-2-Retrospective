using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    public Transform snapPoint; // Optional: Custom snap position
    private bool isOccupied = false;
    public BuildingBlock currentBlock;

    private void OnTriggerEnter(Collider other)
    {
        if (!isOccupied && other.CompareTag("ColorBlock"))
        {
            // If this block is already snapped somewhere else, do nothing
            BuildingBlock block = other.GetComponent<BuildingBlock>();
            if (block != null && block.isSnapped)
                return;

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero; // Stops unnecessary spinning
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            StartCoroutine(SnapBlock(other.transform));

            isOccupied = true;
            currentBlock = block;
            if (currentBlock != null)
            {
                currentBlock.isSnapped = true; // Mark this block as placed
            }

            CheckIfRowIsFull();
        }
    }

    private System.Collections.IEnumerator SnapBlock(Transform blockTransform)
    {
        Vector3 startPosition = blockTransform.position;
        Vector3 targetPosition = (snapPoint != null) ? snapPoint.position : transform.position;
        float snapSpeed = 15f; // Faster snapping to avoid glitching

        float elapsedTime = 0f;
        while (elapsedTime < 0.15f) // Fast transition
        {
            blockTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime * snapSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        blockTransform.position = targetPosition;
    }

    private void OnTriggerExit(Collider other)
    {
        if (isOccupied && other.CompareTag("ColorBlock") && other.gameObject == currentBlock.gameObject)
        {
            ReleaseBlock(other.gameObject);
        }
    }

    private void ReleaseBlock(GameObject releasedBlock)
    {
        if (currentBlock != null && releasedBlock == currentBlock.gameObject)
        {
            Rigidbody rb = releasedBlock.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
            }

            isOccupied = false;
            if (currentBlock != null)
            {
                currentBlock.isSnapped = false; // Allow the block to snap elsewhere
            }
            currentBlock = null;
        }
    }
    
    private void CheckIfRowIsFull()
    {
        MastermindNew gameManager = FindObjectOfType<MastermindNew>();
        if (gameManager != null)
        {
            gameManager.CheckGuess();
        }
    }

}

/*using UnityEngine;

public class BoardSlot : MonoBehaviour
{
    public Transform snapPoint; // Optional: Custom snap position
    private bool isOccupied = false;
    public BuildingBlock currentBlock;

    private void OnTriggerEnter(Collider other)
    {
        if (!isOccupied && other.CompareTag("ColorBlock"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

            StartCoroutine(SnapBlock(other.transform));

            isOccupied = true;
            currentBlock = other.GetComponent<BuildingBlock>();
        }
    }

    private System.Collections.IEnumerator SnapBlock(Transform blockTransform)
    {
        Vector3 startPosition = blockTransform.position;
        Vector3 targetPosition = (snapPoint != null) ? snapPoint.position : transform.position;
        float snapSpeed = 10f;

        float elapsedTime = 0f;
        while (elapsedTime < 0.2f)
        {
            blockTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime * snapSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        blockTransform.position = targetPosition;
    }

    private void OnTriggerExit(Collider other)
    {
        if (isOccupied && other.CompareTag("ColorBlock") && other.gameObject == currentBlock.gameObject)
        {
            ReleaseBlock(other.gameObject);
        }
    }

    private void ReleaseBlock(GameObject releasedBlock)
    {
        if (currentBlock != null && releasedBlock == currentBlock.gameObject)
        {
            Rigidbody rb = releasedBlock.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.constraints = RigidbodyConstraints.None;
            }

            isOccupied = false;
            currentBlock = null;
        }
    }
}*/
