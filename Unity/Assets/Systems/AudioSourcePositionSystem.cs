using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class AudioSourcePositionSystem : ComponentSystem
{
    struct ASGroup
    {
        public readonly int Length;
        public ComponentArray<Transform> Transforms;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
    }

    [Inject] ASGroup asGroup;

    struct ASIDGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<Position> Positions;
        [ReadOnly] public ComponentDataArray<AudioSourceID> ASIDComponent;
    }

    [Inject] ASIDGroup asidGroup;

    struct PoolGroup
    {
        public ComponentArray<AudioSourcePool> Pool;
    }

    [Inject] PoolGroup poolGroup;

    protected override void OnUpdate()
    {
        //record entities' current positions to the dictionary.
        for (int i = 0; i < asidGroup.Length; i++)
        {
            int id = asidGroup.ASIDComponent[i].ASID;
            if (id != -1)
            {
                float3 pos = asidGroup.Positions[i].Value;
                poolGroup.Pool[0].RecordNewPosition(id, pos);
            }
        }

        //each audiosource looks up in the dictionary and set its position
        for (int i = 0; i < asGroup.Length; i++)
        {
            int id = asGroup.AudioSources[i].GetInstanceID();
            asGroup.Transforms[i].position = poolGroup.Pool[0].GetNewPosition(id);
        }
    }
}
