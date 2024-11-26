using UnityEngine;

public class ThrowableSphere : MonoBehaviour
{
    private Rigidbody sphereRigidbody;
    private Vector3 touchStartPosition;
    private Vector3 touchEndPosition;
    private bool isDragging = false;

    [SerializeField]
    private float throwForceMultiplier = 10f;

    void Start()
    {
        // Ensure the object has a Rigidbody component
        sphereRigidbody = GetComponent<Rigidbody>();
        if (sphereRigidbody == null)
        {
            Debug.LogError("No Rigidbody attached to the sphere!");
        }
    }

    void Update()
    {
        // Handle touch or mouse input
        if (Input.GetMouseButtonDown(0)) // Touch or mouse click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isDragging = true;
                    touchStartPosition = Input.mousePosition;
                }
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging) // Release touch or mouse click
        {
            isDragging = false;
            touchEndPosition = Input.mousePosition;

            ThrowSphere();
        }
    }

    private void ThrowSphere()
    {
        // Calculate the direction and magnitude of the throw
        Vector3 direction = touchEndPosition - touchStartPosition;
        Vector3 force = new Vector3(direction.x, Mathf.Abs(direction.y), direction.magnitude) * throwForceMultiplier;

        // Apply force to the Rigidbody
        sphereRigidbody.AddForce(force);
    }
}
