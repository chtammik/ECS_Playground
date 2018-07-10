using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Rendering;

public class ColoringSystem : ComponentSystem
{
    struct ColoringGroup
    {
        public readonly int Length;
        public EntityArray entityArray;
        public ComponentDataArray<Coloring> Colorings;
        [ReadOnly] public SharedComponentDataArray<MeshInstanceRenderer> renderer;
    }

    [Inject] ColoringGroup group;

    protected override void OnUpdate()
    {
        for (int i = 0; i < group.Length; i++)
        {
            //group.renderer[i].material.color = group.Colorings[i].RequestColor;
            PostUpdateCommands.RemoveComponent<Coloring>(group.entityArray[i]);
        }
        
    }
}
