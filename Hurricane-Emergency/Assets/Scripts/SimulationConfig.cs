using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimulationConfig : MonoBehaviour
{
    public static SimulationConfig Instance { get; private set; }
    public int SimulationConfigId { get; private set; }


    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResetSimulatiom() // from WebGL
    {
        SceneManager.LoadScene(0);
    }
}
