using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(AudioPlaySystem))]
[UpdateAfter(typeof(CopyAudioPropertiesSystem.CopyAudioPropertiesBarrier))]
public class ApplyAudioPropertiesSystem : ComponentSystem
{
    struct SourceHandleGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentArray<AudioSource> Sources;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> Claimeds;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
        [ReadOnly] public SubtractiveComponent<AudioReadyToPlay> ReadyToPlays;
        [ReadOnly] public SubtractiveComponent<AudioPlaying> PlayingTags;
    }
    [Inject] SourceHandleGroup sourceGroup;

    [Inject] ComponentDataFromEntity<AudioProperty_AudioClipID> AudioClip;
    [Inject] ComponentDataFromEntity<AudioProperty_SpatialBlend> AudioSpatialBlend;
    [Inject] ComponentDataFromEntity<AudioProperty_Loop> AudioLoop;

    protected override void OnUpdate()
    {
        for (int i = 0; i < sourceGroup.Length; i++)
        {
            Entity entity = sourceGroup.Entities[i];
            AudioSource audioSource = sourceGroup.Sources[i];
            if (AudioClip.Exists(entity))
                audioSource.clip = BootstrapAudio.GetClipList().Clips[AudioClip[entity].ID];
                
            if (AudioSpatialBlend.Exists(entity))
                audioSource.spatialBlend = AudioSpatialBlend[entity].Blend;

            if (AudioLoop.Exists(entity))
                audioSource.loop = true;
            else
                audioSource.loop = false;

            //...
            PostUpdateCommands.AddSharedComponent(entity, new AudioReadyToPlay());
        }
    }
}
