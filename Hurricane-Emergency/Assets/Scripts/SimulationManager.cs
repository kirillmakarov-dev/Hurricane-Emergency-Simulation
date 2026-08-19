using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class SimulationManager : Singelton<SimulationManager>
{

    static bool initFirstTime = true;
    public event Action<ModeName> OnModeChanged;
    private GameModeFactory modeFactory;

    private ISimulationMode activeGameMode;

    public ModeName newMode;
    public ModeName CurrentMode { get; private set; }
    public ModeName PreviousMode { get; private set; }



    protected override void Awake()
    {
        base.Awake();
        modeFactory = new GameModeFactory();
        InitializeGameModes();
        InitSimulation();
    }
    void Start()
    {
        // // Default start mode
        // newMode = ModeName.House;
        // SwitchMode(ModeName.House);
    }

    private static void InitSimulation()
    {
        if (initFirstTime == true)
        {
            WebGLBridge.CallINITfunction();
            initFirstTime = false;
        }
        else
        {
            WebGLBridge.OnResetDone();
        }
    }



    private void InitializeGameModes()
    {
        // initialize modes
        foreach (ModeName modeName in Enum.GetValues(typeof(ModeName)))
        {
            var mode = modeFactory.GetMode(modeName, gameObject);
            mode?.Initialize();
        }
    }

    private void Update()
    {
        if (Application.isEditor)
        {
            if (CurrentMode != newMode)
            {
                SwitchMode(newMode);
            }
        }

    }

    public T GetMode<T>() where T : Component, ISimulationMode
    {
        return modeFactory.GetMode<T>(gameObject);
    }

    public void SwitchMode(ModeName newMode)
    {
        if (newMode == CurrentMode && activeGameMode != null) return;

        if (activeGameMode != null)
        {
            activeGameMode.Cleanup();
        }

        PreviousMode = CurrentMode;
        CurrentMode = newMode;

        activeGameMode = modeFactory.GetMode(newMode, gameObject);

        if (activeGameMode != null)
        {
            activeGameMode.OnSimulationStart();
        }

        OnModeChanged?.Invoke(newMode);
    }

    void OnDestroy()
    {
        if (activeGameMode != null) activeGameMode.Cleanup();
    }
    public void ResetSimulatiom() // from WebGL
    {
        SceneManager.LoadScene(0);
    }

    public void SetSimulationStateInUnity(string state) // from WebGL
    {
        switch (state)
        {
            case "PAUSED":
                AudioManager.Instance.SoundIsPlaying(false);
                Time.timeScale = 0;
                break;
            case "RUNNING":
                Time.timeScale = 1;
                AudioManager.Instance.SoundIsPlaying(true);
                break;
        }
    }
}
