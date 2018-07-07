using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;

public class JobCowMoveSystem : JobComponentSystem
{
    [BurstCompile]
    struct CowMoveJob : IJobProcessComponentData<Position, Moo>
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float speed;

        public void Execute(ref Position position, [ReadOnly]ref Moo moo)
        {
            position.Value = position.Value + (new float3(0, 1, 0) * deltaTime * speed);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CowMoveJob()
        {
            deltaTime = Time.deltaTime,
            speed = 0.1f
        };

        return job.Schedule(this, 64, inputDeps);
    }
}
