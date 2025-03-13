using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceUpTrainCardScript : MonoBehaviour
{
    public GameObject gm;

    public Material highlight;
    private Material defaultMaterial;
    new SpriteRenderer renderer;

    public bool disabled { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager");
        // get Component SpriteRenderer
        renderer = transform.GetComponent<SpriteRenderer>();
        // saving the default Material
        defaultMaterial = renderer.sharedMaterial;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // if user is hovering over the deck, the top card changes its material = is highlighted
    private void OnMouseOver()
    {
        if (!disabled)
            renderer.sharedMaterial = highlight;
    }

    private void OnMouseExit()
    {
        if (!disabled)
            renderer.sharedMaterial = defaultMaterial;
    }

    private void OnMouseUp()
    {
        if (!disabled)
            gm.GetComponent<GameManager>().DrawTrainCardFromOpenCards(this);
    }
}
