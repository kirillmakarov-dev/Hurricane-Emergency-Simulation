using System;
using System.Collections.Generic;
using UnityEngine;

public class GameModeUIController : MonoBehaviour
{
    [Serializable]
    public struct ModeUIConfig
    {
        public ModeName mode;
        public List<GameObject> objectsToEnable;
    }

    [Header("Configuration")]
    [SerializeField] private List<ModeUIConfig> modeUIConfigs;

    private void Awake()
    {
        if (SimulationManager.Instance != null)
        {
            SimulationManager.Instance.OnModeChanged += HandleModeChanged;
        }
    }

    private void OnDestroy()
    {
        if (SimulationManager.Instance != null)
        {
            SimulationManager.Instance.OnModeChanged -= HandleModeChanged;
        }
    }

    private void HandleModeChanged(ModeName newMode)
    {
        // 1. Collect all objects that should be active for the new mode
        HashSet<GameObject> activeObjects = new HashSet<GameObject>();

        foreach (var config in modeUIConfigs)
        {
            if (config.mode == newMode && config.objectsToEnable.Count > 0)
            {
                foreach (var obj in config.objectsToEnable)
                {
                    if (obj != null)
                    {
                        activeObjects.Add(obj);
                    }
                }
                // We assume one config per mode, but if multiple, this aggregates them.
            }
        }

        // 2. Iterate through ALL configured objects in the entire list
        // If an object is in the 'activeObjects' set, enable it.
        // Otherwise, disable it (only if we control it).

        // We create a master set of all objects controlled by this system
        HashSet<GameObject> allControlledObjects = new HashSet<GameObject>();
        foreach (var config in modeUIConfigs)
        {
            foreach (var obj in config.objectsToEnable)
            {
                if (obj != null) allControlledObjects.Add(obj);
            }
        }

        foreach (var obj in allControlledObjects)
        {
            bool shouldBeActive = activeObjects.Contains(obj);
            if (obj.activeSelf != shouldBeActive)
            {
                obj.SetActive(shouldBeActive);
            }
        }
    }
}
