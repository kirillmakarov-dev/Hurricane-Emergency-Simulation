using UnityEngine;

public class ObjectsHolder : MonoBehaviour
{
    public static ObjectsHolder instance;
    private int radioID = -1;
    private int june1ID = -1;
    private int mayID = -1;
    private int kayID = -1;
    private int currentSceneIndex = 1;
    private int hurricaneWatchID = -1;
    private int hurricaneWarningID = -1;

    public int allClearID = -1;

    public int kelanParentsID = -1;


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }
    void Start()
    {
        // SetScineIndex(2);
    }
    public void SetScineIndex(int index) //called from WebGL
    {
        currentSceneIndex = index;
        switch (currentSceneIndex)
        {
            case 1:
                SimulationManager.Instance.SwitchMode(ModeName.House);
                break;
            case 2:
                SimulationManager.Instance.SwitchMode(ModeName.ClearingGarden);
                break;
            case 4:
                SimulationManager.Instance.SwitchMode(ModeName.SuperMarket);
                break;
            case 5:
                SimulationManager.Instance.SwitchMode(ModeName.ChildrenRoom);
                break;
            case 6:
                SimulationManager.Instance.SwitchMode(ModeName.GardenView);
                break;
            case 7:
                SimulationManager.Instance.SwitchMode(ModeName.Shelter);
                break;
            case 8:
                SimulationManager.Instance.SwitchMode(ModeName.AfterTheHurricane);
                break;
            case 9:
                SimulationManager.Instance.SwitchMode(ModeName.GoBagLesson);
                break;
            case 10:
                SimulationManager.Instance.SwitchMode(ModeName.KitchenLesson);
                break;
            case 11:
                SimulationManager.Instance.SwitchMode(ModeName.BathRoomLesson);
                break;
            default:
                Debug.LogError("Invalid scene index: " + currentSceneIndex);
                break;
        }
    }

    public void SetCalendarTimer(float time) // called from WebGL
    {
        switch (SimulationManager.Instance.CurrentMode)
        {
            case ModeName.House:
                {
                    HouseMod houseMod = SimulationManager.Instance.GetMode<HouseMod>();
                    if (houseMod != null)
                    {
                        houseMod.calendarTimer = time;
                        houseMod.timerRun = true;
                    }
                    break;
                }
            case ModeName.ChildrenRoom:
                {
                    ChildrenRoomMode childrenRoomMode = SimulationManager.Instance.GetMode<ChildrenRoomMode>();
                    if (childrenRoomMode != null)
                    {
                        childrenRoomMode.hurricaneWatchTimer = time;
                        childrenRoomMode.timerRun = true;
                    }
                    break;
                }
            case ModeName.GardenView:
                {
                    GardenViewMode gardenViewMode = SimulationManager.Instance.GetMode<GardenViewMode>();
                    if (gardenViewMode != null)
                    {
                        gardenViewMode.hurricaneWarningTimer = time;
                        gardenViewMode.timerRun = true;
                    }
                    break;
                }
            case ModeName.SuperMarket:
                {
                    SuperMarketMode superMarketMode = SimulationManager.Instance.GetMode<SuperMarketMode>();
                    if (superMarketMode != null)
                    {
                        superMarketMode.calendarTimer = time;
                        superMarketMode.timerRun = true;
                    }
                    break;
                }
            case ModeName.Shelter:
                {
                    ShelterMod shelterMod = SimulationManager.Instance.GetMode<ShelterMod>();
                    if (shelterMod != null)
                    {
                        shelterMod.timer = time;
                        shelterMod.simulationStart = true;
                    }
                    break;
                }
            case ModeName.AfterTheHurricane:
                {
                    AfterTheHurricane afterTheHurricane = SimulationManager.Instance.GetMode<AfterTheHurricane>();
                    if (afterTheHurricane != null)
                    {
                        afterTheHurricane.timer = time;
                        afterTheHurricane.simulationStart = true;
                    }
                    break;
                }
            case ModeName.GoBagLesson:
                {
                    GoBagLesson goBagLesson = SimulationManager.Instance.GetMode<GoBagLesson>();
                    if (goBagLesson != null)
                    {
                        goBagLesson.timer = time;
                        goBagLesson.simulationStart = true;
                    }
                    break;
                }
            case ModeName.KitchenLesson:
                {
                    KitchenLesson kitchenLesson = SimulationManager.Instance.GetMode<KitchenLesson>();
                    if (kitchenLesson != null)
                    {
                        kitchenLesson.timer = time;
                        kitchenLesson.simulationStart = true;
                    }
                    break;
                }
            case ModeName.BathRoomLesson:
                {
                    BathRoomLesson bathRoomLesson = SimulationManager.Instance.GetMode<BathRoomLesson>();
                    if (bathRoomLesson != null)
                    {
                        bathRoomLesson.timer = time;
                        bathRoomLesson.simulationStart = true;
                    }
                    break;
                }
        }
    }

    public void CreateRadio(int id) => radioID = id;

    public int GetRadioID() => radioID;

    public void CreateJune1(int id) => june1ID = id;

    public int GetJune1ID() => june1ID;

    public void CreateMay(int id) => mayID = id;
    public int GetMayID() => mayID;

    public void CreateHurricaneWatch(int id) => hurricaneWatchID = id; // called from WebGL

    public int GetHurricaneWatchID() => hurricaneWatchID;

    public void CreateHurricaneWarning(int id) => hurricaneWarningID = id; // called from WebGL

    public int GetHurricaneWarningID() => hurricaneWarningID;

    public void CreateKay(int id) => kayID = id; // called from WebGL

    public int GetKayID() => kayID;

    public void CreateAllClear(int id) => allClearID = id;

    public int GetAllClearID() => allClearID;

    public int CreateKelanParents(int id) => kelanParentsID = id;

    public int GetKelanParentsID() => kelanParentsID;
}
