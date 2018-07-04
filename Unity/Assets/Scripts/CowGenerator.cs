using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class CowGenerator : MonoBehaviour
{
    public int Count = 5;
    EntityManager entityManager;
    AudioSourcePool pool;

    void Awake()
    {
        entityManager = World.Active.GetExistingManager<EntityManager>();
        EntityArchetype CowArchetype = entityManager.CreateArchetype(
            ComponentType.Create<Position>(),
            ComponentType.Create<MoveInDirection>(),
            ComponentType.Create<AudioComponent>());

        pool = new AudioSourcePool();
        pool.Initialize(3);

        for (int i = 0; i < Count; i++)
        {
            Entity cow = entityManager.CreateEntity(CowArchetype);
            entityManager.SetComponentData<Position>(cow, new Position());
            entityManager.SetComponentData<MoveInDirection>(cow, new MoveInDirection(UnityEngine.Random.insideUnitSphere));
            entityManager.SetComponentData<AudioComponent>(cow, NewAudioComponent());
            Debug.Log(entityManager.GetComponentData<AudioComponent>(cow).AudioSourceID + " " + entityManager.GetComponentData<AudioComponent>(cow).Virtual);
        }

    }

    AudioComponent NewAudioComponent()
    {
        AudioComponent ac = new AudioComponent();
        ac.AudioSourceID = pool.GetNewID();
        ac.Virtual = ac.AudioSourceID == -1 ? VirtualStatus.Virtual : VirtualStatus.Real;
        return ac;
    }

}

public struct AudioComponent : IComponentData
{
    public int AudioSourceID;
    public VirtualStatus Virtual;
}

public enum VirtualStatus { Real, Virtual }

public class AudioSourcePool
{
    GameObject[] pool;
    Dictionary<int, GameObject> PoolDictionary;
    Queue<int> IDs;

    public void Initialize(int size)
    {
        pool = new GameObject[size];
        PoolDictionary = new Dictionary<int, GameObject>(size);
        IDs = new Queue<int>(size);

        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = new GameObject();
            pool[i].AddComponent<AudioSource>();
            pool[i].AddComponent<GameObjectEntity>();
            pool[i].name = "AudioSource " + pool[i].GetInstanceID();
            PoolDictionary.Add(pool[i].GetInstanceID(), pool[i]);
            IDs.Enqueue(pool[i].GetInstanceID());
        }
    }

    public GameObject GetAudioSource(int id)
    {
        if (PoolDictionary.ContainsKey(id))
            return PoolDictionary[id];
        else
            throw new Exception("No AudioSource Found, ID invalid");
    }

    public int GetNewID()
    {
        if (IDs.Count == 0)
            return -1;
        else
        {
            int newID = IDs.Dequeue();
            return newID;
        }
    }

    public void ReturnAnID(int id)
    {
        if (PoolDictionary.ContainsKey(id))
            IDs.Enqueue(id);
        else
            throw new Exception("ID can't be returned to the pool, ID invalid");
    }

}

public class JobCowBehaviourSystem : JobComponentSystem
{
    [BurstCompile]
    struct CowJob : IJobProcessComponentData<Position, MoveInDirection, AudioComponent>
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float speed;

        public void Execute(ref Position position, [ReadOnly]ref MoveInDirection dir, [ReadOnly]ref AudioComponent ac)
        {
            position.Value = position.Value + (dir.Value * deltaTime * speed);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CowJob()
        {
            deltaTime = Time.deltaTime,
            speed = 1f
        };

        return job.Schedule(this, 64, inputDeps);
    }
}

public class AudioSourcePositionSystem : ComponentSystem
{
    struct ACGroup
    {
        public readonly int Length;
        public ComponentArray<Transform> Transforms;
        public ComponentArray<AudioSource> AudioSources;
    }

    struct CowGroup
    {
        public readonly int Length;
        public ComponentDataArray<Position> Positions;
        public ComponentDataArray<AudioComponent> AudioComponents;
    }

    [Inject] private ACGroup acGroup;
    [Inject] CowGroup cowGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < acGroup.Length; i++)
        {
            int GOID = acGroup.Transforms[i].gameObject.GetInstanceID();
            for (int x = 0; x < cowGroup.Length; x++)
            {
                int cowID = cowGroup.AudioComponents[x].AudioSourceID;
                if (GOID == cowID)
                {
                    acGroup.Transforms[i].position = cowGroup.Positions[x].Value;
                    break;
                }
            }
        }
    }
}

