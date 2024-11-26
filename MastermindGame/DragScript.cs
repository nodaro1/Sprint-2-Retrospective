// Script to allow cubes to lock into place on the slots on the Mastermind board

using UnityEngine;

public class MetaSnapToSlot : MonoBehaviour
{
    private Transform snapTarget; 

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Slot")) 
        {
            snapTarget = other.transform;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Slot"))
        {
            snapTarget = null;
        }
    }

    void OnDisable() 
    {
        if (snapTarget != null)
        {
            transform.position = snapTarget.position;
            transform.rotation = snapTarget.rotation;
        }
    }
}
