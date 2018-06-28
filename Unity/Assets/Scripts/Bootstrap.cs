using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

public class Bootstrap : MonoBehaviour {
    public static Bootstrap Instance { get; private set; }
    public GameObject ECSPrefab;
    public GameObject UsualPrefab;
    public int count = 10000;
    [Range(-100f, 100f)]
    public float speed = 1f;

    public bool pureECS = true;

    void Awake () {
        Instance = this;
        var entityManager = World.Active.GetExistingManager<EntityManager>();
        // avoid using GameObjects alltogether
        if (pureECS)
        {
            // setup archetype
            EntityArchetype archetype = entityManager.CreateArchetype(
                ComponentType.Create<Position>(),
                ComponentType.Create<Direction>(),
                ComponentType.Create<TransformMatrix>());
            // get instance renderer
            MeshInstanceRenderer meshInstanceRenderer = new MeshInstanceRenderer();
            var proto = Instantiate(ECSPrefab);
            meshInstanceRenderer = proto.GetComponent<MeshInstanceRendererComponent>().Value;
            Destroy(proto);
            // setup entities
            for (int j = 0; j < count; j++)
            {
                Entity entity = entityManager.CreateEntity(archetype);
                entityManager.SetComponentData(entity, new Direction(Random.insideUnitSphere));
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
                entityManager.AddComponentData(entity, new Direction(Random.insideUnitSphere));
            }
        }

        
    }
}
