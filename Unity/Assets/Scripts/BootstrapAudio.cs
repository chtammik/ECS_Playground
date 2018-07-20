using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class BootstrapAudio : MonoBehaviour
{
    public int poolSize;
    public AudioClipList ClipList;
    static AudioClipList clipList;
    static EntityManager entityManager;

    void Awake()
    {
        entityManager = World.Active.GetOrCreateManager<EntityManager>();
        Initialization_Pool();
        Initialization_SoundBank();
    }

    void Initialization_Pool()
    {
        GameObject sourcePrefab = new GameObject();
        AudioSource audioSource = sourcePrefab.AddComponent<AudioSource>();
        ResetAudioSource(audioSource);

        for (int i = 0; i < poolSize; i++)
        {
            GameObject sourceGO = Instantiate(sourcePrefab);
            Entity entity = GameObjectEntity.AddToEntityManager(entityManager, sourceGO);
            entityManager.AddComponentData(entity, new Position());
            entityManager.AddComponentData(entity, new CopyTransformToGameObject());
            entityManager.AddComponentData(entity, new AudioSourceHandle(entity, entity));
            sourceGO.name = "Source " + sourceGO.GetComponent<AudioSource>().GetInstanceID();
        }
  
        Destroy(sourcePrefab);
    }

    void Initialization_SoundBank()
    {
        if (ClipList == null)
            ClipList = FindObjectOfType<AudioClipList>();
        clipList = ClipList;
    }

    public static EntityManager GetEntityManager()
    {
        return entityManager;
    }

    public static AudioClipList GetClipList()
    {
        return clipList;
    }

    public static void ResetAudioSource(AudioSource audioSource)
    {
        audioSource.clip = null;
        audioSource.playOnAwake = false;
        audioSource.loop = true;
    }

}
