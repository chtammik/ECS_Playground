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
        GameObject sourcePrefab = new GameObject("Source");
        AudioSource audioSource = sourcePrefab.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = true;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject sourceGO = Instantiate(sourcePrefab);
            Entity entity = GameObjectEntity.AddToEntityManager(entityManager, sourceGO);
            entityManager.AddComponentData(entity, new Position());
            entityManager.AddComponentData(entity, new CopyTransformToGameObject());
            int sourceID = sourceGO.GetComponent<AudioSource>().GetInstanceID();
            entityManager.AddComponentData(entity, new AudioSourceHandle(entity, sourceID));
        }
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


}
