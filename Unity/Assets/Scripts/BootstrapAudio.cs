using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BootstrapAudio : MonoBehaviour
{
    [SerializeField] int _poolSize;
    [SerializeField] AudioClipList _clipList;
    AudioService _audioService;

    static EntityManager s_entityManager;
    //static World s_audioWorld;

    void Awake()
    {
        //audioWorld = new World("AudioWorld");
        //entityManager = audioWorld.GetOrCreateManager<EntityManager>();
        //ScriptBehaviourUpdateOrder.UpdatePlayerLoop(audioWorld);
        s_entityManager = World.Active.GetOrCreateManager<EntityManager>();
        Initialization_Pool();
        Initialization_SoundBank();
    }

    void Initialization_Pool()
    {
        GameObject sourcePrefab = new GameObject();
        AudioSource audioSource = sourcePrefab.AddComponent<AudioSource>();
        AudioService.ResetAudioSource(audioSource);

        for (int i = 0; i < _poolSize; i++)
        {
            GameObject sourceGO = Instantiate(sourcePrefab);
            Entity entity = GameObjectEntity.AddToEntityManager(s_entityManager, sourceGO);
            s_entityManager.AddComponentData(entity, new Position());
            s_entityManager.AddComponentData(entity, new CopyTransformToGameObject());
            s_entityManager.AddComponentData(entity, new AudioSourceHandle(entity, entity));
            sourceGO.name = "Source " + sourceGO.GetComponent<AudioSource>().GetInstanceID();
        }
  
        Destroy(sourcePrefab);
    }

    void Initialization_SoundBank()
    {
        if (_clipList == null)
            _clipList = FindObjectOfType<AudioClipList>();
        _audioService = new AudioService(_clipList);
    }

    public static EntityManager GetEntityManager()
    {
        return s_entityManager;
    }

}
