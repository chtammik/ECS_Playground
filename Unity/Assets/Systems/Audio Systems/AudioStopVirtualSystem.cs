using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

[UpdateBefore(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
public class AudioStopVirtualSystem : JobComponentSystem
{
    NativeArray<JobHandle> _jobHandles;

    public class StopVirtualBarrier : BarrierSystem { }

    //stop virtual voices that are still trying to get a RealVoice.
    [RequireComponentTag(typeof(VoiceHandle), typeof(StopRequest), typeof(RealVoiceRequest))]
    [RequireSubtractiveComponent(typeof(RealVoice))]
    struct StopVirtualJob : IJobProcessComponentData<VirtualVoice>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref VirtualVoice virtualVoice)
        {
            Entity voiceEntity = virtualVoice.VoiceEntity;
            CommandBuffer.RemoveComponent<StopRequest>(voiceEntity);
            CommandBuffer.RemoveComponent<VirtualVoice>(voiceEntity);
            CommandBuffer.AddComponent(voiceEntity, new AudioMessage_Stopped(voiceEntity));
            CommandBuffer.RemoveComponent<RealVoiceRequest>(voiceEntity);
        }
    }

    //stop virtual voices that are not trying to get RealVoice.
    [RequireComponentTag(typeof(VoiceHandle), typeof(StopRequest))]
    [RequireSubtractiveComponent(typeof(RealVoice), typeof(RealVoiceRequest))]
    struct StopMutedJob : IJobProcessComponentData<VirtualVoice>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref VirtualVoice virtualVoice)
        {
            Entity voiceEntity = virtualVoice.VoiceEntity;
            CommandBuffer.RemoveComponent<StopRequest>(voiceEntity);
            CommandBuffer.RemoveComponent<VirtualVoice>(voiceEntity);
            CommandBuffer.AddComponent(voiceEntity, new AudioMessage_Stopped(voiceEntity));
        }
    }

    //add a StopRequest to virtual voices that have done playing.
    [RequireComponentTag(typeof(VoiceHandle))]
    [RequireSubtractiveComponent(typeof(RealVoice), typeof(StopRequest), typeof(AudioProperty_Loop))]
    struct StopVirtualDonePlayingJob : IJobProcessComponentData<VirtualVoice, DSPTimeOnPlay, AudioProperty_AudioClipID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref VirtualVoice audioPlayingVirtually, [ReadOnly]ref DSPTimeOnPlay timeOnPlay, [ReadOnly]ref AudioProperty_AudioClipID audioClip)
        {
            if (AudioSettings.dspTime - timeOnPlay.Time > AudioService.GetClipLength(audioClip.ID)) 
                CommandBuffer.AddSharedComponent(audioPlayingVirtually.VoiceEntity, new StopRequest());
        }
    }

    protected override void OnStartRunning()
    {
        _jobHandles = new NativeArray<JobHandle>(3, Allocator.Persistent);
    }

    protected override void OnStopRunning()
    {
        _jobHandles.Dispose();
    }

    [Inject] StopVirtualBarrier _stopVirtualBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var stopVirtualJob = new StopVirtualJob { CommandBuffer = _stopVirtualBarrier.CreateCommandBuffer() };
        var stopMutedJob = new StopMutedJob { CommandBuffer = _stopVirtualBarrier.CreateCommandBuffer() };
        var stopVirtualDonePlayingJob = new StopVirtualDonePlayingJob { CommandBuffer = _stopVirtualBarrier.CreateCommandBuffer() };

        _jobHandles[0] = stopVirtualJob.Schedule(this, 64, inputDeps);
        _jobHandles[1] = stopMutedJob.Schedule(this, 64, inputDeps);
        _jobHandles[2] = stopVirtualDonePlayingJob.Schedule(this, 64, inputDeps);

        return JobHandle.CombineDependencies(_jobHandles);
    }
}
