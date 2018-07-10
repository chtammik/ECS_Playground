using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class AudioPoolSystem : ComponentSystem
{
    struct PoolGroup
    {
        public ComponentArray<AudioSourcePool> Pool;
    }

    [Inject] PoolGroup poolGroup;

    struct ASIDGroup
    {
        public readonly int Length;
        public ComponentDataArray<AudioSourceID> asID;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] ASIDGroup asidGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < asidGroup.Length; i++)
        {
            if (asidGroup.asID[i].PlayStatus == PlayType.NeedSource)
            {
                int newID = poolGroup.Pool[0].GetNewID();
                if (newID != -1)
                    asidGroup.asID[i] = new AudioSourceID(asidGroup.asID[i].EntityID, newID, asidGroup.asID[i].Priority, PlayType.ReadyToPlay);
                if (asidGroup.AudioProperties[i].StartTime == -1)
                    asidGroup.AudioProperties[i] = new AudioProperty(AudioSettings.dspTime);
            }
        }
    }
}
