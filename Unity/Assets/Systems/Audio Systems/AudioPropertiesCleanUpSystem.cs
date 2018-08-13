using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

[UpdateAfter(typeof(AudioMuteSystem))]
[UpdateAfter(typeof(AudioStopSystem))]
[UpdateAfter(typeof(AudioStopVirtualSystem.StopVirtualBarrier))]
[UpdateBefore(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
public class AudioPropertiesCleanUpSystem : JobComponentSystem
{
    public class AudioPropertiesCleanUpBarrier : BarrierSystem { }

    #region Start Time
    [RequireSubtractiveComponent(typeof(AudioPlaying), typeof(AudioPlayingVirtually))]
    struct StartTimeCleanUpJob : IJobProcessComponentData<AudioProperty_StartTime>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref AudioProperty_StartTime startTime)
        {
            CommandBuffer.RemoveComponent<AudioProperty_StartTime>(startTime.Entity);
        }
    }
    #endregion

    #region Audio Clip ID
    [RequireSubtractiveComponent(typeof(AudioPlaying), typeof(AudioPlayingVirtually), typeof(AudioPlayRequest))]
    struct ClipIDCleanUpJob : IJobProcessComponentData<AudioProperty_AudioClipID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(ref AudioProperty_AudioClipID clipID)
        {
            CommandBuffer.RemoveComponent<AudioProperty_AudioClipID>(clipID.Entity);
        }
    }
    #endregion

    #region Spatial Blend
    [RequireSubtractiveComponent(typeof(AudioPlaying), typeof(AudioPlayingVirtually), typeof(AudioPlayRequest))]
    struct SpatialBlendCleanUpJob : IJobProcessComponentData<AudioProperty_SpatialBlend>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(ref AudioProperty_SpatialBlend spatialBlend)
        {
            CommandBuffer.RemoveComponent<AudioProperty_SpatialBlend>(spatialBlend.Entity);
        }
    }
    #endregion

    #region Loop
    [RequireSubtractiveComponent(typeof(AudioPlaying), typeof(AudioPlayingVirtually), typeof(AudioPlayRequest))]
    struct LoopCleanUpJob : IJobProcessComponentData<AudioProperty_Loop>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioProperty_Loop loop)
        {
            CommandBuffer.RemoveComponent<AudioProperty_Loop>(loop.Entity);
        }
    }
    #endregion

    [Inject] AudioPropertiesCleanUpBarrier audioPropertiesCleanUpBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var startTimeCleanUpJob = new StartTimeCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var clipIDCleanUpJob = new ClipIDCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var spatialBlendCleanUpJob = new SpatialBlendCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var loopCleanUpJob = new LoopCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };

        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(4, Allocator.Temp)
        {
            [0] = startTimeCleanUpJob.Schedule(this, inputDeps),
            [1] = clipIDCleanUpJob.Schedule(this, inputDeps),
            [2] = spatialBlendCleanUpJob.Schedule(this, inputDeps),
            [3] = loopCleanUpJob.Schedule(this, inputDeps)
        };

        JobHandle combinedDependencies = JobHandle.CombineDependencies(jobHandles);
        jobHandles.Dispose();

        return combinedDependencies;
    }
}
