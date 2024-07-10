using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainCardDeckScript : MonoBehaviour
{
    public GameManager gm;
    public Material highlight;
    private Material defaultMaterial;

    public bool disabled { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        // saving the default Material
        defaultMaterial = GetComponent<Transform>().GetChild(GetComponent<Transform>().childCount - 1).GetComponent<SpriteRenderer>().sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // if user is hovering over the deck, the top card changes its material = is highlighted
    private void OnMouseOver()
    {
        if (!disabled)
            GetComponent<Transform>().GetChild(GetComponent<Transform>().childCount - 1).GetComponent<SpriteRenderer>().sharedMaterial = highlight;
    }

    private void OnMouseExit()
    {
        if (!disabled)
            GetComponent<Transform>().GetChild(GetComponent<Transform>().childCount - 1).GetComponent<SpriteRenderer>().sharedMaterial = defaultMaterial;
    }

    private void OnMouseUp()
    {
        if (!disabled)
            gm.DrawTrainCardFromDeck();
    }
}
