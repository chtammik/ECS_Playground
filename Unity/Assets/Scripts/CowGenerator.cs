using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;

public class CowGenerator : MonoBehaviour
{
    public int Count = 5;
    public GameObject CowPrefab;
    EntityManager entityManager;

    void Awake()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        EntityArchetype CowArchetype = entityManager.CreateArchetype(
            ComponentType.Create<Position>(),
            ComponentType.Create<Moo>(),
            ComponentType.Create<TransformMatrix>(),
            ComponentType.Create<Coloring>());
        MeshInstanceRenderer meshInstanceRenderer = new MeshInstanceRenderer();
        var cowPrefab = Instantiate(CowPrefab);
        meshInstanceRenderer = cowPrefab.GetComponent<MeshInstanceRendererComponent>().Value;
        Destroy(cowPrefab);

        for (int i = 0; i < Count; i++)
        {
            float3 pos = new float3(-10f + 5f * i, 0f, 0f);
            Entity cow = entityManager.CreateEntity(CowArchetype);
            entityManager.SetComponentData<Position>(cow, new Position(pos));
            entityManager.SetComponentData<Moo>(cow, new Moo(MooType.StartMooing, cow));
            entityManager.AddSharedComponentData<MeshInstanceRenderer>(cow, meshInstanceRenderer);
        }
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
public enum MooType { StartMooing, Mooing, Quiet }

public struct Coloring : IComponentData
{
    public Color RequestColor;

    public Coloring(Color color)
    {
        RequestColor = color;
    }
}





