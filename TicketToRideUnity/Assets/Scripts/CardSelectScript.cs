using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSelectScript : MonoBehaviour
{
    public bool selected{ get; set;}
    public bool IsEnabled { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        if (IsEnabled)
        {
            if (selected)
            {
                GetComponent<RectTransform>().transform.position -= new Vector3(0, 20, 0);
            }
            else
            {
                GetComponent<RectTransform>().transform.position += new Vector3(0, 20, 0);
            }
            selected = !selected;
        }
    }

    public void OnClickDestinationCard()
    {
        if (selected)
        {
            GetComponent<RectTransform>().transform.position += new Vector3(0, 250, 0);
        }
        else
        {
            GetComponent<RectTransform>().transform.position -= new Vector3(0, 250, 0);
        }
        selected = !selected;
    }

    public bool getSelected ()
    {
        return selected;
    }
}
