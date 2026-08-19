using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public enum TakesObjectType
{
    Toys,
    Ball,
    Bicycle,
    Flowers,
}
[Serializable]
public class TakesObjects
{
    public GameObject objectTotake;
    public GameObject heandObject;
    public TakesObjectType objectType;
}



public class GardenTakesObjects : MonoBehaviour
{

    public bool isKyelan;
    public List<TakesObjects> takesObjects;

    [SerializeField] private GameObject heandObjectToRemove;

    public void HandleObjectTaken(string objectName) //called from animatorion event
    {

        TakesObjects takenObject = takesObjects.FirstOrDefault(obj => obj.objectType.ToString() == objectName);
        if (takenObject == null)
        {
            Debug.LogError("No matching object found for objectName: " + objectName);
            return;
        }
        Debug.Log("HandleObjectTaken called with objectName: " + objectName);

        takenObject.objectTotake.SetActive(false);
        switch (takenObject.objectType)
        {
            case TakesObjectType.Toys:
                WebGLBridge.SendEvent(Events.GetToys.ToString());
                break;
            case TakesObjectType.Ball:
                WebGLBridge.SendEvent(Events.GetBall.ToString());
                break;
            case TakesObjectType.Bicycle:
                WebGLBridge.SendEvent(Events.GetBicycle.ToString());
                break;
            default:
                Debug.LogError("Unhandled object type: " + takenObject.objectType);
                break;
        }
 

        takenObject.heandObject.SetActive(true);
        heandObjectToRemove = takenObject.heandObject;
        if (isKyelan)
        {
            GardenViewMode.Instance.onKyelanAnimationComplete += RemoveObjectFromHand;
        }
        else
        {
            GardenViewMode.Instance.onKeyAnimationComplete += RemoveObjectFromHand;
        }
    }

    public void RemoveObjectFromHand() //called from animatorion event
    {
        if (heandObjectToRemove != null)
        {
            heandObjectToRemove.SetActive(false);
            heandObjectToRemove = null;
        }
    }
}
