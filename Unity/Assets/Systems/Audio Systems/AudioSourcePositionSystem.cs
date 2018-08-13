using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

//Non-Job version
public class AudioSourcePositionSystem : ComponentSystem
{
    struct ASIDGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<Position> Positions;
        [ReadOnly] public ComponentDataArray<AudioSourceID> ASIDs;
    }
    [Inject] ASIDGroup asidGroup;

    [Inject] ComponentDataFromEntity<Position> SourcePosition;

    protected override void OnUpdate()
    {
        for (int i = 0; i < asidGroup.Length; i++)
        {
            SourcePosition[asidGroup.ASIDs[i].SourceEntity] = asidGroup.Positions[i];
        }
    }
}

//IJob version, actually way slower.
//public class AudioSourcePositionSystem : JobComponentSystem
//{
//    struct ASIDGroup
//    {
//        public readonly int Length;
//        [ReadOnly] public ComponentDataArray<Position> Positions;
//        [ReadOnly] public ComponentDataArray<AudioSourceID> ASIDs;
//    }
//    [Inject] ASIDGroup asidGroup;

//    [Inject] ComponentDataFromEntity<Position> sourcePosition;

//    struct AudioSourcePositionJob : IJob
//    {
//        [WriteOnly] public Position SourcePosition;
//        [ReadOnly] public Position ASIDPosition;

//        public void Execute()
//        {
//            SourcePosition = ASIDPosition;
//        }
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(asidGroup.Length, Allocator.Temp);

//        for (int i = 0; i < asidGroup.Length; i++)
//        {
//            var audioSourcePositionJob = new AudioSourcePositionJob
//            {
//                SourcePosition = sourcePosition[asidGroup.ASIDs[i].SourceEntity],
//                ASIDPosition = asidGroup.Positions[i]
//            };
//            jobHandles[i] = audioSourcePositionJob.Schedule(inputDeps);
//        }

//        JobHandle combinedDeps = JobHandle.CombineDependencies(jobHandles);
//        jobHandles.Dispose();

//        return combinedDeps;
//    }
//}
