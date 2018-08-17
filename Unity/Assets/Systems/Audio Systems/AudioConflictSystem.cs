using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateBefore(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
[UpdateBefore(typeof(AudioStopSystem))]
[UpdateBefore(typeof(AudioStopVirtualSystem.StopVirtualBarrier))]
[UpdateBefore(typeof(AudioMuteSystem))]
public class AudioConflictSystem : JobComponentSystem
{
    NativeArray<JobHandle> _jobHandles;

    public class ConfilictBarrier : BarrierSystem { }

    #region VirtualVoice & RealVoice
    //having a RealVoice means that it's no longer virtual
    [RequireComponentTag(typeof(VirtualVoice))]
    struct DevirtualizeJob : IJobProcessComponentData<RealVoice>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref RealVoice voice)
        {
            CommandBuffer.RemoveComponent<VirtualVoice>(voice.VoiceEntity);
            Debug.LogWarning("Removed the VirtualVoice on the voice entity: " + voice.VoiceEntity.Index + ", because it already has a RealVoice.");
        }
    }
    #endregion

    #region Cancel PlayRequest from MuteRequest
    //having an AudioPlayRequest that hasn't been dealt with means it's been playing virtually and trying to get a RealVoice, 
    //so AudioMuteRequest makes it stay virtual but stop trying to get a RealVoice.
    [RequireComponentTag(typeof(AudioMuteRequest))]
    struct CancelPlayRequestFromMuteJob : IJobProcessComponentData<AudioPlayRequest>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref AudioPlayRequest playRequest)
        {
            CommandBuffer.RemoveComponent<AudioPlayRequest>(playRequest.VoiceEntity);
            CommandBuffer.RemoveComponent<AudioMuteRequest>(playRequest.VoiceEntity);
        }
    }
    #endregion

    #region Invalid MuteRequest
    //if you're trying to mute a virtual voice that's not trying to get a RealVoice, you're muting something that's already muted.
    [RequireComponentTag(typeof(AudioMuteRequest))]
    [RequireSubtractiveComponent(typeof(AudioPlayRequest))]
    struct CancelInvalidMuteRequestJob : IJobProcessComponentData<VirtualVoice>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref VirtualVoice virtualVoice)
        {
            CommandBuffer.RemoveComponent<AudioMuteRequest>(virtualVoice.VoiceEntity);
            Debug.LogWarning("You are trying to mute the voice entity: " + virtualVoice.VoiceEntity.Index + ", which is already muted.");
        }
    }
    #endregion

    #region Invalid StopRequest
    //if you're trying to stop a voice that has no either RealVoice or VirtualVoice, you're stopping nothing.
    [RequireComponentTag(typeof(AudioStopRequest))]
    [RequireSubtractiveComponent(typeof(RealVoice), typeof(VirtualVoice))]
    struct CancelInvalidStopRequestJob : IJobProcessComponentData<VoiceHandle>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref VoiceHandle voiceHandle)
        {
            CommandBuffer.RemoveComponent<AudioStopRequest>(voiceHandle.VoiceEntity);
            Debug.LogWarning("You are trying to stop the voice entity: " + voiceHandle.VoiceEntity.Index + ", which is not playing.");
        }
    }
    #endregion

    #region Invalid PlayRequest
    //sending an AudioPlayRequest alone can have an assigned AudioSource play nothing.
    [RequireSubtractiveComponent(typeof(AudioProperty_AudioClipID))]
    struct CancelInvalidPlayRequestJob : IJobProcessComponentData<AudioPlayRequest>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref AudioPlayRequest playRequest)
        {
            CommandBuffer.RemoveComponent<AudioPlayRequest>(playRequest.VoiceEntity);
            Debug.LogWarning("You are trying to play the voice entity: " + playRequest.VoiceEntity.Index + ", but no audio clip is provided.");
        }
    }
    #endregion

    protected override void OnStartRunning()
    {
        _jobHandles = new NativeArray<JobHandle>(5, Allocator.Persistent);
    }

    protected override void OnStopRunning()
    {
        _jobHandles.Dispose();
    }

    [Inject] ConfilictBarrier _conflictBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var devirtualizeJob = new DevirtualizeJob { CommandBuffer = _conflictBarrier.CreateCommandBuffer() };
        var cancelPlayFromMuteJob = new CancelPlayRequestFromMuteJob { CommandBuffer = _conflictBarrier.CreateCommandBuffer() };
        var cancelInvalidMuteRequestJob = new CancelInvalidMuteRequestJob { CommandBuffer = _conflictBarrier.CreateCommandBuffer() };
        var cancelInvalidStopRequestJob = new CancelInvalidStopRequestJob { CommandBuffer = _conflictBarrier.CreateCommandBuffer() };
        var cancelInvalidPlayRequestJob = new CancelInvalidPlayRequestJob { CommandBuffer = _conflictBarrier.CreateCommandBuffer() };

        _jobHandles[0] = devirtualizeJob.Schedule(this, inputDeps);
        _jobHandles[1] = cancelPlayFromMuteJob.Schedule(this, inputDeps);
        _jobHandles[2] = cancelInvalidMuteRequestJob.Schedule(this, inputDeps);
        _jobHandles[3] = cancelInvalidStopRequestJob.Schedule(this, inputDeps);
        _jobHandles[4] = cancelInvalidPlayRequestJob.Schedule(this, inputDeps);

        return JobHandle.CombineDependencies(_jobHandles);
    }

}
