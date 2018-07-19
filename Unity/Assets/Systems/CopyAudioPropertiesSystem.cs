using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateBefore(typeof(ApplyAudioPropertiesSystem))]
[UpdateAfter(typeof(AudioPoolSystem.AssignSourceIDBarrier))]
public class CopyAudioPropertiesSystem : JobComponentSystem
{
    public class CopyAudioPropertiesBarrier : BarrierSystem { }

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
    struct CopyAudioClipIDJob : IJobProcessComponentData<AudioSourceID, AudioClipID>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public void Execute([ReadOnly]ref AudioSourceID asid, [ReadOnly]ref AudioClipID acid)
        {
            CommandBuffer.AddComponent(asid.HandleEntity, acid);
            CommandBuffer.RemoveComponent<AudioClipID>(asid.OriginalEntity);
        }
    }
    #endregion

    [Inject] CopyAudioPropertiesBarrier copyAudioPropertiesBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var copySpatialBlendJob = new CopySpatialBlendJob()
        {
            CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer()
        };

        var copyAudioClipIDJob = new CopyAudioClipIDJob()
        {
            CommandBuffer = copyAudioPropertiesBarrier.CreateCommandBuffer()
        };

        JobHandle copySpatialBlendJH = copySpatialBlendJob.Schedule(this, inputDeps);
        JobHandle copyAudioClipIDJH = copyAudioClipIDJob.Schedule(this, inputDeps);

        return JobHandle.CombineDependencies(copySpatialBlendJH, copyAudioClipIDJH, inputDeps);
    }
}
