using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

public class ToPlaySystem : ComponentSystem
{
	struct SourceGroup {
		public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<SourceHandle> sources;
	}
	struct ClipGroup {
		public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<Clip> clips;
		[ReadOnly] public ComponentDataArray<ToPlay> toPlay;
	}
	[Inject] SourceGroup sGroup;
	[Inject] ClipGroup cGroup;

    protected override void OnUpdate()
    {
		var entityManager = World.Active.GetExistingManager<EntityManager>();

        for(int i = cGroup.Length; i >= 0; i--){
			if(sGroup.Length == 0)
				break;
			
			Entity clip = cGroup.Entities[i];

			entityManager.RemoveComponent(cGroup.Entities[i], typeof(ToPlay));
			Debug.Log("played " + cGroup.clips[i].id + " at entity");
		}
    }
}
