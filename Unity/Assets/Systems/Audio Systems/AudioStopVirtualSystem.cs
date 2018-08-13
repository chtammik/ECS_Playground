using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

[UpdateBefore(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
public class AudioStopVirtualSystem : JobComponentSystem
{
    public class StopVirtualBarrier : BarrierSystem { }

    [RequireComponentTag(typeof(AudioPlayingVirtually), typeof(AudioStopRequest))]
    [RequireSubtractiveComponent(typeof(AudioSourceID))]
    struct StopVirtualJob : IJobProcessComponentData<AudioPlayRequest>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref AudioPlayRequest audioPlayRequest)
        {
            CommandBuffer.RemoveComponent<AudioPlayRequest>(audioPlayRequest.Entity);
            CommandBuffer.RemoveComponent<AudioStopRequest>(audioPlayRequest.Entity);
            CommandBuffer.RemoveComponent<AudioPlayingVirtually>(audioPlayRequest.Entity);
        }
    }

    //The virtual groups also need to stop when done playing.
    [RequireSubtractiveComponent(typeof(AudioSourceID), typeof(AudioStopRequest), typeof(AudioProperty_Loop))]
    struct StopVirtualDonePlayingJob : IJobProcessComponentData<AudioPlayingVirtually, AudioProperty_StartTime, AudioProperty_AudioClipID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref AudioPlayingVirtually audioPlayingVirtually, [ReadOnly]ref AudioProperty_StartTime startTime, [ReadOnly]ref AudioProperty_AudioClipID audioClip)
        {
            if (AudioSettings.dspTime - startTime.Time > AudioService.GetClipLength(audioClip.ID)) 
                CommandBuffer.AddSharedComponent(audioPlayingVirtually.Entity, new AudioStopRequest());
        }
    }

    [Inject] StopVirtualBarrier stopVirtualBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var stopVirtualJob = new StopVirtualJob { CommandBuffer = stopVirtualBarrier.CreateCommandBuffer() };
        var stopVirtualDonePlayingJob = new StopVirtualDonePlayingJob { CommandBuffer = stopVirtualBarrier.CreateCommandBuffer() };
        return JobHandle.CombineDependencies(stopVirtualJob.Schedule(this, 64, inputDeps), stopVirtualDonePlayingJob.Schedule(this, 64, inputDeps));
    }
}
