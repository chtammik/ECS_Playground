using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
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
            Entity sourceEntity = audioSourceGroup.Entities[i];
            AudioSource audioSource = audioSourceGroup.AudioSources[i];
            audioSource.Stop();
            AudioService.ResetAudioSource(audioSource);
            PostUpdateCommands.RemoveComponent<AudioMuteRequest>(sourceEntity);
            PostUpdateCommands.RemoveComponent<AudioPlaying>(sourceEntity);
            PostUpdateCommands.RemoveComponent<AudioSourceClaimed>(sourceEntity);

            if (PlayRequest.Exists(sourceEntity))
                PostUpdateCommands.RemoveComponent<AudioPlayRequest>(sourceEntity);

            #region Copy Properties
            //copy Audio Properties back to game entities for the later devirtualizing.
            Entity gameEntity = audioSourceGroup.Handles[i].GameEntity;

            if (AudioClip.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(gameEntity, new AudioProperty_AudioClipID(gameEntity, AudioClip[sourceEntity].ID));
                PostUpdateCommands.RemoveComponent<AudioProperty_AudioClipID>(sourceEntity);
            }

            if (AudioSpatialBlend.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(gameEntity, new AudioProperty_SpatialBlend(gameEntity, AudioSpatialBlend[sourceEntity].Blend));
                PostUpdateCommands.RemoveComponent<AudioProperty_SpatialBlend>(sourceEntity);
            }

            if (StartTime.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(gameEntity, new AudioProperty_StartTime(gameEntity, StartTime[sourceEntity].Time));
                PostUpdateCommands.RemoveComponent<AudioProperty_StartTime>(sourceEntity);
            }

            if (AudioLoops.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(gameEntity, new AudioProperty_Loop(gameEntity));
                PostUpdateCommands.RemoveComponent<AudioProperty_Loop>(sourceEntity);
            }

            //...
            #endregion

            PostUpdateCommands.AddComponent(gameEntity, new AudioPlayingVirtually(gameEntity));
        }
    }
}
