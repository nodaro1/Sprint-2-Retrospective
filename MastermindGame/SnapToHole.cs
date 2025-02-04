using UnityEngine;

public class SnapToHole : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is a peg
        if (other.CompareTag("Peg")) // Ensure the peg has the "Peg" tag
        {
            // Snap the peg only if it is inside this trigger zone
            
            other.transform.position = transform.position; // Align the peg's position to the snap point
            other.transform.rotation = transform.rotation; // Align the peg's rotation to the snap point
            
            // Optionally, lock the peg in place
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Stops physics interactions
            }
        }
    }
}
