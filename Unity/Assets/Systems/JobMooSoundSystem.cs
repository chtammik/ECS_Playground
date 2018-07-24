using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class MooBarrier : BarrierSystem { }

public class JobMooSoundSystem : JobComponentSystem
{
    //[BurstCompile] at present EntityCommandBuffer doesn't work with burst.
    struct MooSoundJob : IJobProcessComponentData<Moo>
    {
        public EntityCommandBuffer CommandBuffer;

        public void Execute(ref Moo moo)
        {
            if (moo.MooStatus == MooType.StopMooing)
            {
                CommandBuffer.AddSharedComponent(moo.Entity, new AudioStopRequest());
                moo.MooStatus = MooType.Quiet;
            }

            if (moo.MooStatus == MooType.MuteMooing)
            {
                CommandBuffer.AddSharedComponent(moo.Entity, new AudioMuteRequest());
                moo.MooStatus = MooType.Muted;
            }

            if (moo.MooStatus == MooType.UnmuteMooing)
            {
                CommandBuffer.AddComponent(moo.Entity, new AudioPlayRequest(moo.Entity));
                moo.MooStatus = MooType.Mooing;
            }

            if (moo.MooStatus == MooType.StartMooing)
            {
                CommandBuffer.AddComponent(moo.Entity, new AudioProperty_AudioClipID(moo.Entity.Index - 4));
                CommandBuffer.AddComponent(moo.Entity, new AudioProperty_SpatialBlend(0));
                //CommandBuffer.AddComponent(moo.Entity, new AudioProperty_Loop());
                CommandBuffer.AddComponent(moo.Entity, new AudioPlayRequest(moo.Entity));
                moo.MooStatus = MooType.Mooing;
            }
        }
    }

    [Inject] MooBarrier mooBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MooSoundJob()
        {
            CommandBuffer = mooBarrier.CreateCommandBuffer()
        };

        return job.Schedule(this, inputDeps);
    }
}
