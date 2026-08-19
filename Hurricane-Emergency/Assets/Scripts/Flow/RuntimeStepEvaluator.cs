using System;
using System.Collections.Generic;

public enum RuntimeStepResultType
{
    Correct,
    Incorrect,
    OutOfOrder,
    Ignored,
    Duplicate,
    LevelCompleted
}

public readonly struct RuntimeStepResult
{
    public RuntimeStepResultType Type { get; }
    public Events ReceivedEvent { get; }
    public Events? ExpectedEvent { get; }
    public int CompletedSteps { get; }
    public int TotalSteps { get; }

    public RuntimeStepResult(
        RuntimeStepResultType type,
        Events receivedEvent,
        Events? expectedEvent,
        int completedSteps,
        int totalSteps)
    {
        Type = type;
        ReceivedEvent = receivedEvent;
        ExpectedEvent = expectedEvent;
        CompletedSteps = completedSteps;
        TotalSteps = totalSteps;
    }
}

public sealed class RuntimeStepEvaluator
{
    private readonly List<Events> expectedEvents;
    private readonly HashSet<Events> completedEvents = new();
    private int currentStep;

    public int CompletedSteps => currentStep;
    public int TotalSteps => expectedEvents.Count;
    public bool IsComplete => currentStep >= expectedEvents.Count;

    public RuntimeStepEvaluator(IEnumerable<Events> events)
    {
        expectedEvents = new List<Events>(events);
    }

    public RuntimeStepResult Evaluate(Events receivedEvent)
    {
        if (IsComplete)
        {
            return Result(RuntimeStepResultType.Ignored, receivedEvent, null);
        }

        Events expectedEvent = expectedEvents[currentStep];

        if (completedEvents.Contains(receivedEvent))
        {
            return Result(RuntimeStepResultType.Duplicate, receivedEvent, expectedEvent);
        }

        if (receivedEvent == expectedEvent)
        {
            completedEvents.Add(receivedEvent);
            currentStep++;
            RuntimeStepResultType type = IsComplete
                ? RuntimeStepResultType.LevelCompleted
                : RuntimeStepResultType.Correct;
            Events? nextExpected = IsComplete ? null : expectedEvents[currentStep];
            return Result(type, receivedEvent, nextExpected);
        }

        if (receivedEvent == Events.Empty)
        {
            return Result(RuntimeStepResultType.Incorrect, receivedEvent, expectedEvent);
        }

        if (IndexOf(receivedEvent, currentStep + 1) >= 0)
        {
            return Result(RuntimeStepResultType.OutOfOrder, receivedEvent, expectedEvent);
        }

        return Result(RuntimeStepResultType.Ignored, receivedEvent, expectedEvent);
    }

    private RuntimeStepResult Result(RuntimeStepResultType type, Events received, Events? expected)
    {
        return new RuntimeStepResult(type, received, expected, currentStep, expectedEvents.Count);
    }

    private int IndexOf(Events target, int startIndex)
    {
        for (int i = startIndex; i < expectedEvents.Count; i++)
        {
            if (expectedEvents[i] == target)
            {
                return i;
            }
        }

        return -1;
    }
}

public sealed class LevelSessionController : IDisposable
{
    private readonly ModeName mode;
    private readonly RuntimeStepEvaluator evaluator;
    private bool isRunning;

    public event Action<RuntimeStepResult> StepEvaluated;

    public int CompletedSteps => evaluator.CompletedSteps;
    public int TotalSteps => evaluator.TotalSteps;

    public LevelSessionController(ModeName mode, IEnumerable<Events> expectedEvents)
    {
        this.mode = mode;
        evaluator = new RuntimeStepEvaluator(expectedEvents);
    }

    public void Start()
    {
        if (isRunning)
        {
            return;
        }

        isRunning = true;
        SimulationEventChannel.EventRaised += HandleSimulationEvent;
    }

    public void Dispose()
    {
        if (!isRunning)
        {
            return;
        }

        isRunning = false;
        SimulationEventChannel.EventRaised -= HandleSimulationEvent;
    }

    private void HandleSimulationEvent(SimulationEventData eventData)
    {
        if (!isRunning || eventData.Mode != mode || !eventData.HasKnownEventType)
        {
            return;
        }

        RuntimeStepResult result = evaluator.Evaluate(eventData.EventType);
        StepEvaluated?.Invoke(result);

        if (result.Type == RuntimeStepResultType.LevelCompleted)
        {
            Dispose();
        }
    }
}
