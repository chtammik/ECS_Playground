using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(AudioPlaySystem))]
[UpdateAfter(typeof(CopyAudioPropertiesSystem.CopyAudioPropertiesBarrier))]
public class ApplyAudioPropertiesSystem : ComponentSystem
{
    EntityManager entityManager;

    struct SourceHandleGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentArray<AudioSource> Sources;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> Claimeds;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
        [ReadOnly] public SubtractiveComponent<ReadyToPlay> ReadyToPlays;
        [ReadOnly] public SubtractiveComponent<AudioPlaying> AudioPlayings;
    }
    [Inject] SourceHandleGroup sourceGroup;

    protected override void OnStartRunning()
    {
        entityManager = BootstrapAudio.GetEntityManager();
    }

    [Inject] ComponentDataFromEntity<AudioClipID> AudioClips;
    [Inject] ComponentDataFromEntity<AudioProperty_SpatialBlend> AudioSpatialBlends;

    protected override void OnUpdate()
    {
        for (int i = 0; i < sourceGroup.Length; i++)
        {
            Entity entity = sourceGroup.Entities[i];
            AudioSource audioSource = sourceGroup.Sources[i];
            if (AudioClips.Exists(entity))
            {
                audioSource.clip = BootstrapAudio.GetClipList().clips[AudioClips[entity].ID];
                PostUpdateCommands.RemoveComponent<AudioClipID>(entity);
            }
                
            if (AudioSpatialBlends.Exists(entity))
            {
                audioSource.spatialBlend = AudioSpatialBlends[entity].Blend;
                PostUpdateCommands.RemoveComponent<AudioProperty_SpatialBlend>(entity);
            }
                
            //...
            PostUpdateCommands.AddSharedComponent(entity, new ReadyToPlay());
        }
    }
}
