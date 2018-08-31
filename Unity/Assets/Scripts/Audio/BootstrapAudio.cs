using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BootstrapAudio : MonoBehaviour
{
    [SerializeField] int _poolSize;
    [SerializeField] SoundBank _soundBank;
    
    static EntityManager s_entityManager;
    //static World s_audioWorld;

    public delegate void BootstrapMessage();
    public static event BootstrapMessage OnAudioServiceInitialized;
    static bool s_audioServiceInitialized;
    public static bool IfAudioServiceInitialized() { return s_audioServiceInitialized; }

    void Start()
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
            s_entityManager.AddComponentData(entity, new AudioSourceHandle(entity));
            sourceGO.name = "Source " + sourceGO.GetComponent<AudioSource>().GetInstanceID();
        }

        Destroy(sourcePrefab);
    }

    void Initialization_SoundBank() //TODO: make sound banks able to load and unload.
    {
        if (_soundBank == null)
            Debug.LogError("No SoundBank defined in BootstapAudio");
        CreateAudioService();
    }

    public static EntityManager GetEntityManager()
    {
        return s_entityManager;
    }

    void CreateAudioService()
    {
        AudioService.Initialize(_soundBank, s_entityManager);
        OnAudioServiceInitialized();
        OnAudioServiceInitialized = null; //clear all the subscribers because the AudioService initialization only happpens once.
        s_audioServiceInitialized = true;
    }

    void OnDestroy()
    {
        OnAudioServiceInitialized = null;
    }

}
