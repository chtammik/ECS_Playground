using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class AudioSourcePool : MonoBehaviour
{
    public int Count = 3;
    GameObject[] pool;
    Dictionary<int, GameObject> PoolDictionary;
    Queue<int> IDs;
    Dictionary<int, float3> PositionDictionary;
    public bool SourceAvailable { get { return IDs.Count > 0; } }

    void Awake()
    {
        pool = new GameObject[Count];
        PoolDictionary = new Dictionary<int, GameObject>(Count);
        IDs = new Queue<int>(Count);
        PositionDictionary = new Dictionary<int, float3>(Count);

        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = new GameObject();
            int id = pool[i].AddComponent<AudioSource>().GetInstanceID();
            pool[i].AddComponent<GameObjectEntity>();
            pool[i].name = "AudioSource " + id;
            PoolDictionary.Add(id, pool[i]);
            IDs.Enqueue(id);
            PositionDictionary.Add(id, new float3());
        }
    }

    void Start()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            pool[i].GetComponent<AudioSource>().loop = true;
            pool[i].GetComponent<AudioSource>().playOnAwake = false;
        }
    }

    public GameObject GetAudioSource(int id)
    {
        if (PoolDictionary.TryGetValue(id, out GameObject tempGO))
            return PoolDictionary[id];
        else
        {
            Debug.Log("No AudioSource Found, ID invalid");
            return null;
        }
    }

    public int GetNewID()
    {
        return IDs.Count == 0 ? -1 : IDs.Dequeue();
    }

    public void ReturnIDBack(int id)
    {
        if (PoolDictionary.TryGetValue(id, out GameObject tempGO))
            IDs.Enqueue(id);
        else
            throw new Exception("ID can't be returned to the pool, ID invalid");
    }

    public void RecordNewPosition(int id, float3 pos)
    {
        if (PositionDictionary.TryGetValue(id, out float3 temp))
            PositionDictionary[id] = pos;
        else if (id != -1)
            throw new Exception("position not updated because no such id found");
    }

    public float3 GetNewPosition(int id)
    {
        if (PositionDictionary.TryGetValue(id, out float3 pos))
            return pos;
        else
            throw new Exception("can't get position because no such id found");
    }

}
