using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WitchlightWoods
{
    public class NavAgent2D : MonoBehaviour
    {
        public NavAgentConfig baseConfig;
        public readonly List<Func<NavAgentConfig, NavAgentConfig>> Modifiers = new ();
        public NavAgentConfig Config => Modifiers.Aggregate(baseConfig, (current, fn) => fn(current));

        protected float MoveInput;
        protected float MoveTimer;
        protected float CurrentDirection;
        protected float PreviousDirection;
        protected float SameDirectionMoveTimer;
        protected bool WantsToJump;
        
        public void SetMoveInput(float moveInput)
        {
            MoveInput = moveInput;
            if (moveInput == 0)
                MoveTimer = 0;
            CurrentDirection = Mathf.Sign(moveInput);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (PreviousDirection != CurrentDirection)
            {
                PreviousDirection = CurrentDirection;
                SameDirectionMoveTimer = 0;
            }
        }

        public void SetJump(bool wantsToJump)
        {
            WantsToJump = wantsToJump;
        }

        private void FixedUpdate()
        {
            //todo: store grounded frame
            //todo: slope friction
            //todo: count jump btn frames
            //todo: movement
            //todo: jump curves
            //todo: crouch
        }
    }

    [Serializable]
    public struct NavAgentConfig
    {
        [Header("Movement")]
        public float runSpeed;
        public bool canWalk;
        public float walkSpeed;
        public bool canCrouch;
        public float crouchSpeed;
        [Header("Jump")] 
        public int jumpCount;
        public bool canJump;
        public float jumpForce;
        public int minJumpFrames;
        public int maxJumpFrames;
        public int holdFrames;
        public int coyoteFrames;
        public int jumpBufferFrames;
        public float ascendGravityMultiplier;
        public float descendGravityMultiplier;
        public float descendLimit;
        [Header("Movement Curves")] 
        public AnimationCurve accelerationCurve;
        public AnimationCurve decelerationCurve;
        public AnimationCurve alteringDirectionCurve;
    }
}