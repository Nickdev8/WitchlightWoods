using System;
using UnityEngine;

namespace WitchlightWoods
{
    [Serializable]
    public struct PlatformerAgentConfig
    {
        public bool active;
        public bool controllable;
        [Header("Movement")]
        public float stunFactor;
        
        [Range(0, 60)] public byte accelerationFrames;
        [Range(0, 60)] public byte decelerationFrames;
        public float runSpeed;
        [Tooltip("When agent jumps, hits the wall, but doesn't after until this amount of frames, it keeps its momentum")]
        [Range(0, 20)] public byte momentumHoldFrames;
        
        public bool canWalk;
        public float walkSpeed;
        
        public bool canCrouch;
        public float crouchSpeed;
        
        [Header("Jump")] 
        [Range(0, 10)] public byte jumpCount;
        public bool canJump;
        public float jumpForce;
        
        [Range(0, 60)] public byte minJumpFrames;
        [Range(0, 60)] public byte maxJumpFrames;
        
        [Range(0, 20)] public byte holdFrames;
        [Range(0, 10)] public byte coyoteFrames;
        [Range(0, 10)] public byte jumpBufferFrames;
        
        [Range(0f, 10f)] public float ascendGravityMultiplier;
        [Range(0f, 10f)] public float descendGravityMultiplier;
        [Range(0f, 10f)] public float neutralGravityMultiplier;
        
        public float descendLimit;
        
        [Header("Wall climbing")]
        public bool canWallClimb;

        public float wallJumpForce;
        public bool wallHoldIndefinitely;
        public byte wallHoldFrames;
        public float wallGravityMultiplier;
    }
}