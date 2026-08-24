using System.Collections.Generic;

public interface ISimulationMode 
{
    void Initialize();
    void OnSimulationStart();
    void OnSimulationEnd();
    void Cleanup();

}

public interface IConfiguredSequenceMode : ISimulationMode
{
    void PlayConfiguredSequence(IReadOnlyList<string> animationNames, System.Action onCompleted = null);
}
