using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class AudioSourcePlaybackSystem : ComponentSystem
{
    struct PoolGroup
    {
        [ReadOnly] public ComponentArray<AudioSourcePool> Pool;
    }

    [Inject] PoolGroup poolGroup;

    struct ManagerGroup
    {
        [ReadOnly] public ComponentArray<AudioManager> Manager;
    }

    [Inject] ManagerGroup managerGroup;

    struct StopGroup
    {
        public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        public ComponentDataArray<AudioSourceID> ASIDs;
        [ReadOnly] public ComponentDataArray<AudioClipID> ACIDs;
        [ReadOnly] public ComponentDataArray<StopSoundRequest> StopRequests;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] StopGroup stopGroup;

    struct MuteGroup
    {
        public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        public ComponentDataArray<AudioSourceID> ASIDs;
        [ReadOnly] public ComponentDataArray<MuteSoundRequest> MuteRequests;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] MuteGroup muteGroup;

    struct PlayGroup
    {
        public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        public ComponentDataArray<AudioSourceID> ASIDs;
        [ReadOnly] public ComponentDataArray<AudioClipID> ACIDs;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] PlayGroup playGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < stopGroup.Length; i++)
        {
            AudioSourceID asID = stopGroup.ASIDs[i];
            if (asID.VoiceStatus == VoiceStatusType.Real)
            {
                AudioSource audioSource = poolGroup.Pool[0].GetAudioSource(asID.ASID).GetComponent<AudioSource>();
                audioSource.Stop();
                audioSource.timeSamples = 0;
                poolGroup.Pool[0].ReturnIDBack(asID.ASID);
            }
            stopGroup.ASIDs[i] = new AudioSourceID(asID.EntityID, -1, asID.Priority, PlayType.Stop);
            stopGroup.AudioProperties[i] = new AudioProperty(-1);
            Entity entity = stopGroup.Entities[i];
            PostUpdateCommands.RemoveComponent<AudioSourceID>(entity);
            PostUpdateCommands.RemoveComponent<AudioClipID>(entity);
            PostUpdateCommands.RemoveComponent<StopSoundRequest>(entity);
            AudioInfoGUISystem.ASIDPlayStatus[entity.Index] = PlayType.Stop;
        }

        for (int i = 0; i < muteGroup.Length; i++)
        {
            AudioSourceID asID = muteGroup.ASIDs[i];
            if (asID.VoiceStatus == VoiceStatusType.Real)
            {
                AudioSource audioSource = poolGroup.Pool[0].GetAudioSource(asID.ASID).GetComponent<AudioSource>();
                audioSource.Stop();
                poolGroup.Pool[0].ReturnIDBack(asID.ASID);
            }
            muteGroup.ASIDs[i] = new AudioSourceID(asID.EntityID, -1, asID.Priority, PlayType.Mute);
            Entity entity = muteGroup.Entities[i];
            PostUpdateCommands.RemoveComponent<MuteSoundRequest>(entity);
        }

        for (int i = 0; i < playGroup.Length; i++)
        {
            AudioSourceID asID = playGroup.ASIDs[i];
            if (asID.PlayStatus == PlayType.NeedSource && playGroup.AudioProperties[i].StartTime == -1)
                playGroup.AudioProperties[i] = new AudioProperty(AudioSettings.dspTime);

            if (asID.PlayStatus == PlayType.ReadyToPlay)
            {
                AudioSource audioSource = poolGroup.Pool[0].GetAudioSource(asID.ASID).GetComponent<AudioSource>();
                audioSource.clip = managerGroup.Manager[0].ClipList.clips[playGroup.ACIDs[i].ACID];

                double lastTimeStarted = playGroup.AudioProperties[i].StartTime;
                double currentTime = AudioSettings.dspTime;
                int outputSampleRate = AudioSettings.outputSampleRate;
                int clipSampleRate = audioSource.clip.frequency;
                int clipTotalSamples = audioSource.clip.samples;

                if (lastTimeStarted != -1)
                {
                    int samplesSinceLastTimeStarted = (int)((currentTime - lastTimeStarted) * outputSampleRate);
                    if (clipSampleRate == outputSampleRate)
                        audioSource.timeSamples = samplesSinceLastTimeStarted % clipTotalSamples; //only works when sample rate matches
                    else
                        audioSource.timeSamples = (int)((samplesSinceLastTimeStarted % (clipTotalSamples * ((double)outputSampleRate / clipSampleRate))) * ((double)clipSampleRate / outputSampleRate));
                }
                else
                    playGroup.AudioProperties[i] = new AudioProperty(AudioSettings.dspTime);
                audioSource.Play();
                playGroup.ASIDs[i] = new AudioSourceID(asID.EntityID, asID.ASID, asID.Priority, PlayType.Play);
                //Debug.Log(AudioSettings.dspTime);
                //PostUpdateCommands.AddComponent<Coloring>(carrierGroup.asIDs[i].EntityID, new Coloring(Color.red));
            }
        }
    }

}
