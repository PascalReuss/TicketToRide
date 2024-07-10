using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoutepartScript : MonoBehaviour
{
    // bool for one part of the route
    public bool selected { get; set; }

    // bool for indicating if routepart is part of a double route
    public bool isPartOfDoubleRoute;

    public GameObject gameManager;
    GameManager gm;

    // parent of the route part = whole route
    Transform parent;

    int routeLength;
    string routeColor;

    public bool disabled { get; set; }

    public bool selectingRouteOption { get; set; }

    //public bool Selected { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.Find("GameManager");
        gm = gameManager.GetComponent<GameManager>();
        selected = false;

        parent = GetComponent<Transform>().parent;
        //length of the route
        routeLength = parent.transform.childCount;
        routeColor = GetComponent<MeshRenderer>().sharedMaterial.name;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /* triggered, if any part of a route is clicked
     * the code detects if the route was selected or deselected by checking the variable selected
     * afterwards is scales the route up or down
     */
    public void OnMouseDown()
    {
        // Notiz: DoubleRoute: Zwischenebene muss zum Beispiels fürs Highlightning weg, aber eventuell brauchen wir es später
        if (selectingRouteOption)
        {
            gm.SetCurrentRouteOption(parent.GetComponent<RouteScript>());
        }
        else
        {
            // only executable if route is not occupied
            if (!parent.GetComponent<RouteScript>().occupied && !disabled)
            {
                //Aufruf dient zum Testen, ob die Attribute im Routeskript richtig befüllt werden
                RouteScript routeScript = GetComponentInParent<RouteScript>();
                //routeScript.TestOutput();

                if (gm.CheckNumberOfWagons(parent.transform.childCount))
                {
                    for (int i = 0; i < parent.transform.childCount; i++)
                    {
                        parent.GetChild(i).GetComponent<RoutepartScript>().selected = !parent.GetChild(i).GetComponent<RoutepartScript>().selected;
                    }
                }

                if (selected)
                {
                    // show the confirm button, if the player is able buy the route
                    if (gm.CheckHandCardsForClaimingOfRouteWithoutJoker(routeLength, routeColor))
                    {
                        StartCoroutine(gm.StartTrainCardSelection(routeLength, routeColor, parent));
                    }
                    else if (gm.CheckHandCardsForClaimingOfRouteWithJoker(routeLength, routeColor))
                    {
                        StartCoroutine(gm.StartTrainCardSelection(routeLength, routeColor, parent));
                    }
                    else
                    {
                        for (int i = 0; i < parent.transform.childCount; i++)
                        {
                            parent.GetChild(i).GetComponent<RoutepartScript>().selected = false;
                        }
                    }
                }
                else
                {
                    parent.GetComponent<RouteScript>().highlighted = false;
                    gm.confirmHandCardsBtn.gameObject.SetActive(false);
                    gm.confirmHandCardsBtn.gameObject.GetComponent<Button>().interactable = false;
                    // enable gameobjects
                    gm.traincardsDeck.GetComponent<TrainCardDeckScript>().disabled = false;
                    gm.destinationcardsDeck.GetComponent<DestinationCardDeckScript>().disabled = false;
                    // enable all face up cards
                    for (int i = 0; i < gm.faceupTraincardsParent.transform.childCount; i++)
                        gm.faceupTraincardsParent.transform.GetChild(i).GetChild(0).GetComponent<FaceUpTrainCardScript>().disabled = false;
                    // enable all routes
                    gm.EnableAllRoutes();
                    for (int i = 0; i < gm.handcardsPanel.transform.childCount; i++)
                    {
                        gm.handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().IsEnabled = false;
                        if (gm.handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().selected)
                        {
                            gm.handcardsPanel.transform.GetChild(i).GetComponent<CardSelectScript>().selected = false;
                            gm.handcardsPanel.transform.GetChild(i).GetComponent<RectTransform>().transform.position -= new Vector3(0, 20, 0);
                        }
                    }
                }
            }
        }
    }

    public void OnMouseOver()
    {
        if (!parent.GetComponent<RouteScript>().highlighted && !parent.GetComponent<RouteScript>().occupied && !disabled)
        {
            if (gm.CheckHandCardsForClaimingOfRouteWithoutJoker(routeLength, routeColor) || gm.CheckHandCardsForClaimingOfRouteWithJoker(routeLength, routeColor))
            {
                for (int i = 0; i < parent.transform.childCount; i++)
                {
                    parent.GetChild(i).GetChild(0).gameObject.SetActive(true);
                }
            }
        }
    }

    private void OnMouseExit()
    {
        if (!parent.GetComponent<RouteScript>().highlighted && !parent.GetComponent<RouteScript>().occupied && !disabled)
        {
            if (gm.CheckHandCardsForClaimingOfRouteWithoutJoker(routeLength, routeColor) || gm.CheckHandCardsForClaimingOfRouteWithJoker(routeLength, routeColor))
            {
                for (int i = 0; i < parent.transform.childCount; i++)
                {
                    parent.GetChild(i).GetChild(0).gameObject.SetActive(false);
                }
            }
        }
    }

    // -- wurde nicht genutzt - kann vielleicht nochmal nützlich sein? --
    //public void cloneDestroy(int i)
    //{
    //    // parent of the route part = whole route
    //    Transform parent = GetComponent<Transform>().parent;
 
    //    var trains = new List<GameObject>();
        
    //    foreach (Transform child in parent.GetChild(i).transform) trains.Add(child.gameObject);
    //    trains.ForEach(child => Destroy(child));
    //}
}
