using UnityEngine;

public class StackableObject : MonoBehaviour
{
    public Transform stackPoint; // Point on top of the object where the next object will be placed
    public float stackOffset = 0.5f; // Distance to offset stacked objects

    private void OnMouseDown()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            Transform otherObject = hit.transform;

            // Check if we're hitting another stackable object
            if (otherObject.CompareTag("Stackable"))
            {
                // Calculate new position for stacking
                Vector3 newPosition = otherObject.position + Vector3.up * stackOffset;
                transform.position = newPosition;
            }
        }
    }
}
