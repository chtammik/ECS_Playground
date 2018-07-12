using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace SimpleExample.ECS
{	
	public class JobMoveInDirectionSystem : JobComponentSystem
	{
        // cuts the processing time in half in this instance 
        // (when burst safety checks are disabled also (editor only))
		[BurstCompile]
        // this job type uses dependency injection to "magically" get the components of those entities
        // which all match the defined signature (example "<Position, Direction>")
        struct MovePositionJob : IJobProcessComponentData<Position, MoveInDirection, EmptyComponent>
		{
            // the readonly flag helps the compiler optimize
            [ReadOnly] public float deltaTime;
            [ReadOnly] public float speed;
            // we only write to the position component
            public void Execute(ref Position position, [ReadOnly]ref MoveInDirection dir, [ReadOnly]ref EmptyComponent empty)
			{
				position.Value = position.Value + (dir.Value * deltaTime * speed);
			}
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
            // create a new job
			var job = new MovePositionJob()
            {
                deltaTime = Time.deltaTime,
                speed = BootstrapRenderingCubes.Instance.speed
            };
            // scheduling the job pushes it to the job system to deal with
            // the jobHandle we return can be used to chain data dependensies
            // or force the job to complete
			return job.Schedule(this, 128, inputDeps); 
		} 
	}
}
