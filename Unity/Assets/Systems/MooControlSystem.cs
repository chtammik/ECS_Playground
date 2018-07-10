using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

public class MooControlSystem : ComponentSystem
{
    struct MooGroup
    {
        public readonly int Length;
        [ReadOnly] public EntityArray Entities;
        public ComponentDataArray<Moo> Moos;
    }

    [Inject] MooGroup mooGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < mooGroup.Length; i++)
        {
            if (mooGroup.Moos[i].MooStatus == MooType.Quiet && Input.GetKeyUp(CowGenerator.CowKeyCode(mooGroup.Entities[i].Index)))
                mooGroup.Moos[i] = new Moo(MooType.StartMooing, mooGroup.Entities[i]);
            if (mooGroup.Moos[i].MooStatus == MooType.Mooing && Input.GetKeyUp(CowGenerator.CowKeyCode(mooGroup.Entities[i].Index)))
                mooGroup.Moos[i] = new Moo(MooType.StopMooing, mooGroup.Entities[i]);
        }
    }
}
