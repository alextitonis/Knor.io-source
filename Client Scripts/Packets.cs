using System;
using System.Collections.Generic;
using UnityEngine;

public class IdHolder_String
{
    public string id;
}
public class IdHolder_Int
{
    public int id;
}

public class RegistrationPacket : IdHolder_String
{
}
public class NamePacket : IdHolder_String
{
    public string name;
}
public class PlayPacket : IdHolder_String
{
    public int charId;
}
public class SpawnPacket : IdHolder_String
{
    public string name;
    public bool isBot;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public int charId;
    public float maxHealth, currentHealth;
}
public class MovementPacket : RotationPacket
{
    public float x, y, z;
}
public class RotationPacket : MovementAnimationPacket
{
    public float rx, ry, rz, rw;
}
public class MovementAnimationPacket : IdHolder_String
{
    public float horizontal;
    public float vertical;
    public float inputMagnitude;
    public float moveSetID;
    public bool isGrounded;
    public bool isCrouching;
    public bool isStrafing;
    public float verticalVelocity;
}
public class JumpAnimationPacket : IdHolder_String
{
    public bool jumpMove;
}
public class AttackAnimationPacket : IdHolder_String
{
    public int randomAttack;
    public bool weakAttack;
    public bool strongAttack;
    public bool isBlocking;
    public bool triggerHitReaction;
    public int hitReaction;
}
public class AnimationResetPacket : IdHolder_String
{
    public bool resetState;
}
public class DamagePacket : IdHolder_String
{
    public int damage;
}
public class DeathPacket : IdHolder_String
{
}
public class RespawnPacketRequest : IdHolder_String
{
    public bool accept;
}
public class RespawnPacket : IdHolder_String
{
    public string name;
    public bool isBot;
    public int charId;
    public float posX, posY, posZ;
    public int maxHealth, currentHealth;
}
public class DisconnectedPacket :IdHolder_String
{
}
public class ScorePacket
{
    public int k1, k2;
}
public class ChatPacket
{
    public string sender, msg;
}
public class ZoneDataPacket : IdHolder_Int
{
    public float centerX, centerY, centerZ;
    public int ownerID;
}
public class ZoneUpdatePacket : IdHolder_Int
{
    public int ownerID;
}
public class BotRegistration : IdHolder_String
{
}
public class AIShootPacket : IdHolder_String
{
}
public class AIPlayAnimation : IdHolder_String
{
    public UnityEngine.Object clip;
}
public class AIPlayAnimations : IdHolder_String
{
    public IList<AnimationClip> clips;
}
public class AIAnimation : IdHolder_String
{
    public enum Animation
    {
        none = -1,
        idle = 0,
        walk = 1,
        run = 2,
        aim = 3,
        shoot = 4,
        die = 5,
    }
    public Animation anim;
}