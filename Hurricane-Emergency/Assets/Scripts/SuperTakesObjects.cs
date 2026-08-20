using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SuperMarketObjectEntry
{
    public string objectName;
    public GameObject objectInHand;
    public GameObject objectInCart;

    public void ActivateHand()
    {
        if (objectInHand != null)
        {
            objectInHand.SetActive(true);
        }

        if (objectInCart != null)
        {
            objectInCart.SetActive(false);
        }
    }

    public void ActivateCart()
    {
        if (objectInCart != null)
        {
            objectInCart.SetActive(true);
        }

        if (objectInHand != null)
        {
            objectInHand.SetActive(false);
        }
    }
}

public class SuperTakesObjects : MonoBehaviour
{
    [SerializeField] private List<SuperMarketObjectEntry> objectsToActivateDeactivate =
        new List<SuperMarketObjectEntry>();

    private string currentObject;

    public void HandleObjectTaken(string objectName)
    {
        SuperMarketObjectEntry entry = FindEntry(objectName);
        if (entry == null)
        {
            return;
        }

        entry.ActivateHand();
        currentObject = objectName;
    }

    public void HandleObjectPlaced()
    {
        SuperMarketObjectEntry entry = FindEntry(currentObject);
        if (entry == null)
        {
            return;
        }

        entry.ActivateCart();
    }

    private SuperMarketObjectEntry FindEntry(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        for (int i = 0; i < objectsToActivateDeactivate.Count; i++)
        {
            SuperMarketObjectEntry entry = objectsToActivateDeactivate[i];
            if (entry != null &&
                string.Equals(entry.objectName, objectName, StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }

        return null;
    }
}
