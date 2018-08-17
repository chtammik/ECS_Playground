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
        [ReadOnly] public ComponentDataArray<ClaimedByVoice> Claimeds;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
        [ReadOnly] public SubtractiveComponent<AudioReadyToPlay> No_ReadyToPlay;
        [ReadOnly] public SubtractiveComponent<AudioPlaying> No_AudioPlaying;
    }
    [Inject] SourceHandleGroup _sourceGroup;

    [Inject] ComponentDataFromEntity<AudioProperty_AudioClipID> _audioClip;
    [Inject] ComponentDataFromEntity<AudioProperty_SpatialBlend> _audioSpatialBlend;
    [Inject] ComponentDataFromEntity<AudioProperty_Loop> _audioLoop;

    protected override void OnUpdate()
    {
        for (int i = 0; i < _sourceGroup.Length; i++)
        {
            Entity entity = _sourceGroup.Entities[i];
            AudioSource audioSource = _sourceGroup.Sources[i];
            if (_audioClip.Exists(entity))
                audioSource.clip = AudioService.GetAudioClip(_audioClip[entity].ID);
                
            if (_audioSpatialBlend.Exists(entity))
                audioSource.spatialBlend = _audioSpatialBlend[entity].Blend;

            if (_audioLoop.Exists(entity))
                audioSource.loop = true;
            else
                audioSource.loop = false;

            //...
            PostUpdateCommands.AddSharedComponent(entity, new AudioReadyToPlay());
        }
    }
}
