using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

//public class AudioInfoGUISystem : ComponentSystem
//{
//    public static Dictionary<int, PlaybackStateType> ASIDPlayStatus = new Dictionary<int, PlaybackStateType>();

//    struct VoiceGroup
//    {
//        public readonly int Length;
//        [ReadOnly] public ComponentDataArray<AudioSourceID> voice;
//    }

//    [Inject] VoiceGroup voiceGroup;

//    protected override void OnStartRunning()
//    {
//        UpdateInjectedComponentGroups();
//        for (int i = 0; i < voiceGroup.Length; i++)
//        {
//            if (!ASIDPlayStatus.TryGetValue(voiceGroup.voice[i].HandleEntity.Index, out PlaybackStateType temp))
//                ASIDPlayStatus.Add(voiceGroup.voice[i].HandleEntity.Index, voiceGroup.voice[i].PlayStatus);
//        }
//    }

//    protected override void OnUpdate()
//    {
//        for (int i = 0; i < voiceGroup.Length; i++)
//        {
//            ASIDPlayStatus[voiceGroup.voice[i].HandleEntity.Index] = voiceGroup.voice[i].PlayStatus;
//        }
//    }
//}
