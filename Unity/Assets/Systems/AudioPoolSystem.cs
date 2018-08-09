using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateBefore(typeof(CopyAudioPropertiesSystem.CopyAudioPropertiesBarrier))]
public class AudioPoolSystem : JobComponentSystem
{
    public class AssignSourceIDBarrier : BarrierSystem { }

    struct PlayRequestGroup //all entities that need to play a sound.
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioPlayRequest> PlayRequests;
        [ReadOnly] public SubtractiveComponent<AudioSourceID> ASIDs;
    }
    [Inject] PlayRequestGroup playRequestGroup;

    struct SourceHandleGroup //all vacant sources
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> SourceHandles;
        [ReadOnly] public SubtractiveComponent<AudioSourceClaimed> ClaimedTags;
    }
    [Inject] SourceHandleGroup sourceHandleGroup;

    struct AssignSourceIDJob : IJobParallelFor
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        [ReadOnly] public ComponentDataArray<AudioPlayRequest> PlayRequests;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> SourceHandles;

        public void Execute(int index)
        {
            CommandBuffer.AddComponent(PlayRequests[index].Entity, new AudioSourceID(PlayRequests[index].Entity, SourceHandles[index].HandleEntity));
            CommandBuffer.SetComponent(SourceHandles[index].HandleEntity, new AudioSourceHandle(PlayRequests[index].Entity, SourceHandles[index].HandleEntity));
            CommandBuffer.AddSharedComponent(SourceHandles[index].HandleEntity, new AudioSourceClaimed());
            CommandBuffer.RemoveComponent<AudioPlayRequest>(PlayRequests[index].Entity);
        }
    }

    [Inject] AssignSourceIDBarrier assignSourceIDBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int jobAmount = math.min(playRequestGroup.Length, sourceHandleGroup.Length);
        var assignSourceIDJob = new AssignSourceIDJob
        {
            CommandBuffer = assignSourceIDBarrier.CreateCommandBuffer(),
            PlayRequests = playRequestGroup.PlayRequests,
            SourceHandles = sourceHandleGroup.SourceHandles
        };
        JobHandle assignSourceIDJH = assignSourceIDJob.Schedule(jobAmount, 64, inputDeps);

        return assignSourceIDJH;
    }
}

//public class AudioPoolSystem : ComponentSystem
//{
//    struct PlayRequestGroup
//    {
//        public readonly int Length;
//        public EntityArray Entities;
//        [ReadOnly] public ComponentDataArray<PlaySoundRequest> PlayRequests;
//        public SubtractiveComponent<AudioSourceID> ASIDs;
//    }

//    [Inject] PlayRequestGroup playRequestGroup;

//    struct SourceHandleGroup
//    {
//        public readonly int Length;
//        public EntityArray Entities;
//        [ReadOnly] public ComponentDataArray<AudioSourceHandle> SourceHandles;
//        public SubtractiveComponent<AudioSourceClaimed> Claimeds;
//    }

//    [Inject] SourceHandleGroup sourceHandleGroup;

//    protected override void OnUpdate()
//    {
//        int HandleAmount = sourceHandleGroup.Length;
//        for (int i = 0; i < playRequestGroup.Length; i++)
//        {
//            if (HandleAmount > 0)
//            {
//                int HandleIndex = HandleAmount - 1;
//                Entity entity = playRequestGroup.Entities[i];
//                PostUpdateCommands.AddComponent(entity, new AudioSourceID(entity, sourceHandleGroup.SourceHandles[HandleIndex].ASID));
//                PostUpdateCommands.RemoveComponent<PlaySoundRequest>(entity);
//                PostUpdateCommands.AddComponent(entity, new ReadyToPlay());
//                PostUpdateCommands.AddComponent(sourceHandleGroup.Entities[HandleIndex], new AudioSourceClaimed());
//                --HandleAmount;
//            }
//        }
//    }
//}
