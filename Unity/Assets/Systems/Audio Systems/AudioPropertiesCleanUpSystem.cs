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
    NativeArray<JobHandle> _jobHandles;

    public class AudioPropertiesCleanUpBarrier : BarrierSystem { }

    #region Time On Play
    [RequireSubtractiveComponent(typeof(AudioPlaying), typeof(VirtualVoice))]
    struct TimeOnPlayCleanUpJob : IJobProcessComponentData<DSPTimeOnPlay>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref DSPTimeOnPlay timeOnPlay)
        {
            CommandBuffer.RemoveComponent<DSPTimeOnPlay>(timeOnPlay.Entity);
        }
    }
    #endregion

    #region Audio Clip ID
    [RequireSubtractiveComponent(typeof(AudioPlaying), typeof(VirtualVoice), typeof(AudioPlayRequest))]
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
    [RequireSubtractiveComponent(typeof(AudioPlaying), typeof(VirtualVoice), typeof(AudioPlayRequest))]
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
    [RequireSubtractiveComponent(typeof(AudioPlaying), typeof(VirtualVoice), typeof(AudioPlayRequest))]
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

    protected override void OnStartRunning()
    {
        _jobHandles = new NativeArray<JobHandle>(4, Allocator.Persistent);
    }

    protected override void OnStopRunning()
    {
        _jobHandles.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var timeOnPlayCleanUpJob = new TimeOnPlayCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var clipIDCleanUpJob = new ClipIDCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var spatialBlendCleanUpJob = new SpatialBlendCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var loopCleanUpJob = new LoopCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };

        _jobHandles[0] = timeOnPlayCleanUpJob.Schedule(this, inputDeps);
        _jobHandles[1] = clipIDCleanUpJob.Schedule(this, inputDeps);
        _jobHandles[2] = spatialBlendCleanUpJob.Schedule(this, inputDeps);
        _jobHandles[3] = loopCleanUpJob.Schedule(this, inputDeps);

        return JobHandle.CombineDependencies(_jobHandles);
    }
}
