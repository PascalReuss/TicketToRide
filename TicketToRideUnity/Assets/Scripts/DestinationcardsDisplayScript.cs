using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestinationcardsDisplayScript : MonoBehaviour
{
    public GameObject checkmark;
    public bool startSelection { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!startSelection)
        {
            bool noCardSelected = true;
            for (int i = 0; i < GetComponent<Transform>().childCount; i++)
            {
                if (GetComponent<Transform>().GetChild(i).GetComponent<CardSelectScript>().getSelected())
                {
                    checkmark.GetComponent<Button>().interactable = true;
                    noCardSelected = false;
                }
            }
            if (noCardSelected)
            {
                checkmark.GetComponent<Button>().interactable = false;
            }
        } else
        {
            int counter = 0;
            for (int i = 0; i < GetComponent<Transform>().childCount; i++)
            {
                if (GetComponent<Transform>().GetChild(i).GetComponent<CardSelectScript>().getSelected())
                    counter++;
            }
            if (counter >= 2)
                checkmark.GetComponent<Button>().interactable = true;
            else
                checkmark.GetComponent<Button>().interactable = false;
        }
    }
}
