using Unity.Entities;
using Unity.Mathematics;

public struct AudioSourceID : IComponentData
{
    public Entity OriginalEntity;
    public Entity HandleEntity;

    public AudioSourceID(Entity originalEntity, Entity handleEntity)
    {
        OriginalEntity = originalEntity;
        HandleEntity = handleEntity; //THIS LINE! TOOK ME FOREVER! GOODBYE DICTIONARY!
    }
}

public enum PlaybackStateType { NeedSource, ReadyToPlay, Play, Mute, Stop }
public struct PlaybackState : IComponentData
{
    public PlaybackStateType State;

    public PlaybackState(PlaybackStateType state)
    {
        State = state;
    }
}

public struct AudioPlayRequest : IComponentData
{
    public Entity Entity;

    public AudioPlayRequest(Entity entity)
    {
        Entity = entity;
    }
}
public struct AudioStopRequest : ISharedComponentData { }
public struct AudioMuteRequest : ISharedComponentData { }
public struct ReadyToPlay : ISharedComponentData { }
public struct AudioPlaying : ISharedComponentData { }
public struct AudioPlayingVirtually : ISharedComponentData { }

public struct AudioProperty_StartTime : IComponentData
{
    public double Time;

    public AudioProperty_StartTime(double time)
    {
        Time = time;
    }
}

public struct AudioProperty_AudioClipID : IComponentData
{
    public int ID;

    public AudioProperty_AudioClipID(int id)
    {
        ID = id;
    }
}

public struct AudioProperty_SpatialBlend : IComponentData
{
    public float Blend;

    public AudioProperty_SpatialBlend(float blend)
    {
        Blend = math.clamp(blend, 0, 1);
    }
}

public struct AudioProperty_Loop : IComponentData { }

public struct AudioSourceHandle : IComponentData
{
    public Entity OriginalEntity;
    public Entity HandleEntity;

    public AudioSourceHandle(Entity originalEntity, Entity handleEntity)
    {
        OriginalEntity = originalEntity;
        HandleEntity = handleEntity;
    }
}

public struct AudioSourceClaimed : ISharedComponentData { }

//public enum PlayType { NeedSource, ReadyToPlay, Play, Mute, Stop }
//public enum VoiceStatusType { Real, Virtual, Zero }
//public enum PriorityType { Low, Medium, High }
//public struct AudioSourceID : IComponentData
//{
//    public Entity EntityID;
//    public int ASID;
//    public VoiceStatusType VoiceStatus;
//    public PriorityType Priority;
//    public PlayType PlayStatus;

//    public AudioSourceID(Entity entity, int id)
//    {
//        EntityID = entity;
//        ASID = id;
//        Priority = priority;
//        PlayStatus = status;

//        if (status == PlayType.Mute || status == PlayType.NeedSource)
//            VoiceStatus = VoiceStatusType.Virtual;
//        else if (status == PlayType.Stop)
//            VoiceStatus = VoiceStatusType.Zero;
//        else
//            VoiceStatus = VoiceStatusType.Real;
//    }
//}

//public struct PlaySoundRequest : IComponentData
//{
//    public int AudioClipID;

//    public PlaySoundRequest(int value)
//    {
//        AudioClipID = value;
//    }
//}

//public struct StopSoundRequest : IComponentData { }
//public struct MuteSoundRequest : IComponentData { }

//public struct AudioProperty : IComponentData
//{
//    public double StartTime;
//    public int AudioClipID;

//    public AudioProperty(double startTime, int clipID)
//    {
//        StartTime = startTime;
//        AudioClipID = clipID;
//    }
//}

