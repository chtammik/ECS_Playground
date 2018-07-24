using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(AudioPoolSystem.AssignSourceIDBarrier))]
public class AudioMuteSystem : ComponentSystem
{
    struct AudioSourceGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SharedComponentDataArray<AudioMuteRequest> VirtualizeRequests;
        [ReadOnly] public SharedComponentDataArray<AudioPlaying> PlayingTags;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> ClaimedTags;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] AudioSourceGroup audioSourceGroup;

    [Inject] ComponentDataFromEntity<AudioPlayRequest> PlayRequest;
    [Inject] ComponentDataFromEntity<AudioProperty_AudioClipID> AudioClip;
    [Inject] ComponentDataFromEntity<AudioProperty_SpatialBlend> AudioSpatialBlend;
    [Inject] ComponentDataFromEntity<AudioProperty_StartTime> StartTime;
    [Inject] ComponentDataFromEntity<AudioProperty_Loop> AudioLoops;

    protected override void OnUpdate()
    {
        for (int i = 0; i < audioSourceGroup.Length; i++)
        {
            Entity entity = audioSourceGroup.Entities[i];
            AudioSource audioSource = audioSourceGroup.AudioSources[i];
            audioSource.Stop();
            BootstrapAudio.ResetAudioSource(audioSource);
            PostUpdateCommands.RemoveComponent<AudioMuteRequest>(entity);
            PostUpdateCommands.RemoveComponent<AudioPlaying>(entity);
            PostUpdateCommands.RemoveComponent<AudioSourceClaimed>(entity);

            if (PlayRequest.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioPlayRequest>(entity);

            #region Copy Properties
            //copy Audio Properties back to game entities for the later devirtualizing.
            if (AudioClip.Exists(entity))
            {
                PostUpdateCommands.AddComponent(audioSourceGroup.Handles[i].OriginalEntity, AudioClip[entity]);
                PostUpdateCommands.RemoveComponent<AudioProperty_AudioClipID>(entity);
            }

            if (AudioSpatialBlend.Exists(entity))
            {
                PostUpdateCommands.AddComponent(audioSourceGroup.Handles[i].OriginalEntity, AudioSpatialBlend[entity]);
                PostUpdateCommands.RemoveComponent<AudioProperty_SpatialBlend>(entity);
            }

            if (StartTime.Exists(entity))
            {
                PostUpdateCommands.AddComponent(audioSourceGroup.Handles[i].OriginalEntity, StartTime[entity]);
                PostUpdateCommands.RemoveComponent<AudioProperty_StartTime>(entity);
            }

            if (AudioLoops.Exists(entity))
            {
                PostUpdateCommands.AddComponent(audioSourceGroup.Handles[i].OriginalEntity, AudioLoops[entity]);
                PostUpdateCommands.RemoveComponent<AudioProperty_Loop>(entity);
            }

            //...
            #endregion

            PostUpdateCommands.AddSharedComponent(audioSourceGroup.Handles[i].OriginalEntity, new AudioPlayingVirtually());
        }
    }
}
