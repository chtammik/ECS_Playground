using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct VoiceHandle : IComponentData
{
    public Entity GameEntity;
    public Entity VoiceEntity;
    
    public VoiceHandle(Entity gameEntity, Entity voiceEntity)
    {
        GameEntity = gameEntity;
        VoiceEntity = voiceEntity;
    }
}

public struct RealVoice : IComponentData
{
    public Entity VoiceEntity;
    public Entity SourceEntity;

    public RealVoice(Entity voiceEntity, Entity sourceEntity)
    {
        VoiceEntity = voiceEntity;
        SourceEntity = sourceEntity; //THIS LINE! TOOK ME FOREVER! GOODBYE DICTIONARY!
    }
}

public struct VirtualVoice : IComponentData
{
    public Entity VoiceEntity;

    public VirtualVoice(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}

public struct AudioSourceHandle : IComponentData
{
    public Entity SourceEntity;

    public AudioSourceHandle(Entity sourceEntity)
    {
        SourceEntity = sourceEntity;
    }
}

public struct ClaimedByVoice : IComponentData
{
    public Entity VoiceEntity;

    public ClaimedByVoice(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}

public struct RealVoiceRequest : IComponentData
{
    public Entity VoiceEntity;

    public RealVoiceRequest(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}
public struct StopRequest : ISharedComponentData { }
public struct MuteRequest : ISharedComponentData { }
public struct ReadToPlay : ISharedComponentData { }
public struct Playing : ISharedComponentData { } //different from RealVoice, which is on the voice entity, Playing is on the source entity.

public struct DSPTimeOnPlay : IComponentData
{
    public Entity Entity;
    public double Time;

    public DSPTimeOnPlay(Entity entity, double time)
    {
        Entity = entity;
        Time = time;
    }
}

public struct AudioProperty_AudioClipID : IComponentData
{
    public Entity Entity;
    public int ID;

    public AudioProperty_AudioClipID(Entity entity, int id)
    {
        Entity = entity;
        ID = id;
    }
}

public struct AudioProperty_SpatialBlend : IComponentData
{
    public Entity Entity;
    public float Blend;

    public AudioProperty_SpatialBlend(Entity entity, float blend)
    {
        Entity = entity;
        Blend = math.clamp(blend, 0, 1);
    }
}

public struct AudioProperty_Loop : IComponentData
{
    public Entity Entity;

    public AudioProperty_Loop(Entity entity)
    {
        Entity = entity;
    }
}

public struct AudioMessage_Played : IComponentData
{
    public Entity VoiceEntity;

    public AudioMessage_Played(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}

public struct AudioMessage_Stopped : IComponentData
{
    public Entity VoiceEntity;

    public AudioMessage_Stopped(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}

public struct AudioMessage_Muted : IComponentData
{
    public Entity VoiceEntity;

    public AudioMessage_Muted(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}

public struct AudioMessage_Unmuted : IComponentData
{
    public Entity VoiceEntity;

    public AudioMessage_Unmuted(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}


