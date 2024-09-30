using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace WitchlightWoods.GOAP.Behaviours
{
    public class BrainBehaviour : MonoBehaviour
    {
        private AgentBehaviour _agent;
        private GoapActionProvider _provider;
        private GoapBehaviour _goap;
        
        private void Awake()
        {
            _goap = FindFirstObjectByType<GoapBehaviour>();
            _agent = GetComponent<AgentBehaviour>();
            _provider = GetComponent<GoapActionProvider>();
            
            // This only applies sto the code demo
            if (_provider.AgentTypeBehaviour == null)
                _provider.AgentType = _goap.GetAgentType("TestDemoAgent");
        }

        private void Start()
        {
            _provider.RequestGoal<IdleGoal>();
        }
    }
}