using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

public class AudioSourcePlaybackSystem : ComponentSystem
{
    struct CarrierGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        public ComponentDataArray<AudioSourceID> ASIDs;
        [ReadOnly] public ComponentDataArray<AudioClipID> ACIDs;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] CarrierGroup carrierGroup;

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
        public EntityArray Entities;
        public ComponentDataArray<AudioSourceID> ASIDs;
        public ComponentDataArray<AudioClipID> ACIDs;
        public ComponentDataArray<StopSoundRequest> StopRequests;
    }

    [Inject] StopGroup stopGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < stopGroup.Length; i++)
        {
            if (stopGroup.ASIDs[i].ASID != -1)
            {
                AudioSource audioSource = poolGroup.Pool[0].GetAudioSource(stopGroup.ASIDs[i].ASID).GetComponent<AudioSource>();
                audioSource.Stop();               
                poolGroup.Pool[0].ReturnIDBack(stopGroup.ASIDs[i].ASID);
            }
            Entity entity = stopGroup.Entities[i];
            PostUpdateCommands.RemoveComponent<AudioSourceID>(entity);
            PostUpdateCommands.RemoveComponent<AudioClipID>(entity);
            PostUpdateCommands.RemoveComponent<StopSoundRequest>(entity);
            AudioInfoGUISystem.ASIDPlayStatus[stopGroup.ASIDs[i].EntityID.Index] = PlayType.Stop;
        }

        for (int i = 0; i < carrierGroup.Length; i++)
        {
            if (carrierGroup.ASIDs[i].PlayStatus == PlayType.ReadyToPlay)
            {
                AudioSource audioSource = poolGroup.Pool[0].GetAudioSource(carrierGroup.ASIDs[i].ASID).GetComponent<AudioSource>();
                audioSource.clip = managerGroup.Manager[0].ClipList.clips[carrierGroup.ACIDs[i].ACID];
                double lastTimeStarted = carrierGroup.AudioProperties[i].StartTime;
                double currentTime = AudioSettings.dspTime;
                int outputSampleRate = AudioSettings.outputSampleRate;
                int clipSampeRate = audioSource.clip.frequency;
                int clipTotalSamples = audioSource.clip.samples;

                if (lastTimeStarted != -1)
                {
                    int samplesSinceLastTimeStarted = (int)((currentTime - lastTimeStarted) * outputSampleRate);
                    if (clipSampeRate == outputSampleRate)
                        audioSource.timeSamples = samplesSinceLastTimeStarted % clipTotalSamples; //only works when sample rate matches
                    else
                        audioSource.timeSamples = (int)((samplesSinceLastTimeStarted % (clipTotalSamples * ((double)outputSampleRate / clipSampeRate))) * ((double)clipSampeRate / outputSampleRate));
                }
                else
                    carrierGroup.AudioProperties[i] = new AudioProperty(AudioSettings.dspTime);
                audioSource.Play();
                carrierGroup.ASIDs[i] = new AudioSourceID(carrierGroup.Entities[i], carrierGroup.ASIDs[i].ASID, carrierGroup.ASIDs[i].Priority, PlayType.Play);
                //PostUpdateCommands.AddComponent<Coloring>(carrierGroup.asIDs[i].EntityID, new Coloring(Color.red));
            }
        }
    }

}
