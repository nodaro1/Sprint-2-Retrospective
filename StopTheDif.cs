using UnityEngine;
using UnityEngine.UI;

public class ImageSelector : MonoBehaviour
{
    public Image image1;  // Assign the first image in the inspector
    public Image image2;  // Assign the second image in the inspector
    private Image selectedImage;

    private void Start()
    {
        // Initialize both images with no selection
        DeselectImages();
    }

    private void Update()
    {
        // Detect mouse clicks
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                // Check if the hit object is one of the images
                if (hit.collider.gameObject == image1.gameObject)
                {
                    SelectImage(image1);
                }
                else if (hit.collider.gameObject == image2.gameObject)
                {
                    SelectImage(image2);
                }
            }
        }
    }

    private void SelectImage(Image image)
    {
        DeselectImages();  // Deselect the previous selection
        selectedImage = image;  // Update the selected image
        selectedImage.color = Color.green;  // Highlight selected image in green
    }

    private void DeselectImages()
    {
        image1.color = Color.white;
        image2.color = Color.white;
        selectedImage = null;
    }
}
