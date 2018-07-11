using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

public class AudioPoolSystem : ComponentSystem
{
    static AudioSourcePool ThePool;

    struct PoolGroup
    {
        public ComponentArray<AudioSourcePool> Pool;
    }

    [Inject] PoolGroup poolGroup;

    struct ASIDGroup
    {
        public readonly int Length;
        public ComponentDataArray<AudioSourceID> ASIDs;
        public ComponentDataArray<AudioProperty> AudioProperties;
    }

    [Inject] ASIDGroup asidGroup;

    protected override void OnStartRunning()
    {
        UpdateInjectedComponentGroups();
        ThePool = poolGroup.Pool[0];
    }

    protected override void OnUpdate()
    {
        for (int i = 0; i < asidGroup.Length; i++)
        {
            if (asidGroup.ASIDs[i].PlayStatus == PlayType.NeedSource)
            {
                int newID = poolGroup.Pool[0].GetNewID();
                if (newID != -1)
                    asidGroup.ASIDs[i] = new AudioSourceID(asidGroup.ASIDs[i].EntityID, newID, asidGroup.ASIDs[i].Priority, PlayType.ReadyToPlay);
            }
        }
    }

    public static void AddAudioSourceID(EntityCommandBuffer commandBuffer, Entity entity, int audioClipID)
    {
        if (ThePool != null && ThePool.SourceAvailable)
            commandBuffer.AddComponent<AudioSourceID>(entity, new AudioSourceID(entity, ThePool.GetNewID(), PriorityType.Medium, PlayType.ReadyToPlay));
        else
            commandBuffer.AddComponent<AudioSourceID>(entity, new AudioSourceID(entity, -1, PriorityType.Medium, PlayType.NeedSource));
        commandBuffer.AddComponent<AudioClipID>(entity, new AudioClipID(audioClipID));
    }

    public static void SetAudioSourceID(EntityCommandBuffer commandBuffer, Entity entity)
    {
        if (ThePool != null && ThePool.SourceAvailable)
            commandBuffer.SetComponent<AudioSourceID>(entity, new AudioSourceID(entity, ThePool.GetNewID(), PriorityType.Medium, PlayType.ReadyToPlay));
        else
            commandBuffer.SetComponent<AudioSourceID>(entity, new AudioSourceID(entity, -1, PriorityType.Medium, PlayType.NeedSource));
    }
}
