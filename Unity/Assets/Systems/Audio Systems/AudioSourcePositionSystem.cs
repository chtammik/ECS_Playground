using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

//TODO: Jobfy this.
public class AudioSourcePositionSystem : ComponentSystem
{
    struct ASIDGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<Position> Positions;
        [ReadOnly] public ComponentDataArray<AudioSourceID> ASIDs;
    }
    [Inject] ASIDGroup asidGroup;

    [Inject] ComponentDataFromEntity<Position> SourcePosition;

    protected override void OnUpdate()
    {
        for (int i = 0; i < asidGroup.Length; i++)
        {
            SourcePosition[asidGroup.ASIDs[i].SourceEntity] = asidGroup.Positions[i];
        }
    }
}
