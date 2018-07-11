﻿using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

public class AudioInfoGUISystem : ComponentSystem
{
    public static Dictionary<int, PlayType> ASIDPlayStatus = new Dictionary<int, PlayType>();

    struct ASIDGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioSourceID> asID;
    }

    [Inject] ASIDGroup asidGroup;

    protected override void OnStartRunning()
    {
        UpdateInjectedComponentGroups();
        for (int i = 0; i < asidGroup.Length; i++)
        {
            if (!ASIDPlayStatus.TryGetValue(asidGroup.asID[i].EntityID.Index, out PlayType temp))
                ASIDPlayStatus.Add(asidGroup.asID[i].EntityID.Index, asidGroup.asID[i].PlayStatus);
        }
    }

    protected override void OnUpdate()
    {
        for (int i = 0; i < asidGroup.Length; i++)
        {
            ASIDPlayStatus[asidGroup.asID[i].EntityID.Index] = asidGroup.asID[i].PlayStatus;
        }
    }
}
