﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WitchlightWoods
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlatformerAgent : MonoBehaviour
    {
        public PlatformerAgentConfig baseConfig;
        public readonly List<Func<PlatformerAgentConfig, PlatformerAgentConfig>> Modifiers = new ();
        public PlatformerAgentConfig Config => Modifiers.Aggregate(baseConfig, (current, fn) => fn(current));
        
        public PlatformerAgentExtrasConfig /*base*/ExtrasConfig = new ();
        // public readonly List<Func<PlatformerAgentExtrasConfig, PlatformerAgentExtrasConfig>> ExtrasModifiers = new ();
        // public PlatformerAgentExtrasConfig ExtrasConfig => ExtrasModifiers.Aggregate(baseExtrasConfig, (current, fn) => fn(current));

        public ContactFilter2D groundCheckFilter;
        public float groundCheckRadius;
        
        public bool Grounded => (FrameTimer - LastGroundedFrame) <= Config.coyoteFrames;
        public event Action<int> OnJump = _ => {};
        
        protected float PreviousMoveInput;
        protected float MoveInput;
        protected bool WalkInput;
        protected bool CrouchInput;
        protected float CurrentDirection;
        protected float PreviousDirection;
        protected bool WantsToJump;
        protected bool Crouching;
        protected bool OnWall;
        
        protected ulong FrameTimer = 60;
        protected ulong AccelerationFrames;
        protected ulong DecelerationFrames;
        protected ulong JumpFrame;
        protected byte JumpCount;
        protected ulong SameDirectionMoveFrames;
        protected ulong LastGroundedFrame;
        protected ulong JumpInputFrame;

        private Collider2D _collider;
        private Rigidbody2D _rigidbody2D;
        private float _colliderLowestYPointOffset;
        private readonly Collider2D[] _groundCheckCollisionBuffer = new Collider2D[6];

        private void Awake()
        {
            //This is called from OnDrawGizmosSelected
            _collider = GetComponent<Collider2D>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _colliderLowestYPointOffset = _collider.bounds.min.y - transform.position.y;
        }

        public void SetMoveInput(float moveInput, bool force = false)
        {
            if (!force && !Config.controllable) return;
            PreviousMoveInput = MoveInput;
            MoveInput = moveInput;
            if (moveInput == 0 && PreviousMoveInput == 0)
            {
                // Still for 2 frames
                AccelerationFrames = 0;
                SameDirectionMoveFrames = 0;
            }
            else
            {
                DecelerationFrames = 0;
            }

            CurrentDirection = Mathf.Sign(moveInput);
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (PreviousDirection == CurrentDirection) return;
            PreviousDirection = CurrentDirection;
            SameDirectionMoveFrames = 0;
        }

        public void SetJump(bool wantsToJump, bool force = false)
        {
            if (!force && !Config.controllable) return;
            if(wantsToJump && !WantsToJump)
                JumpInputFrame = FrameTimer;
            WantsToJump = wantsToJump;
        }

        public void SetCrouch(bool state) => CrouchInput = state;
        public void SetWalk(bool state) => WalkInput = state;

        private void FixedUpdate()
        {
            var position = _rigidbody2D.position;
            var config = Config; // Copy config, so it doesn't run getter multiple times
            var extrasConfig = ExtrasConfig;
            var movementSpeedFactor = 1f;
            //Ground check
            if (Physics2D.OverlapCircle(position + new Vector2(0, (_colliderLowestYPointOffset - groundCheckRadius)), groundCheckRadius, groundCheckFilter, _groundCheckCollisionBuffer) > 0)
            {
                //On Grounded
                LastGroundedFrame = FrameTimer;
                JumpCount = 0;
                _rigidbody2D.gravityScale = config.neutralGravityMultiplier;
            }
            
            if (config.active)
            {
                var groundedForJump = FrameTimer - LastGroundedFrame < config.coyoteFrames;
                var jumpFrameDiff = FrameTimer - JumpFrame;
                if (jumpFrameDiff < config.maxJumpFrames) // While jumping/jumped
                {
                    if (WantsToJump) // Keep ascending
                    {
                        var jumpProgress = 1f;
                        if(config.maxJumpFrames != 0)
                            jumpProgress = (float) jumpFrameDiff / config.maxJumpFrames;
                        _rigidbody2D.gravityScale = config.ascendGravityMultiplier * extrasConfig.ascendGravityCurve.Evaluate(jumpProgress);
                        movementSpeedFactor *= extrasConfig.ascendMovementSpeedCurve.Evaluate(jumpProgress);
                    }
                    else if(!groundedForJump)
                    {
                        JumpFrame = FrameTimer - config.maxJumpFrames;//Cancel jump
                        _rigidbody2D.gravityScale = config.descendGravityMultiplier;
                    }
                }
                else if (jumpFrameDiff > config.maxJumpFrames && !groundedForJump)
                {
                    _rigidbody2D.gravityScale = config.descendGravityMultiplier;
                }

                if (config.canJump &&
                    FrameTimer - JumpFrame > config.minJumpFrames &&
                    config.jumpCount - JumpCount > 0 &&
                    FrameTimer - JumpInputFrame < config.jumpBufferFrames &&
                    (groundedForJump || config.jumpCount - 1 - JumpCount > 0))
                {
                    JumpInputFrame = 0;
                    //Start jump
                    if (!groundedForJump)
                        JumpCount++;
                    JumpFrame = FrameTimer;
                    _rigidbody2D.linearVelocityY = config.jumpForce;
                    _rigidbody2D.gravityScale = config.ascendGravityMultiplier;
                    OnJump(JumpCount);
                    JumpCount++;
                }

                //Decide speed
                var speed = 0f;
                var controlFactor = Mathf.Clamp01(1f - config.stunFactor);
                
                if (config.canWalk)
                {
                    if (WalkInput)
                    {
                        speed = config.walkSpeed;
                    }
                    else
                    {
                        speed = config.runSpeed;
                    }
                }
                else
                {
                    speed = config.runSpeed;
                }
                
                if (MoveInput != 0)
                {
                    //Accelerate
                    var accelerationProgress = 1f;
                    if(config.accelerationFrames != 0)
                        accelerationProgress = (float) AccelerationFrames / config.accelerationFrames;
                    if (accelerationProgress > 1f) accelerationProgress = 1f;
                    
                    movementSpeedFactor *= extrasConfig.accelerationCurve.Evaluate(accelerationProgress);
                    AccelerationFrames++;
                    
                    var alterDirectionProgress = 1f;
                    if(config.accelerationFrames != 0)
                        alterDirectionProgress = (float)SameDirectionMoveFrames / config.accelerationFrames;
                    if (alterDirectionProgress > 1f) alterDirectionProgress = 1f;
                    
                    movementSpeedFactor *= extrasConfig.alteringDirectionCurve.Evaluate(alterDirectionProgress);
                    SameDirectionMoveFrames++;
                    
                    _rigidbody2D.linearVelocityX = Mathf.LerpUnclamped(_rigidbody2D.linearVelocityX, MoveInput * speed * movementSpeedFactor, controlFactor);
                }
                else
                {
                    //Decelerate
                    DecelerationFrames++;
                    
                    var decelerationProgress = 1f;
                    if(config.decelerationFrames != 0)
                        decelerationProgress = (float)DecelerationFrames / config.decelerationFrames;
                    
                    if (decelerationProgress > 1f) decelerationProgress = 1f;
                    
                    var previousDecelerationProgress = 0f;
                    if(config.decelerationFrames != 0)
                        previousDecelerationProgress = decelerationProgress - (1f / config.decelerationFrames);

                    var decelerationStep = 1f;
                    if(config.decelerationFrames != 0)
                        decelerationStep = 1f / config.decelerationFrames;
                    decelerationStep *= (extrasConfig.decelerationCurve.Evaluate(decelerationProgress) - extrasConfig.decelerationCurve.Evaluate(previousDecelerationProgress));
                    decelerationStep = Mathf.Clamp01(decelerationStep);
                    
                    _rigidbody2D.linearVelocityX = Mathf.LerpUnclamped(_rigidbody2D.linearVelocityX, 0f, decelerationStep * controlFactor);
                }
            }

            _rigidbody2D.linearVelocityY = Mathf.Max(_rigidbody2D.linearVelocityY, -config.descendLimit);

            //todo: crouch
            //todo: wall climb
            //todo: wall: keep velocity if soon to be out of way
            //todo: slope friction
            FrameTimer++;
        }

        private void OnDrawGizmosSelected()
        {
            if (_rigidbody2D == null) Awake();
            var position = _rigidbody2D.position;
            Gizmos.color = Grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(position + new Vector2(0, (_colliderLowestYPointOffset - groundCheckRadius)), groundCheckRadius);
        }
    }
}