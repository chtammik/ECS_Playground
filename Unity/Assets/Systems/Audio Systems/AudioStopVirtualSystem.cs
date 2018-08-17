using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

[UpdateBefore(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
public class AudioStopVirtualSystem : JobComponentSystem
{
    public class StopVirtualBarrier : BarrierSystem { }

    //Stop virtual voices that already have StopSoundRequest on it, then remove the StopSoundRequest.
    [RequireComponentTag(typeof(VirtualVoice), typeof(AudioStopRequest), typeof(VoiceHandle))]
    [RequireSubtractiveComponent(typeof(RealVoice))]
    struct StopVirtualJob : IJobProcessComponentData<AudioPlayRequest>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref AudioPlayRequest audioPlayRequest)
        {
            Entity voiceEntity = audioPlayRequest.VoiceEntity;
            CommandBuffer.RemoveComponent<AudioStopRequest>(voiceEntity);
            CommandBuffer.RemoveComponent<VirtualVoice>(voiceEntity);
            CommandBuffer.AddComponent(voiceEntity, new AudioMessage_Stopped(voiceEntity));
            CommandBuffer.RemoveComponent<AudioPlayRequest>(voiceEntity);
        }
    }

    //The virtual groups need to stop when done playing.
    [RequireComponentTag(typeof(VoiceHandle))]
    [RequireSubtractiveComponent(typeof(RealVoice), typeof(AudioStopRequest), typeof(AudioProperty_Loop))]
    struct StopVirtualDonePlayingJob : IJobProcessComponentData<VirtualVoice, DSPTimeOnPlay, AudioProperty_AudioClipID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref VirtualVoice audioPlayingVirtually, [ReadOnly]ref DSPTimeOnPlay timeOnPlay, [ReadOnly]ref AudioProperty_AudioClipID audioClip)
        {
            if (AudioSettings.dspTime - timeOnPlay.Time > AudioService.GetClipLength(audioClip.ID)) 
                CommandBuffer.AddSharedComponent(audioPlayingVirtually.VoiceEntity, new AudioStopRequest());
        }
    }

    [Inject] StopVirtualBarrier _stopVirtualBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var stopVirtualJob = new StopVirtualJob { CommandBuffer = _stopVirtualBarrier.CreateCommandBuffer() };
        var stopVirtualDonePlayingJob = new StopVirtualDonePlayingJob { CommandBuffer = _stopVirtualBarrier.CreateCommandBuffer() };
        return JobHandle.CombineDependencies(stopVirtualJob.Schedule(this, 64, inputDeps), stopVirtualDonePlayingJob.Schedule(this, 64, inputDeps));
    }
}
