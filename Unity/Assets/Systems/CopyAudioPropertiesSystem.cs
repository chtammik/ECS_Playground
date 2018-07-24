using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ApplyAudioPropertiesSystem))]
[UpdateAfter(typeof(AudioPoolSystem.AssignSourceIDBarrier))]
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
            CommandBuffer.AddSharedComponent(asid.HandleEntity, new AudioMuteRequest());
            CommandBuffer.RemoveComponent<AudioMuteRequest>(asid.OriginalEntity);
            CommandBuffer.RemoveComponent<AudioSourceID>(asid.OriginalEntity);
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
            CommandBuffer.AddSharedComponent(asid.HandleEntity, new AudioStopRequest());
            CommandBuffer.RemoveComponent<AudioStopRequest>(asid.OriginalEntity);
            CommandBuffer.RemoveComponent<AudioSourceID>(asid.OriginalEntity);
        }
    }
    #endregion

    #region CopySpatialBlendJob
    struct CopySpatialBlendJob : IJobProcessComponentData<AudioSourceID, AudioProperty_SpatialBlend>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid, [ReadOnly]ref AudioProperty_SpatialBlend spatialBlend)
        {
            CommandBuffer.AddComponent(asid.HandleEntity, spatialBlend);
            CommandBuffer.RemoveComponent<AudioProperty_SpatialBlend>(asid.OriginalEntity);
        }
    }
    #endregion

    #region CopyAudioClipIDJob
    struct CopyAudioClipIDJob : IJobProcessComponentData<AudioSourceID, AudioProperty_AudioClipID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid, [ReadOnly]ref AudioProperty_AudioClipID acid)
        {
            CommandBuffer.AddComponent(asid.HandleEntity, acid);
            CommandBuffer.RemoveComponent<AudioProperty_AudioClipID>(asid.OriginalEntity);
        }
    }
    #endregion

    #region CopyStartTimeJob
    struct CopyStartTimeJob : IJobProcessComponentData<AudioSourceID, AudioProperty_StartTime>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid, [ReadOnly]ref AudioProperty_StartTime startTime)
        {
            CommandBuffer.AddComponent(asid.HandleEntity, startTime);
            CommandBuffer.RemoveComponent<AudioProperty_StartTime>(asid.OriginalEntity);
        }
    }
    #endregion

    #region CopyLoopJob
    struct CopyLoopJob : IJobProcessComponentData<AudioSourceID, AudioProperty_Loop>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid, [ReadOnly]ref AudioProperty_Loop loop)
        {
            CommandBuffer.AddComponent(asid.HandleEntity, loop);
            CommandBuffer.RemoveComponent<AudioProperty_Loop>(asid.OriginalEntity);
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

        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(7, Allocator.Temp)
        {
            [0] = inputDeps,
            [1] = copyMuteRequestJob.Schedule(this, inputDeps),
            [2] = copyStopRequestJob.Schedule(this, inputDeps),
            [3] = copySpatialBlendJob.Schedule(this, inputDeps),
            [4] = copyAudioClipIDJob.Schedule(this, inputDeps),
            [5] = copyStartTimeJob.Schedule(this, inputDeps),
            [6] = copyLoopJob.Schedule(this, inputDeps)
        };

        JobHandle combinedDependencies = JobHandle.CombineDependencies(jobHandles);
        jobHandles.Dispose();

        return combinedDependencies;
    }
}
