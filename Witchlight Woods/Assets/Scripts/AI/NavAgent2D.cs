using System;
using Pathfinding;
using UnityEngine;

namespace WitchlightWoods
{
    [RequireComponent(typeof(PlatformerAgent))]
    [RequireComponent(typeof(Seeker))]
    public class NavAgent2D : MonoBehaviour
    {
        public bool isTransformTarget;
        public Transform transformTarget;
        public float distanceTolerance = 0.3f;
        public float maxDistanceTolerance = 5f;
        public float wallClimbDistance = 1;
        public float wallJumpTime = 0.1f;
        public float jumpThreshold = 0.6f;
        public float jumpTime = 1f;
        public float minJumpTime = 0.1f;
        private Vector2 _currentTarget;
        private float _wallJumpTimer;
        [SerializeField] private Vector2 currentPoint;
        [SerializeField] private Vector2 diff;
        private bool _didJump;
        private float _jumpTimer;
        
        private PlatformerAgent _agent;
        private Seeker _seeker;

        private bool _wasWallClimbing;
        private bool _reachedEnd;
        [SerializeField] private int waypoint;
        private Path _path;
        private RaycastHit2D[] _wallBuffer = new RaycastHit2D[4];

        private void Awake()
        {
            _agent = GetComponent<PlatformerAgent>();
            _seeker = GetComponent<Seeker>();
        }

        private void Update()
        {
            var position = (Vector2)transform.position;

            if (isTransformTarget)
            {
                if (((Vector2) transformTarget.position - _currentTarget).sqrMagnitude >= 1f)
                {
                    SetTarget(transformTarget);
                    return;
                }
            }

            if (_path == null || _reachedEnd || waypoint >= _path.vectorPath.Count) return;
            
            currentPoint = _path.vectorPath[waypoint];
            var nextPoint = currentPoint;
                
            if (waypoint + 1 < _path.vectorPath.Count)
                nextPoint = _path.vectorPath[waypoint + 1];
                
            diff = currentPoint - position;
            var nextDiff = nextPoint - currentPoint;
            var distanceSqr = diff.sqrMagnitude;

            if (!_agent.OnClimbableWall)
            {
                var canWallClimb = _agent.Config.canWallClimb;
                var didClimb = false;
                if (canWallClimb)
                {
                    if (Physics2D.Raycast(position, Vector2.right, _agent.Config.climbableWallCheckFilter,
                            _wallBuffer, wallClimbDistance) > 0)
                    {
                        _agent.SetMoveInput(1);
                        didClimb = true;
                    }
                    else if (Physics2D.Raycast(position, Vector2.left,
                                 _agent.Config.climbableWallCheckFilter, _wallBuffer, wallClimbDistance) > 0)
                    {
                        _agent.SetMoveInput(-1);
                        didClimb = true;
                    }
                }

                if (!didClimb)
                {
                    _agent.SetMoveInput(diff.x);
                }
                else if(!_wasWallClimbing)
                {
                    _wallJumpTimer = wallJumpTime;
                }

                _wasWallClimbing = didClimb;
            }

            if ((diff.y + nextDiff.y) > jumpThreshold && !_didJump)
            {
                if (_wallJumpTimer >= 0f)
                    _wallJumpTimer -= Time.deltaTime;
                else
                {
                    _agent.SetJump(true);
                    _didJump = true;
                    _jumpTimer = 1;
                }
            }

            if (_didJump)
            {
                _jumpTimer -= Time.deltaTime;
                _didJump = _jumpTimer <= 0f || (_jumpTimer <= jumpTime - minJumpTime && _agent.Rigidbody2D.linearVelocityY <= 0f);
                _agent.SetJump(_didJump);
            }

            if (distanceSqr <= distanceTolerance)
            {
                waypoint++;
                if (waypoint >= _path.vectorPath.Count)
                {
                    _reachedEnd = true;
                        
                    _agent.SetMoveInput(0);
                    _agent.SetJump(false);
                }
            }else if (distanceSqr >= maxDistanceTolerance)
            {
                if(isTransformTarget)
                    SetTarget(transformTarget);
                else
                    SetTarget(_currentTarget);
            }
        }

        public void SetTarget(Vector3 target)
        {
            isTransformTarget = false;
            waypoint = 0;
            _reachedEnd = false;
            _currentTarget = target;
            _seeker.StartPath(transform.position, target, OnPathCallback);
        }
        
        public void SetTarget(Transform target)
        {
            if (target == null) return;
            isTransformTarget = true;
            transformTarget = target;
            waypoint = 0;
            _reachedEnd = false;
            _currentTarget = target.position;
            _seeker.StartPath(transform.position, target.position, OnPathCallback);
        }

        private void OnPathCallback(Path p)
        {
            if (p.error)
            {
                Debug.LogError(p.errorLog);
                return;
            }

            _path = p;
            waypoint = 0;
            _reachedEnd = false;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3) diff);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_currentTarget, 0.2f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(currentPoint, 0.2f);
        }
    }
}
