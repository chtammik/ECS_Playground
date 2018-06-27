using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

public class Bootstrap : MonoBehaviour {
    public static Bootstrap Instance { get; private set; }
    public GameObject prefab;
    public int count = 1024;
    [Range(-10f, 10f)]
    public float speed = 1f;

    void Awake () {
        Instance = this;
        var entityManager = World.Active.GetExistingManager<EntityManager>();
        float range = 10f;

        // setup GameObject entities
        EntityArchetype archetype = entityManager.CreateArchetype(
            ComponentType.Create<Position>(), 
            ComponentType.Create<Direction>(),
            ComponentType.Create<TransformMatrix>());
        //for (int i = 0; i < count; i++)
        //{
        //    var go = Instantiate(prefab, transform);
        //    var entity = GameObjectEntity.AddToEntityManager(entityManager, go);
        //    var position = new PositionComponent()
        //    {
        //        value = 
        //    };
        //    var direction = new Direction()
        //    {
        //        value = Random.insideUnitSphere
        //    };
        //    entityManager.AddComponentData(entity, position);
        //    entityManager.AddComponentData(entity, direction);
        //}
        
        MeshInstanceRenderer meshInstanceRenderer = new MeshInstanceRenderer();
        var proto = GameObject.Instantiate(prefab);
        meshInstanceRenderer = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        GameObject.Destroy(proto);

        for (int j = 0; j < count; j++)
        {
            var e = entityManager.CreateEntity(archetype);
            //var posData = new Position();
            //posData.Value.xyz = new Vector3(
            //    Random.Range(-range, range), 
            //    Random.Range(-range, range), 
            //    Random.Range(-range, range));
            //entityManager.SetComponentData<Position>(e, posData);
            entityManager.SetComponentData<Direction>(e, new Direction()
                { Value = new float3(Random.insideUnitSphere) });
            entityManager.AddSharedComponentData(e, meshInstanceRenderer);
        }
    }
}
