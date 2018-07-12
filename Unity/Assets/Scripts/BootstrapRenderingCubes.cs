using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

public class BootstrapRenderingCubes : MonoBehaviour {
    public static BootstrapRenderingCubes Instance { get; private set; }
    public GameObject ECSPrefab;
    public GameObject UsualPrefab;
    public int count = 10000;
    [Range(-100f, 100f)]
    public float speed = 1f;

    public bool pureECS = true;

    void Awake ()
    {
        // ugly singleton so we can read the movement speed at runtime from the movement system
        Instance = this;
        // use default world and entity manager so we can use existing systems
        EntityManager entityManager = World.Active.GetExistingManager<EntityManager>();
        if (pureECS) // avoid using GameObjects alltogether
        {
            // setup archetype
            EntityArchetype archetype = entityManager.CreateArchetype(
                ComponentType.Create<Position>(),
                ComponentType.Create<MoveInDirection>(),
                ComponentType.Create<TransformMatrix>(),
                ComponentType.Create<EmptyComponent>());
            // get instance renderer
            MeshInstanceRenderer meshInstanceRenderer = new MeshInstanceRenderer();
            var proto = Instantiate(ECSPrefab);
            meshInstanceRenderer = proto.GetComponent<MeshInstanceRendererComponent>().Value;
            Destroy(proto);
            // setup entities
            for (int j = 0; j < count; j++)
            {
                Entity entity = entityManager.CreateEntity(archetype);
                entityManager.SetComponentData(entity, new MoveInDirection(Random.insideUnitSphere));
                // add shared instance renderer
                entityManager.AddSharedComponentData(entity, meshInstanceRenderer);
            }
        }
        else // use GameObjects
        {
            for (int i = 0; i < count; i++)
            {
                // instanciate prefab
                GameObject go = Instantiate(UsualPrefab, transform);
                // get entity
                Entity entity = GameObjectEntity.AddToEntityManager(entityManager, go);
                // setup components for moving
                entityManager.AddComponentData(entity, new Position());
                entityManager.AddComponentData(entity, new MoveInDirection(Random.insideUnitSphere));
            }
        }
    }
}

///<summary>an empty component only used to make a signature unique</summary>
public struct EmptyComponent : IComponentData { }
