﻿using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(ApplyAudioPropertiesSystem))]
public class AudioPlaySystem : ComponentSystem
{
    struct ToPlayGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SharedComponentDataArray<AudioSourceClaimed> Claimeds;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
        [ReadOnly] public SharedComponentDataArray<AudioReadyToPlay> ReadyToPlays;
        [ReadOnly] public SubtractiveComponent<AudioPlaying> PlayingTags;
    }
    [Inject] ToPlayGroup toPlayGroup;

    struct ToPlayVirtuallyGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<AudioPlayRequest> PlayRequests;
        [ReadOnly] public SubtractiveComponent<AudioSourceID> ASIDs;
        [ReadOnly] public SubtractiveComponent<AudioPlayingVirtually> VirtualTags;
    }
    [Inject] ToPlayVirtuallyGroup toPlayVirtuallyGroup;

    [Inject] ComponentDataFromEntity<AudioProperty_StartTime> StartTime;

    protected override void OnUpdate()
    {
        for (int i = 0; i < toPlayGroup.Length; i++)
        {
            Entity sourceEntity = toPlayGroup.Entities[i];
            AudioSource audioSource = toPlayGroup.AudioSources[i];

            if (StartTime.Exists(sourceEntity)) //StartTime is the only property that needs to be applied in the AudioPlaySystem because of the need for precision.
            {
                double currentTime = AudioSettings.dspTime;
                double lastTimeStarted = StartTime[sourceEntity].Time;
                int outputSampleRate = AudioSettings.outputSampleRate;
                int clipSampleRate = audioSource.clip.frequency;
                int clipTotalSamples = audioSource.clip.samples;
                int samplesSinceLastTimeStarted = (int)((currentTime - lastTimeStarted) * outputSampleRate);
                if (clipSampleRate == outputSampleRate)
                    audioSource.timeSamples = samplesSinceLastTimeStarted % clipTotalSamples; //only works when sample rate matches
                else
                    audioSource.timeSamples = (int)((samplesSinceLastTimeStarted % (clipTotalSamples * ((double)outputSampleRate / clipSampleRate))) * ((double)clipSampleRate / outputSampleRate));
            }
            else
                PostUpdateCommands.AddComponent(sourceEntity, new AudioProperty_StartTime(sourceEntity, AudioSettings.dspTime));

            audioSource.Play();

            PostUpdateCommands.RemoveComponent<AudioReadyToPlay>(sourceEntity);
            PostUpdateCommands.AddSharedComponent(sourceEntity, new AudioPlaying());
        }

        for (int i = 0; i < toPlayVirtuallyGroup.Length; i++)
        {
            Entity gameEntity = toPlayVirtuallyGroup.Entities[i];
            PostUpdateCommands.AddComponent(gameEntity, new AudioProperty_StartTime(gameEntity, AudioSettings.dspTime));
            PostUpdateCommands.AddComponent(gameEntity, new AudioPlayingVirtually(gameEntity));
        }
    }
}