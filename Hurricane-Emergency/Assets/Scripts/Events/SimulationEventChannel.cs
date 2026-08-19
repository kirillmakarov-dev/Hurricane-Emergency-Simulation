using System;

public readonly struct SimulationEventData
{
    public string EventName { get; }
    public Events EventType { get; }
    public bool HasKnownEventType { get; }
    public ModeName Mode { get; }
    public float Time { get; }

    public SimulationEventData(string eventName, ModeName mode, float time)
    {
        EventName = eventName;
        Mode = mode;
        Time = time;
        HasKnownEventType = Enum.TryParse(eventName, true, out Events parsedEvent);
        EventType = parsedEvent;
    }
}

public static class SimulationEventChannel
{
    public static event Action<SimulationEventData> EventRaised;

    public static void Raise(string eventName)
    {
        ModeName mode = SimulationManager.Instance != null
            ? SimulationManager.Instance.CurrentMode
            : ModeName.EntryScreen;

        EventRaised?.Invoke(new SimulationEventData(eventName, mode, UnityEngine.Time.unscaledTime));
    }
}
