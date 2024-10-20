using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WitchlightWoods
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlatformerAgent : MonoBehaviour
    {
        [field: SerializeField]
        public PlatformerAgentConfigBox ConfigSource { get; private set; }
        public PlatformerAgentConfig Config => ConfigSource!.Config;
        public PlatformerAgentExtrasConfig ExtrasConfig;

        public float groundCheckRadius;
        
        public bool Grounded => (FrameTimer - LastGroundedFrame) <= Config.coyoteFrames;
        public event Action<bool, int> OnJump = (_, _) => {};
        public event Action<float> OnLand = (_) => {};
        
        protected float PreviousMoveInput;
        protected float MoveInput;
        public float LastPreviousMoveInput => PreviousMoveInput;
        public float LastMoveInput => MoveInput;
        protected float Momentum;
        protected bool WalkInput;
        protected bool CrouchInput;
        protected float CurrentDirection;
        protected float PreviousDirection;
        protected bool WantsToJump;
        protected bool Crouching;
        public bool OnClimbableWall { get; protected set; }
        protected bool WallHitBottom;
        protected bool WallHitTop;
        
        protected ulong FrameTimer = 60;
        protected ulong AccelerationFrames;
        protected ulong DecelerationFrames;
        protected ulong MomentumFrames;
        protected ulong JumpFrame;
        protected byte JumpCount;
        protected ulong SameDirectionMoveFrames;
        protected ulong LastGroundedFrame;
        protected ulong JumpInputFrame;
        protected ulong OnWallFrames;
        protected ulong WallJumpFrame;
        protected ulong WallJumpNoControlFrames;
        
        private Collider2D _collider;
        public Rigidbody2D Rigidbody2D { get; protected set; }
        public bool JumpAscendingPhase { get; protected set; }
        public float Speed { get; protected set; }

        private float _colliderLowestYPointOffset;
        private float _colliderHighestYPointOffset;
        private Bounds _colliderBounds;
        private readonly RaycastHit2D[] _wallHitBuffer = new RaycastHit2D[6];
        private readonly Collider2D[] _groundCheckCollisionBuffer = new Collider2D[6];
        private Vector2 _lastFrameVelocity;

        private void Awake()
        {
            //This is called from OnDrawGizmosSelected
            _collider = GetComponent<Collider2D>();
            Rigidbody2D = GetComponent<Rigidbody2D>();
            _colliderLowestYPointOffset = _collider.bounds.min.y - transform.position.y;
            _colliderHighestYPointOffset = _collider.bounds.max.y - transform.position.y;
            _colliderBounds = _collider.bounds;
            _colliderBounds.center -= transform.position;
        }

        public virtual void SetMoveInput(float moveInput, bool force = false)
        {
            if (!force && !Config.controllable) return;
            moveInput = Mathf.Clamp(moveInput, -1, 1);
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

        public virtual void SetJump(bool wantsToJump, bool force = false)
        {
            if (!force && !Config.controllable) return;
            if(wantsToJump && !WantsToJump)
                JumpInputFrame = FrameTimer;
            WantsToJump = wantsToJump;
        }

        public virtual void SetCrouch(bool state) => CrouchInput = state;
        public virtual void SetWalk(bool state) => WalkInput = state;

        protected virtual void FixedUpdate()
        {
            var position = Rigidbody2D.position;
            var rigidbodyVelocity = Rigidbody2D.linearVelocity;
            var velocityX = rigidbodyVelocity.x;
            var velocityY = rigidbodyVelocity.y;
            var config = Config; // Copy config, so it doesn't run getter multiple times
            var extrasConfig = ExtrasConfig;
            var movementSpeedFactor = 1f;
            var gravity = config.neutralGravityMultiplier;
            
            //Ground check
            if (Physics2D.OverlapCircle(position + new Vector2(0, (_colliderLowestYPointOffset - groundCheckRadius)), groundCheckRadius, config.groundCheckFilter, _groundCheckCollisionBuffer) > 0)
            {
                //On Grounded
                if (LastGroundedFrame != FrameTimer - 1)
                    OnLand((FrameTimer - LastGroundedFrame) / 50f);
                LastGroundedFrame = FrameTimer;
                JumpCount = 0;
            }
            
            if (config.active)
            {
                var groundedForJump = FrameTimer - LastGroundedFrame < config.coyoteFrames;
                var jumpFrameDiff = FrameTimer - JumpFrame;
                
                //Decide speed
                var speed = 0f;
                var controlFactor = Mathf.Clamp01(1f - config.stunFactor);

                if (WallJumpNoControlFrames > 0 && config.wallJumpControlCurveFrames > 0)
                {
                    controlFactor *= extrasConfig.wallJumpControlCurve.Evaluate((config.wallJumpControlCurveFrames - WallJumpNoControlFrames) / (float) config.wallJumpControlCurveFrames);
                    WallJumpNoControlFrames--;
                }

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
                
                //Jump
                if (jumpFrameDiff < config.maxJumpFrames) // While jumping/jumped
                {
                    if (WantsToJump) // Keep ascending
                    {
                        JumpAscendingPhase = true;
                        var jumpProgress = 1f;
                        if(config.maxJumpFrames != 0)
                            jumpProgress = (float) jumpFrameDiff / config.maxJumpFrames;
                        gravity = config.ascendGravityMultiplier * extrasConfig.ascendGravityCurve.Evaluate(jumpProgress);
                        movementSpeedFactor *= extrasConfig.ascendMovementSpeedCurve.Evaluate(jumpProgress);
                    }
                    else if(!groundedForJump)
                    {
                        JumpAscendingPhase = false;
                        JumpFrame = FrameTimer - config.maxJumpFrames;//Cancel jump
                        gravity = config.descendGravityMultiplier;
                    }
                }
                else if (jumpFrameDiff > ((uint)config.maxJumpFrames + config.holdFrames) && !groundedForJump)
                {
                    JumpAscendingPhase = false;
                    gravity = config.descendGravityMultiplier;
                }

                if (config.canJump && 
                          OnClimbableWall && 
                          config.canWallClimb && 
                          FrameTimer - JumpFrame > config.minJumpFrames &&
                          FrameTimer - JumpInputFrame < config.jumpBufferFrames)
                {
                    JumpInputFrame = 0;
                    //Wall jump
                    JumpFrame = FrameTimer;
                    WallJumpFrame = FrameTimer;
                    WallJumpNoControlFrames = (uint)config.wallJumpControlCurveFrames + config.wallJumpZeroControlFrames;
                    velocityX = config.wallJumpForce.x;
                    velocityY = config.wallJumpForce.y;
                    velocityX *= MoveInput;
                    gravity = config.ascendGravityMultiplier;
                    OnJump(true, 0);
                    JumpCount = 1;
                }else if (config.canJump &&
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
                    velocityY = config.jumpForce;
                    gravity = config.ascendGravityMultiplier;
                    OnJump(false, JumpCount);
                    JumpCount++;
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

                    var wantedVelocityX = Mathf.LerpUnclamped(velocityX, (MoveInput * speed * movementSpeedFactor) + Momentum, controlFactor);
                    var absoluteVelocityX = Mathf.Max(Mathf.Abs(wantedVelocityX), Mathf.Abs(MoveInput), Mathf.Abs(velocityX)) * Time.fixedDeltaTime;

                    var wallCheckDirection = MoveInput < 0 ? Vector2.left : Vector2.right;
                    var wallCheckDistance = absoluteVelocityX + _colliderBounds.extents.x + 0.01f;
                    
                    var bottomWallHits = Physics2D.Raycast(position + new Vector2(0, _colliderLowestYPointOffset + 0.01f), wallCheckDirection, config.wallCheckFilter, _wallHitBuffer, wallCheckDistance);
                    var topWallHits = Physics2D.Raycast(position + new Vector2(0, _colliderHighestYPointOffset - 0.01f), wallCheckDirection, config.wallCheckFilter, _wallHitBuffer, wallCheckDistance);
                    
                    var bottomClimbableWallHits = Physics2D.Raycast(position + new Vector2(0, _colliderLowestYPointOffset + 0.01f), wallCheckDirection, config.climbableWallCheckFilter, _wallHitBuffer, wallCheckDistance);
                    var topClimbableWallHits = Physics2D.Raycast(position + new Vector2(0, _colliderHighestYPointOffset - 0.01f), wallCheckDirection, config.climbableWallCheckFilter, _wallHitBuffer, wallCheckDistance);
                    
                    WallHitBottom = bottomWallHits > 0;
                    WallHitTop = topWallHits > 0;
                    
                    OnClimbableWall = config.canWallClimb && bottomClimbableWallHits > 0 && topClimbableWallHits > 0 && !groundedForJump;

                    var wallJumpInProgress = FrameTimer - config.wallJumpZeroControlFrames <= WallJumpFrame;
                    
                    if (!WallHitBottom && !WallHitTop && !wallJumpInProgress)
                    {
                        velocityX = wantedVelocityX;
                        MomentumFrames = 0;
                        Momentum = 0;
                        OnWallFrames = 0;
                    }
                    else
                    {
                        if(!wallJumpInProgress)
                            velocityX = 0;
                        if (MomentumFrames <= config.momentumHoldFrames)
                        {
                            Momentum = wantedVelocityX > 0
                                ? Mathf.Max(Momentum, wantedVelocityX)
                                : Mathf.Min(Momentum, wantedVelocityX);
                            Momentum = Mathf.Clamp(Momentum, -speed / 2f, speed / 2f);
                        }
                        else
                        {
                            Momentum = 0;

                            if (OnClimbableWall && !wallJumpInProgress)
                            {
                                
                                if (OnWallFrames > config.wallHoldFrames && !config.wallHoldIndefinitely)
                                {
                                    if (velocityY <= 0){
                                        velocityX *= 0.1f;
                                        velocityY = Mathf.Max(velocityY, -config.wallDescendLimit);
                                        gravity = config.wallGravityMultiplier;
                                    }
                                }
                                else
                                {
                                    gravity = 0;
                                    velocityY = 0;
                                }
                                
                                OnWallFrames++;
                            }
                            else
                            {
                                OnWallFrames = 0;
                            }
                        }
                        MomentumFrames++;
                    }
                }
                else
                {
                    //Decelerate
                    DecelerationFrames++;
                    WallHitBottom = false;
                    WallHitTop = false;
                    OnClimbableWall = false;
                    Momentum = 0;
                    MomentumFrames = 0;
                    OnWallFrames = 0;
                    
                    // var decelerationProgress = 1f;
                    // if(config.decelerationFrames != 0)
                    //     decelerationProgress = (float)DecelerationFrames / config.decelerationFrames;
                    //
                    // if (decelerationProgress > 1f) decelerationProgress = 1f;
                    //
                    // var previousDecelerationProgress = 0f;
                    // if(config.decelerationFrames != 0)
                    //     previousDecelerationProgress = decelerationProgress - (1f / config.decelerationFrames);

                    var decelerationStep = 1f;
                    if(config.decelerationFrames != 0)
                        decelerationStep = 1f / config.decelerationFrames;
                    
                    velocityX = Mathf.LerpUnclamped(velocityX, 0f, decelerationStep * controlFactor);
                }
                
                velocityY = Mathf.Max(velocityY, -config.descendLimit);
                Speed = Mathf.Abs(velocityX) * controlFactor;
                var velocity = new Vector2(velocityX, velocityY);
                
                Debug.DrawRay(position, velocity, Color.magenta, Time.fixedDeltaTime);
                Rigidbody2D.gravityScale = gravity;
                Rigidbody2D.linearVelocity = velocity;
            }

            
            //todo: crouch
            //todo: slope friction
            _lastFrameVelocity = rigidbodyVelocity;
            FrameTimer++;
        }

        private void OnDrawGizmosSelected()
        {
            if (Rigidbody2D == null) Awake();
            var position = Rigidbody2D.position;
            var velocity = Rigidbody2D.linearVelocity;
            Gizmos.color = Grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(position + new Vector2(0, (_colliderLowestYPointOffset - groundCheckRadius)), groundCheckRadius);
            // var colliderEdge = MoveInput < 0 ? _colliderBounds.min.x + 0.01f : _colliderBounds.max.x - 0.01f;
            // Gizmos.color = WallHitBottom ? Color.red : Color.green;
            // Gizmos.DrawLine(position + new Vector2(colliderEdge, _colliderLowestYPointOffset + 0.05f), position + new Vector2(colliderEdge + DesiredVelocityX, _colliderLowestYPointOffset + 0.05f));
            
        }
    }
}