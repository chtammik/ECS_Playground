using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(AssignAudioSourceSystem.AssignSourceIDBarrier))]
public class AudioMuteSystem : ComponentSystem
{
    struct AudioSourceGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SharedComponentDataArray<MuteRequest> VirtualizeRequests;
        [ReadOnly] public SharedComponentDataArray<Playing> Playing;
        [ReadOnly] public ComponentDataArray<ClaimedByVoice> Claimed;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] AudioSourceGroup _audioSourceGroup;

    [Inject] ComponentDataFromEntity<RealVoiceRequest> _realVoiceRequest;
    [Inject] ComponentDataFromEntity<AudioProperty_AudioClipID> _audioClip;
    [Inject] ComponentDataFromEntity<AudioProperty_SpatialBlend> _audioSpatialBlend;
    [Inject] ComponentDataFromEntity<DSPTimeOnPlay> _timeOnPlay;
    [Inject] ComponentDataFromEntity<AudioProperty_Loop> _audioLoops;

    [Inject] ComponentDataFromEntity<VoiceHandle> _voiceHandle;
    [Inject] ComponentDataFromEntity<InstanceClaimed> _instanceClaimed;
    [Inject] ComponentDataFromEntity<InstanceHandle> _instanceHandle;

    protected override void OnUpdate()
    {
        Dictionary<Entity, Tuple<int, int>> cachedCount = new Dictionary<Entity, Tuple<int, int>>();

        for (int i = 0; i < _audioSourceGroup.Length; i++)
        {
            Entity sourceEntity = _audioSourceGroup.Entities[i];
            Entity voiceEntity = _audioSourceGroup.Claimed[i].VoiceEntity;
            AudioSource audioSource = _audioSourceGroup.AudioSources[i];
            audioSource.Stop();
            AudioService.ResetAudioSource(audioSource);
            PostUpdateCommands.RemoveComponent<MuteRequest>(sourceEntity);
            PostUpdateCommands.RemoveComponent<Playing>(sourceEntity);
            PostUpdateCommands.RemoveComponent<ClaimedByVoice>(sourceEntity);

            if (_realVoiceRequest.Exists(sourceEntity))
                PostUpdateCommands.RemoveComponent<RealVoiceRequest>(sourceEntity);

            if (_timeOnPlay.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(voiceEntity, new DSPTimeOnPlay(voiceEntity, _timeOnPlay[sourceEntity].Time));
                PostUpdateCommands.RemoveComponent<DSPTimeOnPlay>(sourceEntity);
            }

            //copy Audio Properties back to voice handles for the later devirtualizing.
            #region Copy Properties
            if (_audioClip.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(voiceEntity, new AudioProperty_AudioClipID(voiceEntity, _audioClip[sourceEntity].ID));
                PostUpdateCommands.RemoveComponent<AudioProperty_AudioClipID>(sourceEntity);
            }

            if (_audioSpatialBlend.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(voiceEntity, new AudioProperty_SpatialBlend(voiceEntity, _audioSpatialBlend[sourceEntity].Blend));
                PostUpdateCommands.RemoveComponent<AudioProperty_SpatialBlend>(sourceEntity);
            }

            if (_audioLoops.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(voiceEntity, new AudioProperty_Loop(voiceEntity));
                PostUpdateCommands.RemoveComponent<AudioProperty_Loop>(sourceEntity);
            }

            //...
            #endregion

            PostUpdateCommands.RemoveComponent<RealVoice>(voiceEntity);
            PostUpdateCommands.AddComponent(voiceEntity, new VirtualVoice(voiceEntity));

            Entity instanceEntity = _voiceHandle[voiceEntity].InstanceEntity;
            int previousPlayingCount = _instanceClaimed[instanceEntity].PlayingVoiceCount;
            int previousVirtualCount = _instanceClaimed[instanceEntity].VirtualVoiceCount;
            int totalVoiceCount = _instanceHandle[instanceEntity].VoiceCount;

            if (totalVoiceCount == 1)
            {
                PostUpdateCommands.SetComponent(instanceEntity, new InstanceClaimed(1, 1));
                PostUpdateCommands.AddComponent(instanceEntity, new InstanceMuted(instanceEntity));
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceMuted(instanceEntity));
            }
            else
            {
                if (cachedCount.TryGetValue(instanceEntity, out Tuple<int, int> voiceInfo))
                    cachedCount[instanceEntity] = new Tuple<int, int>(voiceInfo.Item1, voiceInfo.Item2 + 1);
                else
                    cachedCount.Add(instanceEntity, new Tuple<int, int>(previousPlayingCount, previousVirtualCount + 1));
            }
        }

        foreach (KeyValuePair<Entity, Tuple<int, int>> pair in cachedCount)
        {
            Entity instanceEntity = pair.Key;
            int newPlayingCount = pair.Value.Item1;
            int newVirtualCount = pair.Value.Item2;
            int totalVoiceCount = _instanceHandle[instanceEntity].VoiceCount;

            PostUpdateCommands.SetComponent(instanceEntity, new InstanceClaimed(newPlayingCount, newVirtualCount));
            if (newVirtualCount == totalVoiceCount)
            {
                PostUpdateCommands.AddComponent(instanceEntity, new InstanceMuted(instanceEntity));
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceMuted(instanceEntity));
            }               
        }
    }
}
