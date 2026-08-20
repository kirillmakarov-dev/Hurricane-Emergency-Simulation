using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTakesObjects : MonoBehaviour
{
    public List<ActivateDeactivateObject> objectsToActivateDeactivate = new List<ActivateDeactivateObject>();

    public void HandleObjectTaken(string objectName)
    {
        TrySetObjectActive(objectName, true);
    }

    public bool TrySetObjectActive(string objectName, bool activate)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            Debug.LogWarning("RoomTakesObjects: objectName is empty.");
            return false;
        }

        ActivateDeactivateObject entry = FindEntry(objectName);
        if (entry == null)
        {
            Debug.LogWarning($"RoomTakesObjects: object '{objectName}' was not found.");
            return false;
        }

        if (activate)
        {
            entry.ActivateObject();
        }
        else
        {
            entry.DeactivateObject();
        }

        return true;
    }

    public bool TryGetHandObject(string objectName, out GameObject handObject)
    {
        handObject = null;
        ActivateDeactivateObject entry = FindEntry(objectName);
        if (entry == null)
        {
            Debug.LogWarning($"RoomTakesObjects: object '{objectName}' was not found.");
            return false;
        }

        handObject = entry.objectInHand;
        if (handObject == null)
        {
            Debug.LogWarning($"RoomTakesObjects: hand object for '{objectName}' was not assigned.");
            return false;
        }

        return true;
    }

    private ActivateDeactivateObject FindEntry(string objectName)
    {
        for (int i = 0; i < objectsToActivateDeactivate.Count; i++)
        {
            ActivateDeactivateObject entry = objectsToActivateDeactivate[i];
            if (entry != null &&
                string.Equals(entry.objectName, objectName, StringComparison.OrdinalIgnoreCase))
            {
                return entry;
            }
        }

        return null;
    }
}
