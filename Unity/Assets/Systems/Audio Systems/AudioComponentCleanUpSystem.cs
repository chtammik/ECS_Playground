using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;

[UpdateAfter(typeof(AudioMuteSystem))]
[UpdateAfter(typeof(AudioStopSystem))]
[UpdateAfter(typeof(AudioStopVirtualSystem))]
[UpdateBefore(typeof(AssignAudioSourceSystem.AssignSourceIDBarrier))]
public class AudioComponentCleanUpSystem : JobComponentSystem
{
    NativeArray<JobHandle> _jobHandles;

    public class AudioPropertiesCleanUpBarrier : BarrierSystem { }

    #region Redundant InstanceMuted
    [RequireSubtractiveComponent(typeof(InstanceClaimed))]
    struct InstanceMutedCleanUpJob : IJobProcessComponentData<InstanceMuted>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref InstanceMuted instanceMuted)
        {
            CommandBuffer.RemoveComponent<InstanceMuted>(instanceMuted.InstanceEntity);
        }
    }

    #endregion

    #region Time On Play
    [RequireSubtractiveComponent(typeof(Playing), typeof(VirtualVoice))]
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
    [RequireSubtractiveComponent(typeof(Playing), typeof(VirtualVoice), typeof(RealVoiceRequest))]
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
    [RequireSubtractiveComponent(typeof(Playing), typeof(VirtualVoice), typeof(RealVoiceRequest))]
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
    [RequireSubtractiveComponent(typeof(Playing), typeof(VirtualVoice), typeof(RealVoiceRequest))]
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
        _jobHandles = new NativeArray<JobHandle>(5, Allocator.Persistent);
    }

    protected override void OnStopRunning()
    {
        _jobHandles.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var instanceMutedCleanUpJob = new InstanceMutedCleanUpJob { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var timeOnPlayCleanUpJob = new TimeOnPlayCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var clipIDCleanUpJob = new ClipIDCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var spatialBlendCleanUpJob = new SpatialBlendCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };
        var loopCleanUpJob = new LoopCleanUpJob() { CommandBuffer = audioPropertiesCleanUpBarrier.CreateCommandBuffer() };

        _jobHandles[0] = instanceMutedCleanUpJob.Schedule(this, inputDeps);
        _jobHandles[1] = timeOnPlayCleanUpJob.Schedule(this, inputDeps);
        _jobHandles[2] = clipIDCleanUpJob.Schedule(this, inputDeps);
        _jobHandles[3] = spatialBlendCleanUpJob.Schedule(this, inputDeps);
        _jobHandles[4] = loopCleanUpJob.Schedule(this, inputDeps);

        return JobHandle.CombineDependencies(_jobHandles);
    }
}
