using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

//public class AudioInfoGUISystem : ComponentSystem
//{
//    public static Dictionary<int, PlaybackStateType> ASIDPlayStatus = new Dictionary<int, PlaybackStateType>();

//    struct ASIDGroup
//    {
//        public readonly int Length;
//        [ReadOnly] public ComponentDataArray<AudioSourceID> asID;
//    }

//    [Inject] ASIDGroup asidGroup;

//    protected override void OnStartRunning()
//    {
//        UpdateInjectedComponentGroups();
//        for (int i = 0; i < asidGroup.Length; i++)
//        {
//            if (!ASIDPlayStatus.TryGetValue(asidGroup.asID[i].HandleEntity.Index, out PlaybackStateType temp))
//                ASIDPlayStatus.Add(asidGroup.asID[i].HandleEntity.Index, asidGroup.asID[i].PlayStatus);
//        }
//    }

//    protected override void OnUpdate()
//    {
//        for (int i = 0; i < asidGroup.Length; i++)
//        {
//            ASIDPlayStatus[asidGroup.asID[i].HandleEntity.Index] = asidGroup.asID[i].PlayStatus;
//        }
//    }
//}
