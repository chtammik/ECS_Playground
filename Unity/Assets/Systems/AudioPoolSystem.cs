using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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
        public EntityArray Entities;
        public ComponentDataArray<AudioSourceID> ASIDs;
        [ReadOnly] public ComponentDataArray<PlaySoundRequest> PlayRequests;
    }

    [Inject] ASIDGroup asidGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < asidGroup.Length; i++)
        {
            if (asidGroup.ASIDs[i].PlayStatus == PlayType.NeedSource 
                || asidGroup.ASIDs[i].PlayStatus == PlayType.Mute
                || asidGroup.ASIDs[i].PlayStatus == PlayType.Stop)
            {
                if (poolGroup.Pool[0].SourceAvailable)
                    asidGroup.ASIDs[i] = new AudioSourceID(asidGroup.Entities[i], poolGroup.Pool[0].GetNewID(), PriorityType.Medium, PlayType.ReadyToPlay);
                else
                    asidGroup.ASIDs[i] = new AudioSourceID(asidGroup.Entities[i], -1, PriorityType.Medium, PlayType.NeedSource);
            }

            else
            {
                Debug.Log("It's already playing.");
                PostUpdateCommands.RemoveComponent<PlaySoundRequest>(asidGroup.Entities[i]);
            }
        }
    }
}
