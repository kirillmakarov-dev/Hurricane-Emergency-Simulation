using UnityEngine;

public interface ISimulationMode 
{
    void Initialize();
    void OnSimulationStart();
    void OnSimulationEnd();
    void Cleanup();

}
