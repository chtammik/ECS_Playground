using Unity.Collections;
using Unity.Entities;
using UnityEngine;

//TODO: The virtual groups also need to stop when done playing if it's not looping.

[UpdateBefore(typeof(AudioPoolSystem.AssignSourceIDBarrier))]
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
        [ReadOnly] public SharedComponentDataArray<AudioPlaying> PlayingTags;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> ClaimedTags;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] ToStop_DonePlayingGroup toStop_DonePlayingGroup;

    struct ToStop_VirtualGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<AudioSourceID> ASIDs;
        [ReadOnly] public SharedComponentDataArray<AudioStopRequest> StopRequests;
        [ReadOnly] public ComponentDataArray<AudioPlayRequest> PlayRequests;
        [ReadOnly] public SharedComponentDataArray<AudioPlayingVirtually> VirtualTags;
    }
    [Inject] ToStop_VirtualGroup toStop_VirtualGroup;

    [Inject] ComponentDataFromEntity<AudioProperty_AudioClipID> AudioClip;
    [Inject] ComponentDataFromEntity<AudioProperty_SpatialBlend> AudioSpatialBlend;
    [Inject] ComponentDataFromEntity<AudioProperty_StartTime> StartTime;

    protected override void OnUpdate()
    {
        //Stop AudioSources that already have StopSoundRequest on it, then remove the StopSoundRequest and all the Audio Properties.
        for (int i = 0; i < toStopGroup.Length; i++)
        {
            Entity entity = toStopGroup.Entities[i];
            AudioSource audioSource = toStopGroup.AudioSources[i];
            audioSource.Stop(); 
            BootstrapAudio.ResetAudioSource(audioSource);
            PostUpdateCommands.RemoveComponent<AudioStopRequest>(entity);
            PostUpdateCommands.RemoveComponent<AudioPlaying>(entity);
            PostUpdateCommands.RemoveComponent<AudioSourceClaimed>(entity);

            if (AudioClip.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_AudioClipID>(entity);

            if (AudioSpatialBlend.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_SpatialBlend>(entity);

            if (StartTime.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_StartTime>(entity);
            //...
        }

        //Add StopSoundRequests to AudioSources that have done playing.
        for (int i = 0; i < toStop_DonePlayingGroup.Length; i++)
        {
            Entity entity = toStop_DonePlayingGroup.Entities[i];
            AudioSource audioSource = toStop_DonePlayingGroup.AudioSources[i];
            if (!audioSource.isPlaying && audioSource.time == 0) //checking this everyframe is not ideal, any better way to do this?
                PostUpdateCommands.AddSharedComponent(entity, new AudioStopRequest());
        }

        //Clean AudioSourceIDs that are playing virtually.
        for (int i = 0; i < toStop_VirtualGroup.Length; i++)
        {
            Entity entity = toStop_VirtualGroup.Entities[i];
            PostUpdateCommands.RemoveComponent<AudioStopRequest>(entity);
            PostUpdateCommands.RemoveComponent<AudioPlayingVirtually>(entity);
            PostUpdateCommands.RemoveComponent<AudioPlayRequest>(entity);

            if (AudioClip.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_AudioClipID>(entity);

            if (AudioSpatialBlend.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_SpatialBlend>(entity);

            if (StartTime.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_StartTime>(entity);
            //...
        }
    }
}
