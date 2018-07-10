using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

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

            if (moo.MooStatus == MooType.StartMooing)
            {
                CommandBuffer.AddComponent<AudioSourceID>(moo.EntityID, new AudioSourceID(moo.EntityID, -1, PriorityType.Medium, PlayType.NeedSource));
                CommandBuffer.AddComponent<AudioClipID>(moo.EntityID, new AudioClipID(moo.EntityID.Index - 2));
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
