using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLBridge
{

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] public static extern void CallINITfunction();// Declaration of the external JavaScript function
    [DllImport("__Internal")] public static extern void OnResetDone();
    [DllImport("__Internal")] public static extern void SendEvent(string eventName);
    [DllImport("__Internal")] public static extern void OnJuneArrives(int juneID);
    [DllImport("__Internal")] public static extern void OnMayArrives(int mayID);
    [DllImport("__Internal")] public static extern void OnInShelter(int kayID);
    [DllImport("__Internal")] public static extern void HurricaneWatchOnAnnounced( int hurricaneWatchID);
    [DllImport("__Internal")] public static extern void HurricaneWarningOnAnnounced( int hurricaneWarningID);
    [DllImport("__Internal")] public static extern void AllClear( int allClearID);
    [DllImport("__Internal")] public static extern void GivesReminder( int kelanParentsID);

    
    

#else
    public static void CallINITfunction()
    {
        Debug.Log("callTNITfunction called - not in WebGL build, so no action taken.");
    }
    public static void OnResetDone()
    {
        Debug.Log("OnResetDone called - not in WebGL build, so no action taken.");
    }
    public static void SendEvent(string eventName)
    {
        Debug.Log("SendEvent called with eventName: " + eventName + " - not in WebGL build, so no action taken.");
    }
    public static void OnJuneArrives(int juneID)
    {
        Debug.Log("OnJuneArrives called with juneID: " + juneID + " - not in WebGL build, so no action taken.");
    }
    public static void OnMayArrives(int mayID)
    {
        Debug.Log("OnMayArrives called with mayID: " + mayID + " - not in WebGL build, so no action taken.");
    }
    public static void HurricaneWatchOnAnnounced(int hurricaneWatchID)
    {
        Debug.Log("HurricaneWatchOnAnnounced called with hurricaneWatchID: " + hurricaneWatchID + " - not in WebGL build, so no action taken.");
    }
    public static void HurricaneWarningOnAnnounced(int hurricaneWarningID)
    {
        Debug.Log("HurricaneWarningOnAnnounced called with hurricaneWarningID: " + hurricaneWarningID + " - not in WebGL build, so no action taken.");
    }
    public static void OnInShelter(int kayID)
    {
        Debug.Log("OnInShelter called with kayID: " + kayID + " - not in WebGL build, so no action taken.");
    }
    public static void AllClear(int allClearID)
    {
        Debug.Log("AllClear called with allClearID: " + allClearID + " - not in WebGL build, so no action taken.");
    }
    public static void GivesReminder(int kelanParentsID)
    {
        Debug.Log("AllClear called with kelanParentsID: " + kelanParentsID + " - not in WebGL build, so no action taken.");
    }

#endif
}
