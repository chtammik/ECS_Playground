using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(ApplyAudioPropertiesSystem))]
public class AudioPlaySystem : ComponentSystem
{
    struct ToPlayGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> Claimeds;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
        [ReadOnly] public SharedComponentDataArray<ReadyToPlay> ReadyToPlays;
        [ReadOnly] public SubtractiveComponent<AudioPlaying> AudioPlayings;
    }
    [Inject] ToPlayGroup toPlayGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < toPlayGroup.Length; i++)
        {
            toPlayGroup.AudioSources[i].Play();
            PostUpdateCommands.RemoveComponent<ReadyToPlay>(toPlayGroup.Entities[i]);
            PostUpdateCommands.AddSharedComponent(toPlayGroup.Entities[i], new AudioPlaying());
        }
    }

}
