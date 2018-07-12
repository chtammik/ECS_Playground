using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;
using System.Collections.Generic;

public class BootstrapAudioSystem : MonoBehaviour {
    public static BootstrapAudioSystem Instance { get; private set; }
    public GameObject ECSPrefab;
    public AudioClipList list;
    public int cubeCount = 10000;
    public int soundCount = 100;
    
    private List<Entity> entities; 
    EntityManager entityManager;
    void Awake ()
    {
        // ugly singleton so we can read the movement speed at runtime from the movement system
        Instance = this;
        // use default world and entity manager so we can use existing systems
        entityManager = World.Active.GetExistingManager<EntityManager>();

        // setup cube
        EntityArchetype cube = entityManager.CreateArchetype(
            ComponentType.Create<Position>(),
            ComponentType.Create<MoveInDirection>(),
            ComponentType.Create<TransformMatrix>(),
            ComponentType.Create<EmptyComponent>(),
            ComponentType.Create<Clip>());
        // get instance renderer
        MeshInstanceRenderer meshInstanceRenderer = new MeshInstanceRenderer();
        var proto = Instantiate(ECSPrefab);
        meshInstanceRenderer = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        Destroy(proto);
        // setup entities
        for (int j = 0; j < cubeCount; j++)
        {
            Entity entity = entityManager.CreateEntity(cube);
            entityManager.SetComponentData(entity, new MoveInDirection(Random.insideUnitSphere));
            entityManager.SetComponentData(entity, new Clip(list.clips[Random.Range(0, list.clips.Length)]));
            // add shared instance renderer
            entityManager.AddSharedComponentData(entity, meshInstanceRenderer);
        }

        // setup sounds
        GameObject sourcePrefab = new GameObject("Source");
        var source = sourcePrefab.AddComponent<AudioSource>();
        source.playOnAwake = false;
        for (int i = 0; i < soundCount; i++)
        {
            // instanciate prefab
            GameObject go = Instantiate(sourcePrefab, transform);
            // get entity
            Entity entity = GameObjectEntity.AddToEntityManager(entityManager, go);
            // setup components for moving
            entityManager.AddComponentData(entity, new Position());
            int sourceID = go.GetComponent<AudioSource>().GetInstanceID();
            entityManager.AddComponentData(entity, new SourceHandle(sourceID));
        }
    }

    void OnGui(){
        if(GUILayout.Button("PlaySome")){
            int random = Random.Range(20, 200);
            for(int i = 0; i < 10; i += random){
                if(i > entities.Count)
                    return;
                entityManager.AddComponent(entities[i], typeof(ToPlay));
            }
        }
    }
}
