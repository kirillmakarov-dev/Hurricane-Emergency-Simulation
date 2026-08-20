using System;
using UnityEngine;

public static class RoomObjectActivation
{
    public static void SetActive(GameObject character, string objectName, bool activate)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            Debug.LogWarning("RoomObjectActivation: objectName is empty.");
            return;
        }

        RoomTakesObjects roomObjects = character != null
            ? character.GetComponent<RoomTakesObjects>()
            : null;
        if (roomObjects == null)
        {
            Debug.LogWarning("RoomObjectActivation: RoomTakesObjects component was not found.");
            return;
        }

        for (int i = 0; i < roomObjects.objectsToActivateDeactivate.Count; i++)
        {
            ActivateDeactivateObject entry = roomObjects.objectsToActivateDeactivate[i];
            if (entry == null ||
                !string.Equals(entry.objectName, objectName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (activate)
                entry.ActivateObject();
            else
                entry.DeactivateObject();

            return;
        }

        Debug.LogWarning($"RoomObjectActivation: object '{objectName}' was not found.");
    }
}
