using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTakesObjects : MonoBehaviour
{
    public List<ActivateDeactivateObject> objectsToActivateDeactivate = new List<ActivateDeactivateObject>();
    
    public GameObject tShirtHeand;
    public GameObject tShirtRoom;

    public GameObject fruitsHeand;
    public GameObject fruitsRoom;

    public GameObject flashLightHeand;
    public GameObject flashLightRoom;

    public GameObject lampHeand;
    public GameObject lampRoom;

    public GameObject toyHeand;
    public GameObject toyRoom;

    public GameObject aquriumHeand;
    public GameObject aquriumRoom;
    public GameObject waterHeand;
    public GameObject waterRoom;
    public GameObject scissorsHeand;
    public GameObject chickensHeand;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void HandleObjectTaken(string objectName)
    {
        if (objectName == "TShirt")
        {
            tShirtRoom.SetActive(false);
            tShirtHeand.SetActive(true);
        }
        if (objectName == "Fruits")
        {
            fruitsRoom.SetActive(false);
            fruitsHeand.SetActive(true);

        }
        if (objectName == "FlashLight")
        {
            flashLightRoom.SetActive(false);
            flashLightHeand.SetActive(true);
        }
        if (objectName == "Lamp")
        {
            lampRoom.SetActive(false);
            lampHeand.SetActive(true);
        }
        if (objectName == "Toy")
        {
            toyRoom.SetActive(false);
            toyHeand.SetActive(true);
        }
        if (objectName == "Aquarium")
        {
            aquriumRoom.SetActive(false);
            aquriumHeand.SetActive(true);
        }
        if (objectName == "Water")
        {
            waterRoom.SetActive(false);
            waterHeand.SetActive(true);
        }
        if (objectName == "Scissors")
        {
            scissorsHeand.SetActive(true);
        }
        if (objectName == "Chickens")
        {
            chickensHeand.SetActive(true);
        }
    }
}
