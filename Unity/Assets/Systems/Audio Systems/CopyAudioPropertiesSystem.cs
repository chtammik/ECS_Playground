using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ApplyAudioPropertiesSystem))]
[UpdateAfter(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
public class CopyAudioPropertiesSystem : JobComponentSystem
{
    public class CopyAudioPropertiesBarrier : BarrierSystem { }

    #region CopyMuteRequestJob
    [RequireComponentTag(typeof(AudioMuteRequest))]
    struct CopyMuteRequestJob : IJobProcessComponentData<AudioSourceID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid)
        {
            CommandBuffer.AddSharedComponent(asid.SourceEntity, new AudioMuteRequest());
            CommandBuffer.RemoveComponent<AudioMuteRequest>(asid.GameEntity);
            CommandBuffer.RemoveComponent<AudioSourceID>(asid.GameEntity);
        }
    }
    #endregion

    #region CopyStopRequestJob
    [RequireComponentTag(typeof(AudioStopRequest))]
    struct CopyStopRequestJob : IJobProcessComponentData<AudioSourceID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid)
        {
            CommandBuffer.AddSharedComponent(asid.SourceEntity, new AudioStopRequest());
            CommandBuffer.RemoveComponent<AudioStopRequest>(asid.GameEntity);
            CommandBuffer.RemoveComponent<AudioSourceID>(asid.GameEntity);
        }
    }
    #endregion

    #region CopySpatialBlendJob
    struct CopySpatialBlendJob : IJobProcessComponentData<AudioSourceID, AudioProperty_SpatialBlend>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid, [ReadOnly]ref AudioProperty_SpatialBlend spatialBlend)
        {
            CommandBuffer.AddComponent(asid.SourceEntity, new AudioProperty_SpatialBlend(asid.SourceEntity, spatialBlend.Blend));
            CommandBuffer.RemoveComponent<AudioProperty_SpatialBlend>(asid.GameEntity);
        }
    }
    #endregion

    #region CopyAudioClipIDJob
    struct CopyAudioClipIDJob : IJobProcessComponentData<AudioSourceID, AudioProperty_AudioClipID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid, [ReadOnly]ref AudioProperty_AudioClipID acid)
        {
            CommandBuffer.AddComponent(asid.SourceEntity, new AudioProperty_AudioClipID(asid.SourceEntity, acid.ID));
            CommandBuffer.RemoveComponent<AudioProperty_AudioClipID>(asid.GameEntity);
        }
    }
    #endregion

    #region CopyStartTimeJob
    struct CopyStartTimeJob : IJobProcessComponentData<AudioSourceID, AudioProperty_StartTime>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid, [ReadOnly]ref AudioProperty_StartTime startTime)
        {
            CommandBuffer.AddComponent(asid.SourceEntity, new AudioProperty_StartTime(asid.SourceEntity, startTime.Time));
            CommandBuffer.RemoveComponent<AudioProperty_StartTime>(asid.GameEntity);
        }
    }
    #endregion

    #region CopyLoopJob
    struct CopyLoopJob : IJobProcessComponentData<AudioSourceID, AudioProperty_Loop>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid, [ReadOnly]ref AudioProperty_Loop loop)
        {
            CommandBuffer.AddComponent(asid.SourceEntity, new AudioProperty_Loop(asid.SourceEntity));
            CommandBuffer.RemoveComponent<AudioProperty_Loop>(asid.GameEntity);
        }
    }
    #endregion

    [Inject] CopyAudioPropertiesBarrier copyAudioPropertiesBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var copyMuteRequestJob = new CopyMuteRequestJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copyStopRequestJob = new CopyStopRequestJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copySpatialBlendJob = new CopySpatialBlendJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copyAudioClipIDJob = new CopyAudioClipIDJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copyStartTimeJob = new CopyStartTimeJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copyLoopJob = new CopyLoopJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };

        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(6, Allocator.Temp)
        {
            [0] = copyMuteRequestJob.Schedule(this, inputDeps),
            [1] = copyStopRequestJob.Schedule(this, inputDeps),
            [2] = copySpatialBlendJob.Schedule(this, inputDeps),
            [3] = copyAudioClipIDJob.Schedule(this, inputDeps),
            [4] = copyStartTimeJob.Schedule(this, inputDeps),
            [5] = copyLoopJob.Schedule(this, inputDeps)
        };

        JobHandle combinedDependencies = JobHandle.CombineDependencies(jobHandles);
        jobHandles.Dispose();

        return combinedDependencies;
    }
}
