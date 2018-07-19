using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AudioPlaySystem : ComponentSystem
{
    struct PlayGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> Claimeds;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
        [ReadOnly] public SharedComponentDataArray<ReadyToPlay> ReadyToPlays;
        [ReadOnly] public SubtractiveComponent<AudioPlaying> AudioPlayings;
    }
    [Inject] PlayGroup playGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < playGroup.Length; i++)
        {
            playGroup.AudioSources[i].Play();
            PostUpdateCommands.RemoveComponent<ReadyToPlay>(playGroup.Entities[i]);
            PostUpdateCommands.AddSharedComponent(playGroup.Entities[i], new AudioPlaying());
        }
    }

}
