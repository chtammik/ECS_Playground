using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ApplyAudioPropertiesSystem))]
[UpdateAfter(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
public class CopyAudioPropertiesSystem : JobComponentSystem
{
    NativeArray<JobHandle> _jobHandles;

    public class CopyAudioPropertiesBarrier : BarrierSystem { }

    #region CopyMuteRequestJob
    [RequireComponentTag(typeof(AudioMuteRequest))]
    struct CopyMuteRequestJob : IJobProcessComponentData<RealVoice>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref RealVoice realVoice)
        {
            CommandBuffer.AddSharedComponent(realVoice.SourceEntity, new AudioMuteRequest());
            CommandBuffer.RemoveComponent<AudioMuteRequest>(realVoice.VoiceEntity);
            CommandBuffer.RemoveComponent<RealVoice>(realVoice.VoiceEntity);
        }
    }
    #endregion

    #region CopyStopRequestJob
    [RequireComponentTag(typeof(AudioStopRequest))]
    struct CopyStopRequestJob : IJobProcessComponentData<RealVoice>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref RealVoice realVoice)
        {
            CommandBuffer.AddSharedComponent(realVoice.SourceEntity, new AudioStopRequest());
            CommandBuffer.RemoveComponent<AudioStopRequest>(realVoice.VoiceEntity);
            CommandBuffer.RemoveComponent<RealVoice>(realVoice.VoiceEntity);
        }
    }
    #endregion

    #region CopyTimeOnPlayJob
    struct CopyTimeOnPlayJob : IJobProcessComponentData<RealVoice, DSPTimeOnPlay>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref RealVoice realVoice, [ReadOnly]ref DSPTimeOnPlay timeOnPlay)
        {
            CommandBuffer.AddComponent(realVoice.SourceEntity, new DSPTimeOnPlay(realVoice.SourceEntity, timeOnPlay.Time));
            CommandBuffer.RemoveComponent<DSPTimeOnPlay>(realVoice.VoiceEntity);
        }
    }
    #endregion

    #region CopySpatialBlendJob
    struct CopySpatialBlendJob : IJobProcessComponentData<RealVoice, AudioProperty_SpatialBlend>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref RealVoice realVoice, [ReadOnly]ref AudioProperty_SpatialBlend spatialBlend)
        {
            CommandBuffer.AddComponent(realVoice.SourceEntity, new AudioProperty_SpatialBlend(realVoice.SourceEntity, spatialBlend.Blend));
            CommandBuffer.RemoveComponent<AudioProperty_SpatialBlend>(realVoice.VoiceEntity);
        }
    }
    #endregion

    #region CopyAudioClipIDJob
    struct CopyAudioClipIDJob : IJobProcessComponentData<RealVoice, AudioProperty_AudioClipID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref RealVoice realVoice, [ReadOnly]ref AudioProperty_AudioClipID acid)
        {
            CommandBuffer.AddComponent(realVoice.SourceEntity, new AudioProperty_AudioClipID(realVoice.SourceEntity, acid.ID));
            CommandBuffer.RemoveComponent<AudioProperty_AudioClipID>(realVoice.VoiceEntity);
        }
    }
    #endregion

    #region CopyLoopJob
    struct CopyLoopJob : IJobProcessComponentData<RealVoice, AudioProperty_Loop>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref RealVoice realVoice, [ReadOnly]ref AudioProperty_Loop loop)
        {
            CommandBuffer.AddComponent(realVoice.SourceEntity, new AudioProperty_Loop(realVoice.SourceEntity));
            CommandBuffer.RemoveComponent<AudioProperty_Loop>(realVoice.VoiceEntity);
        }
    }
    #endregion

    protected override void OnStartRunning()
    {
        _jobHandles = new NativeArray<JobHandle>(6, Allocator.Persistent);
    }

    protected override void OnStopRunning()
    {
        _jobHandles.Dispose();
    }

    [Inject] CopyAudioPropertiesBarrier copyAudioPropertiesBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var copyMuteRequestJob = new CopyMuteRequestJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copyStopRequestJob = new CopyStopRequestJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copyTimeOnPlayJob = new CopyTimeOnPlayJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copySpatialBlendJob = new CopySpatialBlendJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copyAudioClipIDJob = new CopyAudioClipIDJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };
        var copyLoopJob = new CopyLoopJob() { CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer() };

        _jobHandles[0] = copyMuteRequestJob.Schedule(this, inputDeps);
        _jobHandles[1] = copyStopRequestJob.Schedule(this, inputDeps);
        _jobHandles[2] = copyTimeOnPlayJob.Schedule(this, inputDeps);
        _jobHandles[3] = copySpatialBlendJob.Schedule(this, inputDeps);
        _jobHandles[4] = copyAudioClipIDJob.Schedule(this, inputDeps);
        _jobHandles[5] = copyLoopJob.Schedule(this, inputDeps);

        return JobHandle.CombineDependencies(_jobHandles);
    }
}
