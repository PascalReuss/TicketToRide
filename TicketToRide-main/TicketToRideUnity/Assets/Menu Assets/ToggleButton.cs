using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButton : MonoBehaviour
{
    public Sprite toggleOffImage;
    public Sprite toggleOnImage;
    public Button button;

    public bool isAI;
    // Start is called before the first frame update
    void Start()
    {
        isAI = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeButtonImage()
    {
        if(button.image.sprite == toggleOnImage)
        {
            button.image.sprite = toggleOffImage;
            isAI = false;
        } else if(button.image.sprite == toggleOffImage)
        {
            button.image.sprite = toggleOnImage;
            isAI = true;
        }
        
    }

}
