using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteScript : MonoBehaviour
{
    public bool isDoubleRoute;
    public bool highlighted;

    public bool occupied { get; set; }
    public PlayerScript owner { get; set; }
    public string[] cities { get; set; }
    public int routeLength { get; set; }
    public string routeColor { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        highlighted = false;
        occupied = false;

        routeLength = transform.childCount;
        cities = gameObject.name.Split('_');
        routeColor = transform.GetChild(0).GetComponent<MeshRenderer>().sharedMaterial.name;

        owner = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void TestOutput()
    {
        Debug.Log("occupied:  " + occupied);
        Debug.Log("owner:  " + owner);
        Debug.Log("city 1:  " + cities[0]);
        Debug.Log("city 2:  " + cities[1]);
        Debug.Log("routeLength:  " + routeLength);
        Debug.Log("routeColor:  " + routeColor);
    }

}
