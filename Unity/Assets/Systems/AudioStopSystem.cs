using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[UpdateBefore(typeof(AudioPoolSystem.AssignSourceIDBarrier))]
public class AudioStopSystem : ComponentSystem
{
    struct ToStopGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<AudioSourceID> ASIDs;
        [ReadOnly] public SharedComponentDataArray<StopSoundRequest> StopRequests;
    }
    [Inject] ToStopGroup toStopGroup;

    struct AudioSourceGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SharedComponentDataArray<StopSoundRequest> StopRequests;
        [ReadOnly] public SharedComponentDataArray<AudioPlaying> AudioPlayings;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> Claimeds;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] AudioSourceGroup audioSourceGroup;

    protected override void OnUpdate()
    {
        //Stop AudioSources that already have StopSoundRequest on it.
        for (int i = 0; i < audioSourceGroup.Length; i++)
        {
            audioSourceGroup.AudioSources[i].Stop();
            PostUpdateCommands.RemoveComponent<StopSoundRequest>(audioSourceGroup.Entities[i]);
            PostUpdateCommands.RemoveComponent<AudioPlaying>(audioSourceGroup.Entities[i]);
            PostUpdateCommands.RemoveComponent<AudioSourceClaimed>(audioSourceGroup.Entities[i]);
        }

        //Copy StopSoundRequest over to AudioSources then remove it from game entities.
        for (int i = 0; i < toStopGroup.Length; i++)
        {
            PostUpdateCommands.AddSharedComponent(toStopGroup.ASIDs[i].HandleEntity, toStopGroup.StopRequests[i]);
            PostUpdateCommands.RemoveComponent<StopSoundRequest>(toStopGroup.Entities[i]);
            PostUpdateCommands.RemoveComponent<AudioSourceID>(toStopGroup.Entities[i]);
        }
    }
}
