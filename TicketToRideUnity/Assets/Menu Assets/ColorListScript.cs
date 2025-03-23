using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorListScript : MonoBehaviour
{
    List<string> names = new List<string>() { "Select Color", "Blue", "Green", "Red", "Yellow", "Purple" };
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        names = new List<string>() { "Select Color", "Blue", "Green", "Red", "Yellow", "Purple", "Reset" };
        for (int i = 0; i < transform.childCount; i++)
        {
            string color = transform.GetChild(i).GetChild(3).GetChild(3).GetComponent<Text>().text;
            if (color != "Select Color")
            {
                names.Remove(color);
            }
            transform.GetChild(i).GetChild(3).GetComponent<Dropdownexample>().colors = names;
        }
    }
}
