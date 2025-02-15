using UnityEngine;

public class BuildingBlock : MonoBehaviour
{
    public BlockColor blockColor; // Stores the logical color for game comparison
    public bool isSnapped = false;
    private void Start()
    {
        // Log the manually assigned color to confirm it's stored
        Debug.Log(gameObject.name + " has been assigned color: " + blockColor);
    }
}

