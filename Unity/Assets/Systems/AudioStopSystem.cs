using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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
        [ReadOnly] public SubtractiveComponent<AudioProperty_Loop> LoopingTags;
        [ReadOnly] public SharedComponentDataArray<AudioPlaying> PlayingTags;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> ClaimedTags;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] ToStop_DonePlayingGroup toStop_DonePlayingGroup;

    struct ToStop_VirtualGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public SharedComponentDataArray<AudioPlayingVirtually> VirtualTags;
        [ReadOnly] public SharedComponentDataArray<AudioStopRequest> StopRequests;
        [ReadOnly] public ComponentDataArray<AudioPlayRequest> PlayRequests;
        [ReadOnly] public SubtractiveComponent<AudioSourceID> ASIDs;
    }
    [Inject] ToStop_VirtualGroup toStop_VirtualGroup;

    struct ToStop_DonePlayingVirtualGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public SharedComponentDataArray<AudioPlayingVirtually> VirtualTags;
        [ReadOnly] public ComponentDataArray<AudioProperty_StartTime> StartTimes;
        [ReadOnly] public SubtractiveComponent<AudioSourceID> ASIDs;
        [ReadOnly] public SubtractiveComponent<AudioStopRequest> StopRequests;
        [ReadOnly] public SubtractiveComponent<AudioProperty_Loop> LoopingTags;
    }
    [Inject] ToStop_DonePlayingVirtualGroup toStop_DonePlayingVirtualGroup;

    [Inject] ComponentDataFromEntity<AudioProperty_AudioClipID> AudioClip;
    [Inject] ComponentDataFromEntity<AudioProperty_SpatialBlend> AudioSpatialBlend;
    [Inject] ComponentDataFromEntity<AudioProperty_StartTime> StartTime;
    [Inject] ComponentDataFromEntity<AudioProperty_Loop> AudioLoops;

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

            #region Remove Properties
            if (AudioClip.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_AudioClipID>(entity);

            if (AudioSpatialBlend.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_SpatialBlend>(entity);

            if (StartTime.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_StartTime>(entity);

            if (AudioLoops.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_Loop>(entity);
            //...
            #endregion
        }

        //Add StopSoundRequests to AudioSources that have done playing.
        for (int i = 0; i < toStop_DonePlayingGroup.Length; i++)
        {
            Entity entity = toStop_DonePlayingGroup.Entities[i];
            AudioSource audioSource = toStop_DonePlayingGroup.AudioSources[i];
            if (!audioSource.isPlaying && audioSource.time == 0) //checking this every frame is not ideal, any better way to do this?
                PostUpdateCommands.AddSharedComponent(entity, new AudioStopRequest());
        }

        //Clean up AudioSourceIDs that are playing virtually.
        for (int i = 0; i < toStop_VirtualGroup.Length; i++)
        {
            Entity entity = toStop_VirtualGroup.Entities[i];
            PostUpdateCommands.RemoveComponent<AudioStopRequest>(entity);
            PostUpdateCommands.RemoveComponent<AudioPlayingVirtually>(entity);
            PostUpdateCommands.RemoveComponent<AudioPlayRequest>(entity);

            #region Remove Properties
            if (AudioClip.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_AudioClipID>(entity);

            if (AudioSpatialBlend.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_SpatialBlend>(entity);

            if (StartTime.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_StartTime>(entity);

            if (AudioLoops.Exists(entity))
                PostUpdateCommands.RemoveComponent<AudioProperty_Loop>(entity);
            //...
            #endregion
        }

        //The virtual groups also need to stop when done playing.
        for (int i = 0; i < toStop_DonePlayingVirtualGroup.Length; i++)
        {
            Entity entity = toStop_DonePlayingVirtualGroup.Entities[i];
            if (AudioClip.Exists(entity))
            {
                AudioClip clip = BootstrapAudio.GetClipList().clips[AudioClip[entity].ID];
                if(AudioSettings.dspTime - toStop_DonePlayingVirtualGroup.StartTimes[i].Time > clip.length)
                    PostUpdateCommands.AddSharedComponent(entity, new AudioStopRequest());
            }          
        }
    }
}
