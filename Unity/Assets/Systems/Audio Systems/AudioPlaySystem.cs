using Unity.Collections;
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
        [ReadOnly] public ComponentDataArray<ClaimedByVoice> Claimeds;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
        [ReadOnly] public SharedComponentDataArray<AudioReadyToPlay> ReadyToPlays;
        [ReadOnly] public SubtractiveComponent<AudioPlaying> No_AudioPlaying;
    }
    [Inject] ToPlayGroup _toPlayGroup;

    struct ToPlayVirtuallyGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<AudioPlayRequest> PlayRequests;
        [ReadOnly] public SubtractiveComponent<RealVoice> No_RealVoice;
        [ReadOnly] public SubtractiveComponent<VirtualVoice> No_VirtualVoice;
    }
    [Inject] ToPlayVirtuallyGroup _toPlayVirtuallyGroup;

    [Inject] ComponentDataFromEntity<DSPTimeOnPlay> _timeOnPlay;

    protected override void OnUpdate()
    {
        for (int i = 0; i < _toPlayGroup.Length; i++)
        {
            Entity sourceEntity = _toPlayGroup.Entities[i];
            Entity voiceEntity = _toPlayGroup.Claimeds[i].VocieEntity;
            AudioSource audioSource = _toPlayGroup.AudioSources[i];

            if (_timeOnPlay.Exists(sourceEntity)) //DSPTimeOnPlay is the only property that needs to be applied in the AudioPlaySystem because of the need for precision.
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
                PostUpdateCommands.AddComponent(voiceEntity, new AudioMessage_Unmuted(voiceEntity));
            }
            else
            {
                PostUpdateCommands.AddComponent(sourceEntity, new DSPTimeOnPlay(sourceEntity, AudioSettings.dspTime));
                PostUpdateCommands.AddComponent(voiceEntity, new AudioMessage_Played(voiceEntity));
            }
                
            audioSource.Play();

            PostUpdateCommands.RemoveComponent<AudioReadyToPlay>(sourceEntity);
            PostUpdateCommands.AddSharedComponent(sourceEntity, new AudioPlaying());
        }

        //The voices that didn't get a RealVoice will start playing virtually.
        for (int i = 0; i < _toPlayVirtuallyGroup.Length; i++)
        {
            Entity voiceEntity = _toPlayVirtuallyGroup.Entities[i];
            PostUpdateCommands.AddComponent(voiceEntity, new DSPTimeOnPlay(voiceEntity, AudioSettings.dspTime));
            PostUpdateCommands.AddComponent(voiceEntity, new VirtualVoice(voiceEntity));
            PostUpdateCommands.AddComponent(voiceEntity, new AudioMessage_Played(voiceEntity));
            PostUpdateCommands.AddComponent(voiceEntity, new AudioMessage_Muted(voiceEntity));
        }
    }
}
