using UnityEngine;
using System.Collections;
using Unity.Entities;

public class AudioSourcePlaybackSystem : ComponentSystem
{
    struct CarrierGroup
    {
        public readonly int Length;
        public ComponentDataArray<AudioSourceID> asIDs;
        public ComponentDataArray<AudioClipID> acIDs;
    }

    [Inject] CarrierGroup carrierGroup;

    struct PoolGroup
    {
        public ComponentArray<AudioSourcePool> Pool;
    }

    [Inject] PoolGroup poolGroup;

    struct ManagerGroup
    {
        public ComponentArray<AudioManager> Manager;
    }

    [Inject] ManagerGroup managerGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < carrierGroup.Length; i++)
        {
            if (carrierGroup.asIDs[i].PlayStatus == PlayType.ReadyToPlay)
            {
                AudioSource audioSource = poolGroup.Pool[0].GetAudioSource(carrierGroup.asIDs[i].ASID).GetComponent<AudioSource>();
                audioSource.clip = managerGroup.Manager[0].ClipList.clips[carrierGroup.acIDs[i].ACID];
                audioSource.Play();
                carrierGroup.asIDs[i] = new AudioSourceID(carrierGroup.asIDs[i].EntityID, carrierGroup.asIDs[i].ASID, carrierGroup.asIDs[i].Priority, PlayType.Play);
                //PostUpdateCommands.AddComponent<Coloring>(carrierGroup.asIDs[i].EntityID, new Coloring(Color.red));
            }
        }
    }

}
