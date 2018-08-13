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
        [ReadOnly] public SharedComponentDataArray<AudioPlaying> PlayingTags;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> ClaimedTags;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] ToStopGroup toStopGroup;

    struct ToStop_DonePlayingGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SubtractiveComponent<AudioStopRequest> StopRequests;
        [ReadOnly] public SubtractiveComponent<AudioProperty_Loop> LoopingTags;
        [ReadOnly] public SharedComponentDataArray<AudioPlaying> PlayingTags;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> ClaimedTags;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] ToStop_DonePlayingGroup toStop_DonePlayingGroup;

    protected override void OnUpdate()
    {
        //Stop AudioSources that already have StopSoundRequest on it, then remove the StopSoundRequest and all the Audio Properties.
        for (int i = 0; i < toStopGroup.Length; i++)
        {
            Entity entity = toStopGroup.Entities[i];
            AudioSource audioSource = toStopGroup.AudioSources[i];
            audioSource.Stop();
            AudioService.ResetAudioSource(audioSource);
            PostUpdateCommands.RemoveComponent<AudioStopRequest>(entity);
            PostUpdateCommands.RemoveComponent<AudioPlaying>(entity);
            PostUpdateCommands.RemoveComponent<AudioSourceClaimed>(entity);
        }

        //Add StopSoundRequests to AudioSources that have done playing.
        for (int i = 0; i < toStop_DonePlayingGroup.Length; i++)
        {
            Entity entity = toStop_DonePlayingGroup.Handles[i].GameEntity;
            AudioSource audioSource = toStop_DonePlayingGroup.AudioSources[i];
            if (!audioSource.isPlaying && audioSource.time == 0) //checking this every frame is not ideal, any better way to do this?
                PostUpdateCommands.AddSharedComponent(entity, new AudioStopRequest());
        }

    }
}
