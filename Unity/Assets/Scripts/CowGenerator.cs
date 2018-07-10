using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using System.Collections.Generic;
using System;

public class CowGenerator : MonoBehaviour
{
    public int Count = 5;
    public GameObject CowPrefab;
    EntityManager entityManager;
    static List<int> CowEntityIDs;

    void Awake()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        EntityArchetype CowArchetype = entityManager.CreateArchetype(
            ComponentType.Create<Position>(),
            ComponentType.Create<Moo>(),
            ComponentType.Create<TransformMatrix>(),
            ComponentType.Create<AudioProperty>());
        MeshInstanceRenderer meshInstanceRenderer = new MeshInstanceRenderer();
        var cowPrefab = Instantiate(CowPrefab);
        meshInstanceRenderer = cowPrefab.GetComponent<MeshInstanceRendererComponent>().Value;
        Destroy(cowPrefab);

        CowEntityIDs = new List<int>(Count);

        for (int i = 0; i < Count; i++)
        {
            float3 pos = new float3(-10f + 5f * i, 0f, 0f);
            Entity cow = entityManager.CreateEntity(CowArchetype);
            entityManager.SetComponentData<Position>(cow, new Position(pos));
            entityManager.SetComponentData<Moo>(cow, new Moo(MooType.StartMooing, cow));
            entityManager.SetComponentData<AudioProperty>(cow, new AudioProperty(-1));
            entityManager.AddSharedComponentData<MeshInstanceRenderer>(cow, meshInstanceRenderer);
            CowEntityIDs.Add(cow.Index);
        }
    }

    public static KeyCode CowKeyCode(int entityIndex)
    {
        int index = CowEntityIDs.IndexOf(entityIndex);
        switch (index)
        {
            case 0:
                return KeyCode.Alpha1;
            case 1:
                return KeyCode.Alpha2;
            case 2:
                return KeyCode.Alpha3;
            case 3:
                return KeyCode.Alpha4;
            case 4:
                return KeyCode.Alpha5;
            default:
                throw new Exception("Entity index invalid or not found in CowEntityIDs");
        }
    }

    void OnGUI()
    {
        GUI.Box(new Rect(100, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[0]].ToString());
        GUI.Box(new Rect(325, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[1]].ToString());
        GUI.Box(new Rect((Screen.width / 2) - 25, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[2]].ToString());
        GUI.Box(new Rect(Screen.width - 350, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[3]].ToString());
        GUI.Box(new Rect(Screen.width - 125, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[4]].ToString());

        GUI.Box(new Rect(100, 50, 100, 30), "Press 1");
        GUI.Box(new Rect(325, 50, 100, 30), "Press 2");
        GUI.Box(new Rect((Screen.width / 2) - 25, 50, 100, 30), "Press 3");
        GUI.Box(new Rect(Screen.width - 350, 50, 100, 30), "Press 4");
        GUI.Box(new Rect(Screen.width - 125, 50, 100, 30), "Press 5");
    }

}

public struct Moo : IComponentData
{
    public Entity EntityID;
    public MooType MooStatus;
    public Moo(MooType status, Entity entity)
    {
        MooStatus = status;
        EntityID = entity;
    }
}
public enum MooType { StartMooing, Mooing, StopMooing, Quiet }

public struct Coloring : IComponentData
{
    public Color RequestColor;

    public Coloring(Color color)
    {
        RequestColor = color;
    }
}





