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
        [ReadOnly] public ComponentDataArray<StopSoundRequest> StopRequests;
        public ComponentDataArray<AudioSourceID> ASIDs;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] StopGroup stopGroup;

    struct StopFromPlayGroup
    {
        public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<StopSoundRequest> StopRequests;
        [ReadOnly] public ComponentDataArray<PlaySoundRequest> PlayRequests;
        public ComponentDataArray<AudioSourceID> ASIDs;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] StopFromPlayGroup stopFromPlayGroup;

    struct MuteGroup
    {
        public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<MuteSoundRequest> MuteRequests;
        public ComponentDataArray<AudioSourceID> ASIDs;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] MuteGroup muteGroup;

    struct PlayGroup
    {
        public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<PlaySoundRequest> PlayRequests;
        public ComponentDataArray<AudioSourceID> ASIDs;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] PlayGroup playGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < stopFromPlayGroup.Length; i++)
        {
            AudioSourceID asID = stopFromPlayGroup.ASIDs[i];
            stopFromPlayGroup.ASIDs[i] = new AudioSourceID(asID.EntityID, -1, asID.Priority, PlayType.Stop);
            stopFromPlayGroup.AudioProperties[i] = new AudioProperty(-1, -1);
            Entity entity = stopFromPlayGroup.Entities[i];
            PostUpdateCommands.RemoveComponent<StopSoundRequest>(entity);
            PostUpdateCommands.RemoveComponent<PlaySoundRequest>(entity);
            AudioInfoGUISystem.ASIDPlayStatus[entity.Index] = PlayType.Stop;
        }

        for (int i = 0; i < stopGroup.Length; i++)
        {
            AudioSourceID asID = stopGroup.ASIDs[i];
            if(asID.PlayStatus != PlayType.Stop)
            {
                if (asID.VoiceStatus == VoiceStatusType.Real)
                {
                    AudioSource audioSource = poolGroup.Pool[0].GetAudioSource(asID.ASID).GetComponent<AudioSource>();
                    audioSource.Stop();
                    audioSource.timeSamples = 0;
                    poolGroup.Pool[0].ReturnIDBack(asID.ASID);
                }
                stopGroup.ASIDs[i] = new AudioSourceID(asID.EntityID, -1, asID.Priority, PlayType.Stop);
                stopGroup.AudioProperties[i] = new AudioProperty(-1, -1);
                Entity entity = stopGroup.Entities[i];
                PostUpdateCommands.RemoveComponent<StopSoundRequest>(entity);
                AudioInfoGUISystem.ASIDPlayStatus[entity.Index] = PlayType.Stop;
            }
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
            int newClipID = playGroup.PlayRequests[i].AudioClipID;
            int currentClipID = playGroup.AudioProperties[i].AudioClipID;
            double lastTimeStarted = playGroup.AudioProperties[i].StartTime;

            if (asID.PlayStatus == PlayType.NeedSource && lastTimeStarted == -1)
            {
                if (newClipID != -1)
                    playGroup.AudioProperties[i] = new AudioProperty(AudioSettings.dspTime, newClipID);
                else
                    playGroup.AudioProperties[i] = new AudioProperty(AudioSettings.dspTime, currentClipID);
            }

            if (asID.PlayStatus == PlayType.ReadyToPlay)
            {
                AudioSource audioSource = poolGroup.Pool[0].GetAudioSource(asID.ASID).GetComponent<AudioSource>();
                double currentTime = AudioSettings.dspTime;

                if (newClipID != -1)
                    audioSource.clip = managerGroup.Manager[0].ClipList.clips[newClipID];
                else
                    audioSource.clip = managerGroup.Manager[0].ClipList.clips[currentClipID];

                if (lastTimeStarted != -1)
                {
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
                    playGroup.AudioProperties[i] = new AudioProperty(currentTime, newClipID);
                audioSource.Play();
                playGroup.ASIDs[i] = new AudioSourceID(asID.EntityID, asID.ASID, asID.Priority, PlayType.Play);
                PostUpdateCommands.RemoveComponent<PlaySoundRequest>(playGroup.Entities[i]);
                //Debug.Log(AudioSettings.dspTime);
                //PostUpdateCommands.AddComponent<Coloring>(carrierGroup.asIDs[i].EntityID, new Coloring(Color.red));
            }
        }
    }

}
