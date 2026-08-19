using UnityEngine;

public enum Events
{
    ReviewEmergencyPlan,
    RadioBroadcast,
    CheckGoBag,
    CleanYard,
    CollectPlywood,
    JuneFirst,
    GoToSupermarket,
    GetCannedFood,
    GetWater,
    GetCrackers,
    HurricaneWatch,
    HurricaneWarning,
    PackClothes,
    PackToys,
    PackWater,
    PackFlashlight,
    MayArrives,
    Empty,
    CoverWindow,
    GetBicycle,
    GetToys,
    GetBall,
    ColorBook,
    PlayToy,
    CutBranches,
    PickGlass,
    PickBottles,
    PickBranches,
    AllClear,
    GobagReminder,// new fromhere
    PackBook,
    PackCrackers,
    PackCannedFood,
    PackFirstAid,
    PackToothbrush,
    PackWipes,
    PackSoap,
}
public class EventsManager : MonoBehaviour
{
    void Start()
    {
        // ...existing code...
    }

    public void ReviewEmergencyPlan() // from Unity to WebGL - called from animation event
    {
        WebGLBridge.SendEvent(Events.ReviewEmergencyPlan.ToString());
    }

    public void RadioBroadcast()
    {
        WebGLBridge.SendEvent(Events.RadioBroadcast.ToString());
    }

    public void CheckGoBag()
    {
        WebGLBridge.SendEvent(Events.CheckGoBag.ToString());
    }

    public void CleanYard()
    {
        WebGLBridge.SendEvent(Events.CleanYard.ToString());
    }

    public void CollectPlywood()
    {
        WebGLBridge.SendEvent(Events.CollectPlywood.ToString());
    }

    public void JuneFirst()
    {
        WebGLBridge.SendEvent(Events.JuneFirst.ToString());
    }

    public void GoToSupermarket()
    {
        WebGLBridge.SendEvent(Events.GoToSupermarket.ToString());
    }

    public void GetCannedFood()
    {
        WebGLBridge.SendEvent(Events.GetCannedFood.ToString());
    }

    public void GetWater()
    {
        WebGLBridge.SendEvent(Events.GetWater.ToString());
    }

    public void GetCrackers()
    {
        WebGLBridge.SendEvent(Events.GetCrackers.ToString());
    }

    public void HurricaneWatch()
    {
        WebGLBridge.SendEvent(Events.HurricaneWatch.ToString());
    }

    public void PackClothes()
    {
        WebGLBridge.SendEvent(Events.PackClothes.ToString());
    }

    public void PackToys()
    {
        WebGLBridge.SendEvent(Events.PackToys.ToString());
    }

    public void PackWater()
    {
        WebGLBridge.SendEvent(Events.PackWater.ToString());
    }

    public void PackFlashlight()
    {
        WebGLBridge.SendEvent(Events.PackFlashlight.ToString());
    }
}
