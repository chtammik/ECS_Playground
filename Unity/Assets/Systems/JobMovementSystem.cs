using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;

namespace SimpleExample.ECS
<<<<<<< HEAD
{
    public class JobMovementSystem : JobComponentSystem
    {
        // TODO: research what this attribute does?
        [BurstCompile]
        // this job type uses dependency injection to "magically" get the components of those entities
        // which all match the defined signature (example "<Position, Direction>")
        struct MovementJob : IJobProcessComponentData<Position, Direction, EmptyComponent>
        {
=======
{	
	public class JobMoveInDirectionSystem : JobComponentSystem
	{
        // cuts the processing time in half in this instance 
        // (when burst safety checks are disabled also (editor only))
		[BurstCompile]
        // this job type uses dependency injection to "magically" get the components of those entities
        // which all match the defined signature (example "<Position, Direction>")
        struct MovePositionJob : IJobProcessComponentData<Position, MoveInDirection>
		{
>>>>>>> 02f54775fff28a7d2fd76e6b4de2091799cb9621
            // the readonly flag helps the compiler optimize
            [ReadOnly] public float deltaTime;
            [ReadOnly] public float speed;
            // we only write to the position component
<<<<<<< HEAD
            public void Execute(ref Position position, [ReadOnly]ref Direction dir, [ReadOnly]ref EmptyComponent emp)
            {
                position.Value = position.Value + (dir.Value * deltaTime * speed);
            }
        }
=======
            public void Execute(ref Position position, [ReadOnly]ref MoveInDirection dir)
			{
				position.Value = position.Value + (dir.Value * deltaTime * speed);
			}
		}
>>>>>>> 02f54775fff28a7d2fd76e6b4de2091799cb9621

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            // create a new job
<<<<<<< HEAD
            var job = new MovementJob()
=======
			var job = new MovePositionJob()
>>>>>>> 02f54775fff28a7d2fd76e6b4de2091799cb9621
            {
                deltaTime = Time.deltaTime,
                speed = Bootstrap.Instance.speed
            };
            // scheduling the job pushes it to the job system to deal with
            // the jobHandle we return can be used to chain data dependensies
            // or force the job to complete
<<<<<<< HEAD
            return job.Schedule(this, 64, inputDeps);
        }
    }
=======
			return job.Schedule(this, 128, inputDeps); 
		} 
	}
>>>>>>> 02f54775fff28a7d2fd76e6b4de2091799cb9621
}
