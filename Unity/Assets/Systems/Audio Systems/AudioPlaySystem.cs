using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;
using System;

[UpdateAfter(typeof(ApplyAudioPropertiesSystem))]
public class AudioPlaySystem : ComponentSystem
{
    struct ToPlayGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public ComponentDataArray<ClaimedByVoice> Claimeds;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
        [ReadOnly] public SharedComponentDataArray<ReadToPlay> ReadyToPlays;
        [ReadOnly] public SubtractiveComponent<Playing> No_Playing;
    }
    [Inject] ToPlayGroup _toPlayGroup;

    struct ToPlayVirtuallyGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<RealVoiceRequest> PlayRequests;
        [ReadOnly] public SubtractiveComponent<RealVoice> No_RealVoice;
        [ReadOnly] public SubtractiveComponent<VirtualVoice> No_VirtualVoice;
    }
    [Inject] ToPlayVirtuallyGroup _toPlayVirtuallyGroup;

    [Inject] ComponentDataFromEntity<DSPTimeOnPlay> _timeOnPlay; //DSPTimeOnPlay is the only property that needs to be applied in the AudioPlaySystem because of the need for precision.
    [Inject] ComponentDataFromEntity<VoiceHandle> _voiceHandle;
    [Inject] ComponentDataFromEntity<InstanceClaimed> _instanceClaimed;
    [Inject] ComponentDataFromEntity<InstanceMuted> _instanceMuted;
    [Inject] ComponentDataFromEntity<InstanceHandle> _instanceHandle;

    protected override void OnUpdate()
    {
        Dictionary<Entity, Tuple<int, int>> cachedCount = new Dictionary<Entity, Tuple<int, int>>();

        for (int i = 0; i < _toPlayGroup.Length; i++)
        {
            Entity sourceEntity = _toPlayGroup.Entities[i];
            Entity voiceEntity = _toPlayGroup.Claimeds[i].VoiceEntity;
            AudioSource audioSource = _toPlayGroup.AudioSources[i];

            Entity instanceEntity = _voiceHandle[voiceEntity].InstanceEntity;
            int previousPlayingCount = _instanceClaimed[instanceEntity].PlayingVoiceCount;
            int previousVirtualCount = _instanceClaimed[instanceEntity].VirtualVoiceCount;
            int totalVoiceCount = _instanceHandle[instanceEntity].VoiceCount;

            if (_timeOnPlay.Exists(sourceEntity)) //de-virtualize voice
            {
                double currentTime = AudioSettings.dspTime;
                double lastTimeStarted = _timeOnPlay[sourceEntity].Time;
                int outputSampleRate = AudioSettings.outputSampleRate;
                int clipSampleRate = audioSource.clip.frequency;
                int clipTotalSamples = audioSource.clip.samples;
                int samplesSinceLastTimeStarted = (int)((currentTime - lastTimeStarted) * outputSampleRate);
                if (clipSampleRate == outputSampleRate)
                    audioSource.timeSamples = samplesSinceLastTimeStarted % clipTotalSamples; //only works when sample rate matches
                else
                    audioSource.timeSamples = (int)((samplesSinceLastTimeStarted % (clipTotalSamples * ((double)outputSampleRate / clipSampleRate))) * ((double)clipSampleRate / outputSampleRate));
                PostUpdateCommands.RemoveComponent<VirtualVoice>(voiceEntity);

                if (totalVoiceCount == 1)
                {
                    PostUpdateCommands.SetComponent(instanceEntity, new InstanceClaimed(1, 0));
                    if (_instanceMuted.Exists(instanceEntity))
                    {
                        PostUpdateCommands.RemoveComponent<InstanceMuted>(instanceEntity);
                        PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceUnmuted(instanceEntity));
                    }
                }
                else
                {
                    if (cachedCount.TryGetValue(instanceEntity, out Tuple<int, int> voiceInfo))
                        cachedCount[instanceEntity] = new Tuple<int, int>(voiceInfo.Item1, voiceInfo.Item2 - 1);
                    else
                        cachedCount.Add(instanceEntity, new Tuple<int, int>(previousPlayingCount, previousVirtualCount - 1));
                }

            }

            else //playing new voice.
            {
                PostUpdateCommands.AddComponent(sourceEntity, new DSPTimeOnPlay(sourceEntity, AudioSettings.dspTime));

                if (totalVoiceCount == 1)
                {
                    PostUpdateCommands.SetComponent(instanceEntity, new InstanceClaimed(1, 0));
                    PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstancePlayed(instanceEntity));
                }
                else
                {
                    if (cachedCount.TryGetValue(instanceEntity, out Tuple<int, int> voiceInfo))
                        cachedCount[instanceEntity] = new Tuple<int, int>(voiceInfo.Item1 + 1, voiceInfo.Item2);
                    else
                        cachedCount.Add(instanceEntity, new Tuple<int, int>(previousPlayingCount + 1, previousVirtualCount));
                }
            }

            audioSource.Play();

            PostUpdateCommands.RemoveComponent<ReadToPlay>(sourceEntity);
            PostUpdateCommands.AddSharedComponent(sourceEntity, new Playing());
        }

        //The voices that didn't get a RealVoice will start playing virtually.
        for (int i = 0; i < _toPlayVirtuallyGroup.Length; i++)
        {
            Entity voiceEntity = _toPlayVirtuallyGroup.Entities[i];
            PostUpdateCommands.AddComponent(voiceEntity, new DSPTimeOnPlay(voiceEntity, AudioSettings.dspTime));
            PostUpdateCommands.AddComponent(voiceEntity, new VirtualVoice(voiceEntity));

            Entity instanceEntity = _voiceHandle[voiceEntity].InstanceEntity;
            int previousPlayingCount = _instanceClaimed[instanceEntity].PlayingVoiceCount;
            int previousVirtualCount = _instanceClaimed[instanceEntity].VirtualVoiceCount;
            int totalVoiceCount = _instanceHandle[instanceEntity].VoiceCount;

            if (totalVoiceCount == 1)
            {
                PostUpdateCommands.SetComponent(instanceEntity, new InstanceClaimed(1, 1));
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstancePlayed(instanceEntity));
            }
            else
            {
                if (cachedCount.TryGetValue(instanceEntity, out Tuple<int, int> voiceInfo))
                    cachedCount[instanceEntity] = new Tuple<int, int>(voiceInfo.Item1 + 1, voiceInfo.Item2 + 1);
                else
                    cachedCount.Add(instanceEntity, new Tuple<int, int>(previousPlayingCount + 1, previousVirtualCount + 1));
            }
        }

        foreach (KeyValuePair<Entity, Tuple<int, int>> pair in cachedCount)
        {
            Entity instanceEntity = pair.Key;
            int newPlayingCount = pair.Value.Item1;
            int newVirtualCount = pair.Value.Item2;
            int totalVoiceCount = _instanceHandle[instanceEntity].VoiceCount;

            PostUpdateCommands.SetComponent(instanceEntity, new InstanceClaimed(newPlayingCount, newVirtualCount));
            if (newPlayingCount >= 1)
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstancePlayed(instanceEntity));
            if (newVirtualCount == totalVoiceCount)
            {
                PostUpdateCommands.AddComponent(instanceEntity, new InstanceMuted(instanceEntity));
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceMuted(instanceEntity));
            }
            if (newVirtualCount == 0 && _instanceMuted.Exists(instanceEntity))
            {
                PostUpdateCommands.RemoveComponent<InstanceMuted>(instanceEntity);
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceUnmuted(instanceEntity));
            }
        }
    }
}