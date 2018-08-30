using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(AssignAudioSourceSystem.AssignSourceIDBarrier))]
public class AudioStopSystem : ComponentSystem
{
    struct ToStopGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SharedComponentDataArray<StopRequest> StopRequests;
        [ReadOnly] public SharedComponentDataArray<Playing> Playing;
        [ReadOnly] public ComponentDataArray<ClaimedByVoice> Claimed;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] ToStopGroup _toStopGroup;

    struct ToStop_DonePlayingGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SubtractiveComponent<StopRequest> No_StopRequests;
        [ReadOnly] public SubtractiveComponent<AudioProperty_Loop> No_Loop;
        [ReadOnly] public SharedComponentDataArray<Playing> Playing;
        [ReadOnly] public ComponentDataArray<ClaimedByVoice> Claimed;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] ToStop_DonePlayingGroup _toStop_DonePlayingGroup;

    [Inject] ComponentDataFromEntity<VoiceHandle> _voiceHandle;
    [Inject] ComponentDataFromEntity<InstanceClaimed> _instanceClaimed;
    [Inject] ComponentDataFromEntity<InstanceHandle> _instanceHandle;

    protected override void OnUpdate()
    {
        Dictionary<Entity, Tuple<int, int>> cachedCount = new Dictionary<Entity, Tuple<int, int>>();

        //Stop AudioSources that already have StopSoundRequest on it, then remove the StopSoundRequest.
        for (int i = 0; i < _toStopGroup.Length; i++)
        {
            Entity sourceEntity = _toStopGroup.Entities[i];
            Entity voiceEntity = _toStopGroup.Claimed[i].VoiceEntity;
            AudioSource audioSource = _toStopGroup.AudioSources[i];
            audioSource.Stop();
            AudioService.ResetAudioSource(audioSource);
            PostUpdateCommands.RemoveComponent<StopRequest>(sourceEntity);
            PostUpdateCommands.RemoveComponent<Playing>(sourceEntity);
            PostUpdateCommands.RemoveComponent<RealVoice>(voiceEntity);
            PostUpdateCommands.RemoveComponent<ClaimedByVoice>(sourceEntity);

            Entity instanceEntity = _voiceHandle[voiceEntity].InstanceEntity;
            int previousPlayingCount = _instanceClaimed[instanceEntity].PlayingVoiceCount;
            int previousVirtualCount = _instanceClaimed[instanceEntity].VirtualVoiceCount;
            int totalVoiceCount = _instanceHandle[instanceEntity].VoiceCount;

            if (totalVoiceCount == 1)
            {
                PostUpdateCommands.RemoveComponent<InstanceClaimed>(instanceEntity);
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceStopped(instanceEntity));
            }
            else
            {
                if (cachedCount.TryGetValue(instanceEntity, out Tuple<int, int> voiceInfo))
                    cachedCount[instanceEntity] = new Tuple<int, int>(voiceInfo.Item1 - 1, voiceInfo.Item2);
                else
                    cachedCount.Add(instanceEntity, new Tuple<int, int>(previousPlayingCount - 1, previousVirtualCount));
            }
        }

        foreach (KeyValuePair<Entity, Tuple<int, int>> pair in cachedCount)
        {
            Entity instanceEntity = pair.Key;
            int newPlayingCount = pair.Value.Item1;
            int newVirtualCount = pair.Value.Item2;

            if (newPlayingCount == 0)
            {
                PostUpdateCommands.RemoveComponent<InstanceClaimed>(instanceEntity);
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceStopped(instanceEntity));
            }         
        }

        //Add StopSoundRequests to AudioSources that have done playing.
        for (int i = 0; i < _toStop_DonePlayingGroup.Length; i++)
        {
            Entity entity = _toStop_DonePlayingGroup.Claimed[i].VoiceEntity;
            AudioSource audioSource = _toStop_DonePlayingGroup.AudioSources[i];
            if (!audioSource.isPlaying && audioSource.time == 0) //TODO: checking this every frame is not ideal, any better way to do this?
                PostUpdateCommands.AddSharedComponent(entity, new StopRequest());
        }

    }
}
