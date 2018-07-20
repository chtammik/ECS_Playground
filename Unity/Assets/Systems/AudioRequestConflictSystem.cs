using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

//TODO: Sending an AudioPlayRequest alone can have an assigned AudioSource play nothing. Do we want to prevent this?

[UpdateBefore(typeof(AudioPoolSystem.AssignSourceIDBarrier))]
public class AudioRequestConflictSystem : JobComponentSystem
{
    public class RequestConfilictBarrier : BarrierSystem { }

    //having an ASID means that it's no longer virtual
    [RequireComponentTag(typeof(AudioPlayingVirtually))]
    struct DevirtualizeJob : IJobProcessComponentData<AudioSourceID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref AudioSourceID asid)
        {
            CommandBuffer.RemoveComponent<AudioPlayingVirtually>(asid.OriginalEntity);
        }
    }

    //having an AudioPlayRequest that hasn't been dealt with means it's been playing virtually, so these two requests canceled out each other.
    [RequireComponentTag(typeof(AudioMuteRequest))]
    struct CancelPlayRequestFromMuteJob : IJobProcessComponentData<AudioPlayRequest>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute([ReadOnly]ref AudioPlayRequest playRequest)
        {
            CommandBuffer.RemoveComponent<AudioPlayRequest>(playRequest.Entity);
            CommandBuffer.RemoveComponent<AudioMuteRequest>(playRequest.Entity);
        }
    }

    [Inject] RequestConfilictBarrier requestConfilictBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var devirtualizeJob = new DevirtualizeJob { CommandBuffer = requestConfilictBarrier.CreateCommandBuffer() };
        JobHandle devirtualizeJH = devirtualizeJob.Schedule(this, inputDeps);

        var cancelPlayFromMuteJob = new CancelPlayRequestFromMuteJob { CommandBuffer = requestConfilictBarrier.CreateCommandBuffer() };
        JobHandle cancelPlayRequestJH = cancelPlayFromMuteJob.Schedule(this, inputDeps);

        return JobHandle.CombineDependencies(devirtualizeJH, cancelPlayRequestJH);
    }

}
