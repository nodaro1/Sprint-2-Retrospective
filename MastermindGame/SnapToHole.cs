using UnityEngine;

public class SnapToHole : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is a peg
        if (other.CompareTag("Peg")) 
        {
            other.transform.position = transform.position; 
            other.transform.rotation = transform.rotation; 
            
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Stops physics interactions
            }
        }
    }
}
