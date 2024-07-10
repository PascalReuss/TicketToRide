using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TrainFigScript : MonoBehaviour
{
    
    public GameObject gm;
    public GameObject cloneTrainContainer;
    //public GameObject trainfig;
    public bool created = false;

    // Start is called before the first frame update
    void Start()
    {
        gm = GameObject.Find("GameManager");
        cloneTrainContainer = GameObject.Find("container");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnMouseOver()   
    //{
    //    Transform parent = GetComponent<Transform>().parent;

    //    if (!created)
    //    {
    //        for (int i = 0; i < parent.transform.childCount; i++)
    //        {
    //            SpawnTrain(i);

    //        }
    //    }
    //}

    //private void OnMouseExit()
    //{
    //    Transform parent = GetComponent<Transform>().parent;
    //    cloneDestroy();
    //}


    private void spawnTrain(int i)
    {
        Transform parent = GetComponent<Transform>().parent;
        Quaternion rote;

        Vector3 trainPos = new Vector3(parent.GetChild(i).GetComponent<Transform>().transform.position.x,
            parent.GetChild(i).GetComponent<Transform>().transform.position.y + 0.1f,
            parent.GetChild(i).GetComponent<Transform>().transform.position.z);

        Vector3 trainRot = new Vector3(parent.GetChild(i).GetComponent<Transform>().transform.eulerAngles.x,
            parent.GetChild(i).GetComponent<Transform>().transform.eulerAngles.y + 90,
            parent.GetChild(i).GetComponent<Transform>().transform.eulerAngles.z);
        rote = Quaternion.Euler(trainRot);

        GameObject trainClone = Instantiate(cloneTrainContainer,trainPos,rote);

        trainClone.transform.parent = cloneTrainContainer.transform;
        //currentMat = trainClone.GetComponent<Renderer>().material;
    }

    public void cloneDestroy()
    {
        var trains = new List<GameObject>();
        foreach (Transform child in cloneTrainContainer.transform) trains.Add(child.gameObject);
        trains.ForEach(child => Destroy(child));
    }

    
}



