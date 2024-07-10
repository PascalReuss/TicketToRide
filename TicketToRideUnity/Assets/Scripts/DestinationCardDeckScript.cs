using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestinationCardDeckScript : MonoBehaviour
{
    public GameObject X, Hand, Dimmer, Map, Checkmark;
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

    public void OnMouseUp()
    {
        if (!disabled)
        {
            gm.spritesDestinationcardsDisplay.SetActive(true);
            gm.dimmer.SetActive(true);

            gm.DisableGameObjects();
            // draw max 3 random cards (3 if 3 available)
            gm.DrawDestinationCardsFromDeck();
        }
    }
}
