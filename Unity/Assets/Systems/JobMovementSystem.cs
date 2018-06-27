using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace SimpleExample.ECS
{	
	public class JobMovementSystem : JobComponentSystem
	{
		[BurstCompile]
		struct MovementJob : IJobProcessComponentData<Position, Direction>
		{
            [ReadOnly] public float deltaTime;
            [ReadOnly] public float speed;
            public void Execute(ref Position position, [ReadOnly]ref Direction dir)
			{
				position.Value = position.Value + (dir.Value * deltaTime * speed);
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var job = new MovementJob()
            {
                deltaTime = Time.deltaTime,
                speed = Bootstrap.Instance.speed
            };
			return job.Schedule(this, 64, inputDeps); 
		} 
	}
}
