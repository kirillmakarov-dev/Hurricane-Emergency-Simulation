using System;
using System.Collections.Generic;
using UnityEngine;

public class GameModeFactory
{
    private static readonly Dictionary<ModeName, Type> modeTypeMap = new Dictionary<ModeName, Type>
    {
         { ModeName.EntryScreen, typeof(EntryScreenMod) },
         { ModeName.House, typeof(HouseMod) },
         { ModeName.ClearingGarden, typeof(ClearingGardenMod) },
         { ModeName.SuperMarket, typeof(SuperMarketMode) },
         { ModeName.ChildrenRoom, typeof(ChildrenRoomMode) },
         { ModeName.GardenView, typeof(GardenViewMode) },
         { ModeName.Shelter, typeof(ShelterMod) },
         { ModeName.AfterTheHurricane, typeof(AfterTheHurricane) },
         { ModeName.GoBagLesson, typeof(GoBagLesson) },
         { ModeName.KitchenLesson, typeof(KitchenLesson) },
         { ModeName.BathRoomLesson, typeof(BathRoomLesson) }
    };



    public ISimulationMode GetMode(ModeName modeName, GameObject target)
    {
        if (modeTypeMap.TryGetValue(modeName, out Type type))
        {
            Component component = target.GetComponent(type);
            if (component == null)
            {
                component = target.AddComponent(type);
                if (component is ISimulationMode gameMode)
                {
                    gameMode.Initialize();
                }
            }
            return component as ISimulationMode;
        }

        // Debug.LogError($"No mode mapping found for {modeName}");
        return null;
    }

    public T GetMode<T>(GameObject target) where T : Component, ISimulationMode
    {
        T component = target.GetComponent<T>();
        if (component == null)
        {
            component = target.AddComponent<T>();
            component.Initialize();
        }
        return component;
    }
}
