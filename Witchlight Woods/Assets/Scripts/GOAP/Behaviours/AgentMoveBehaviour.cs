using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using UnityEngine;

namespace WitchlightWoods.GOAP.Behaviours
{
    public class AgentMoveBehaviour : MonoBehaviour
    {
        private AgentBehaviour _agent;
        private ITarget _currentTarget;
        private bool _shouldMove;

        private void Awake()
        {
            _agent = GetComponent<AgentBehaviour>();
        }

        private void OnEnable()
        {
            _agent.Events.OnTargetInRange += OnTargetInRange;
            _agent.Events.OnTargetChanged += OnTargetChanged;
            _agent.Events.OnTargetNotInRange += TargetNotInRange;
            _agent.Events.OnTargetLost += TargetLost;
        }

        private void OnDisable()
        {
            _agent.Events.OnTargetInRange -= OnTargetInRange;
            _agent.Events.OnTargetChanged -= OnTargetChanged;
            _agent.Events.OnTargetNotInRange -= TargetNotInRange;
            _agent.Events.OnTargetLost -= TargetLost;
        }
        
        private void TargetLost()
        {
            _currentTarget = null;
            _shouldMove = false;
        }

        private void OnTargetInRange(ITarget target)
        {
            _shouldMove = false;
        }

        private void OnTargetChanged(ITarget target, bool inRange)
        {
            _currentTarget = target;
            _shouldMove = !inRange;
        }

        private void TargetNotInRange(ITarget target)
        {
            _shouldMove = true;
        }

        public void Update()
        {
            if (!_shouldMove)
                return;
            
            if (_currentTarget == null)
                return;
            
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(_currentTarget.Position.x, transform.position.y, _currentTarget.Position.z), Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            if (_currentTarget == null)
                return;
            
            Gizmos.DrawLine(transform.position, _currentTarget.Position);
        }
    }
}