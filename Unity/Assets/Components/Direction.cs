using Unity.Entities;
using Unity.Mathematics;

public struct Direction : IComponentData
{
    public float3 Value;

    public Direction(UnityEngine.Vector3 dir)
    {
        Value = dir;
    }
}