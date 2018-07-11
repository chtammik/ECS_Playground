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
                CommandBuffer.AddComponent<StopSoundRequest>(moo.EntityID, new StopSoundRequest());
                moo.MooStatus = MooType.Quiet;
            }

            if (moo.MooStatus == MooType.MuteMooing)
            {
                CommandBuffer.AddComponent<MuteSoundRequest>(moo.EntityID, new MuteSoundRequest());
                moo.MooStatus = MooType.Muted;
            }

            if (moo.MooStatus == MooType.UnmuteMooing)
            {
                AudioPoolSystem.SetAudioSourceID(CommandBuffer, moo.EntityID);
                moo.MooStatus = MooType.Mooing;
            }

            if (moo.MooStatus == MooType.StartMooing)
            {
                AudioPoolSystem.AddAudioSourceID(CommandBuffer, moo.EntityID, moo.EntityID.Index - 2);
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
