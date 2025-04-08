//using UnityEngine;

//public class ColumnTouchSimple : MonoBehaviour
//{
//    public bool isCorrectColumn = false;
//    private Renderer rend;
//    private Color defaultColor = Color.gray;

//    void Start()
//    {
//        rend = GetComponent<Renderer>();
//        rend.material.color = defaultColor;
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        Debug.Log("Touched by: " + other.name);

//        if (isCorrectColumn)
//        {
//            rend.material.color = Color.green;
//            Debug.Log("Correct column touched!");

//            // Reset color after 5 seconds
//            Invoke(nameof(ResetColor), 5f);
//        }
//        else
//        {
//            rend.material.color = Color.red;

//            // Reset wrong ones too, if you want
//            Invoke(nameof(ResetColor), 5f);
//        }
//    }

//    void ResetColor()
//    {
//        rend.material.color = defaultColor;
//    }
//}


using UnityEngine;
using System.Collections.Generic;

public class ColumnTouchSimple : MonoBehaviour
{
    public bool isCorrectColumn = false;
    private Renderer rend;
    private static List<ColumnTouchSimple> allColumns = new List<ColumnTouchSimple>();
    private static Color defaultColor = Color.gray;

    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.material.color = defaultColor;
        allColumns.Add(this);
    }

    void OnDestroy()
    {
        allColumns.Remove(this);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Touched by: " + other.name);

        if (isCorrectColumn)
        {
            rend.material.color = Color.green;
            Debug.Log("Correct column touched!");

            // Start reset for all after 5 seconds
            Invoke(nameof(ResetAllColumns), 3f);
        }
        else
        {
            rend.material.color = Color.red;
            Debug.Log("Wrong column touched.");
        }
    }

    void ResetAllColumns()
    {
        foreach (var column in allColumns)
        {
            column.rend.material.color = defaultColor;
        }

        Debug.Log("All columns reset to gray.");
    }
}
