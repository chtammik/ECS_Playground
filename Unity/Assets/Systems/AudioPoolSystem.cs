using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateBefore(typeof(CopyAudioPropertiesSystem.CopyAudioPropertiesBarrier))]
public class AudioPoolSystem : JobComponentSystem
{
    public class AssignSourceIDBarrier : BarrierSystem { }

    struct PlayRequestGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<PlaySoundRequest> PlayRequests;
        [ReadOnly] public SubtractiveComponent<AudioSourceID> ASIDs;
    }
    [Inject] PlayRequestGroup playRequestGroup;

    struct SourceHandleGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> SourceHandles;
        [ReadOnly] public SubtractiveComponent<AudioSourceClaimed> Claimeds;
    }
    [Inject] SourceHandleGroup sourceHandleGroup;

    struct AssignSourceIDJob : IJobParallelFor
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        [ReadOnly] public ComponentDataArray<PlaySoundRequest> PlayRequests;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> SourceHandles;

        public void Execute(int index)
        {
            CommandBuffer.AddComponent(PlayRequests[index].Entity, new AudioSourceID(PlayRequests[index].Entity, SourceHandles[index].Entity, SourceHandles[index].ASID));
            CommandBuffer.AddSharedComponent(SourceHandles[index].Entity, new AudioSourceClaimed());
            CommandBuffer.RemoveComponent<PlaySoundRequest>(PlayRequests[index].Entity);
        }
    }

    [Inject] AssignSourceIDBarrier assignSourceIDBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int jobAmount = math.min(playRequestGroup.Length, sourceHandleGroup.Length);
        var newJob = new AssignSourceIDJob
        {
            CommandBuffer = assignSourceIDBarrier.CreateCommandBuffer(),
            PlayRequests = playRequestGroup.PlayRequests,
            SourceHandles = sourceHandleGroup.SourceHandles
        };
        return newJob.Schedule(jobAmount, 64, inputDeps);
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
