using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dropdownexample : MonoBehaviour
{
    public string selectedColor;
    public List<string> colors = new List<string>() { "Select Color", "Blue", "Green", "Red", "Yellow", "Purple", "Reset" };
    public Dropdown dropdown;
    public Text selectedName;
    //public String reservedName = "Reserved";
    public void Dropdown_IndexChanged(int index)
    {
        selectedName.text = colors[index];
        if (index == 0)
        {
            selectedName.color = Color.red;
        }
        else
        {
            selectedName.color = Color.black;
        }
        selectedColor = colors[index];
    }

    private void Start()
    {
        PopulateList();
    }

    private void Update()
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(colors);
    }

    void PopulateList()
    {
        dropdown.AddOptions(colors);
    }

}
