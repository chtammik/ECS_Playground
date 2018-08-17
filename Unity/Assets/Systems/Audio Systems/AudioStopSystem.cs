using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
public class AudioStopSystem : ComponentSystem
{
    struct ToStopGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SharedComponentDataArray<AudioStopRequest> StopRequests;
        [ReadOnly] public SharedComponentDataArray<AudioPlaying> Playing;
        [ReadOnly] public ComponentDataArray<ClaimedByVoice> Claimed;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] ToStopGroup _toStopGroup;

    struct ToStop_DonePlayingGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SubtractiveComponent<AudioStopRequest> No_StopRequests;
        [ReadOnly] public SubtractiveComponent<AudioProperty_Loop> No_Loop;
        [ReadOnly] public SharedComponentDataArray<AudioPlaying> Playing;
        [ReadOnly] public ComponentDataArray<ClaimedByVoice> Claimed;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] ToStop_DonePlayingGroup _toStop_DonePlayingGroup;

    protected override void OnUpdate()
    {
        //Stop AudioSources that already have StopSoundRequest on it, then remove the StopSoundRequest.
        for (int i = 0; i < _toStopGroup.Length; i++)
        {
            Entity sourceEntity = _toStopGroup.Entities[i];
            Entity voiceEntity = _toStopGroup.Claimed[i].VocieEntity;
            AudioSource audioSource = _toStopGroup.AudioSources[i];
            audioSource.Stop();
            AudioService.ResetAudioSource(audioSource);
            PostUpdateCommands.RemoveComponent<AudioStopRequest>(sourceEntity);
            PostUpdateCommands.RemoveComponent<AudioPlaying>(sourceEntity);
            PostUpdateCommands.AddComponent(voiceEntity, new AudioMessage_Stopped(voiceEntity));
            PostUpdateCommands.RemoveComponent<ClaimedByVoice>(sourceEntity);
        }

        //Add StopSoundRequests to AudioSources that have done playing.
        for (int i = 0; i < _toStop_DonePlayingGroup.Length; i++)
        {
            Entity entity = _toStop_DonePlayingGroup.Claimed[i].VocieEntity;
            AudioSource audioSource = _toStop_DonePlayingGroup.AudioSources[i];
            if (!audioSource.isPlaying && audioSource.time == 0) //TODO: checking this every frame is not ideal, any better way to do this?
                PostUpdateCommands.AddSharedComponent(entity, new AudioStopRequest());
        }

    }
}
