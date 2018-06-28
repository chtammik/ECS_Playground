using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// This component defines the direction and magnitude in which a position should be moving
/// </summary>
public struct Direction : IComponentData
{
    public float3 Value;

    public Direction(UnityEngine.Vector3 dir)
    {
        Value = dir;
    }
}