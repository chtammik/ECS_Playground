using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// This component defines the direction and magnitude in which a position should be moving
/// </summary>
public struct MoveInDirection : IComponentData
{
    public float3 Value;

    public MoveInDirection(UnityEngine.Vector3 dir)
    {
        Value = dir;
    }
}