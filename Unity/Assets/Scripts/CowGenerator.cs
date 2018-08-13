using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class CowGenerator : MonoBehaviour
{
    [SerializeField] int _count = 5;
    [SerializeField] GameObject _cowPrefab;
    EntityManager _entityManager;
    static List<int> s_cowEntityIDs;

    void Awake()
    {
        _entityManager = World.Active.GetOrCreateManager<EntityManager>();
        EntityArchetype CowArchetype = _entityManager.CreateArchetype(
            ComponentType.Create<Position>(),
            ComponentType.Create<Moo>(),
            ComponentType.Create<TransformMatrix>(),
            ComponentType.Create<MeshInstanceRenderer>()
            );
        var cowPrefab = Instantiate(_cowPrefab);
        MeshInstanceRenderer meshInstanceRenderer = cowPrefab.GetComponent<MeshInstanceRendererComponent>().Value;
        Destroy(cowPrefab);

        s_cowEntityIDs = new List<int>(_count);

        for (int i = 0; i < _count; i++)
        {
            float3 pos = new float3(-10f + 5f * i, 0f, 0f);
            Entity cow = _entityManager.CreateEntity(CowArchetype);
            _entityManager.SetComponentData(cow, new Position(pos));
            _entityManager.SetComponentData(cow, new Moo(MooType.StartMooing, cow));
            _entityManager.SetSharedComponentData(cow, meshInstanceRenderer);
            s_cowEntityIDs.Add(cow.Index);
        }
    }

    public static KeyCode KeyCode_Moo(int entityIndex)
    {
        int index = s_cowEntityIDs.IndexOf(entityIndex);
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

    public static KeyCode KeyCode_Mute(int entityIndex)
    {
        int index = s_cowEntityIDs.IndexOf(entityIndex);
        switch (index)
        {
            case 0:
                return KeyCode.Q;
            case 1:
                return KeyCode.W;
            case 2:
                return KeyCode.E;
            case 3:
                return KeyCode.R;
            case 4:
                return KeyCode.T;
            default:
                throw new Exception("Entity index invalid or not found in CowEntityIDs");
        }
    }

    void OnGUI()
    {
        //GUI.Box(new Rect(50, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[0]].ToString());
        //GUI.Box(new Rect(300, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[1]].ToString());
        //GUI.Box(new Rect((Screen.width / 2) - 50, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[2]].ToString());
        //GUI.Box(new Rect(Screen.width - 400, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[3]].ToString());
        //GUI.Box(new Rect(Screen.width - 175, 25, 100, 30), AudioInfoGUISystem.ASIDPlayStatus[CowEntityIDs[4]].ToString());

        GUI.Box(new Rect(50, 50, 150, 30), "Play/Stop: Press 1");
        GUI.Box(new Rect(300, 50, 150, 30), "Play/Stop: Press 2");
        GUI.Box(new Rect((Screen.width / 2) - 50, 50, 150, 30), "Play/Stop: Press 3");
        GUI.Box(new Rect(Screen.width - 400, 50, 150, 30), "Play/Stop: Press 4");
        GUI.Box(new Rect(Screen.width - 175, 50, 150, 30), "Play/Stop: Press 5");

        GUI.Box(new Rect(50, 75, 150, 30), "Mute/Unmute: Press Q");
        GUI.Box(new Rect(300, 75, 150, 30), "Mute/Unmute: Press W");
        GUI.Box(new Rect((Screen.width / 2) - 50, 75, 150, 30), "Mute/Unmute: Press E");
        GUI.Box(new Rect(Screen.width - 400, 75, 150, 30), "Mute/Unmute: Press R");
        GUI.Box(new Rect(Screen.width - 175, 75, 150, 30), "Mute/Unmute: Press T");
    }

}

public struct Moo : IComponentData
{
    public Entity Entity;
    public MooType MooStatus;
    public Moo(MooType status, Entity entity)
    {
        MooStatus = status;
        Entity = entity;
    }
}
public enum MooType { StartMooing, Mooing, StopMooing, Quiet, MuteMooing, Muted, UnmuteMooing }





