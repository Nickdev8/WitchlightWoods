using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace WitchlightWoods
{
    [GoapId("Idle-32a92176-5f00-4742-9fe7-c3470978a5cf")]
    public class IdleAction : GoapActionBase<IdleAction.Data>
    {
        public override void Start(IMonoAgent agent, Data data)
        {
            data.RandomTime = Random.Range(0f, 5f);
        }

        // This method is called every frame while the action is running
        // This method is required
        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            return ActionRunState.Completed;
        }

        // The action class itself must be stateless!
        // All data should be stored in the data class
        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float RandomTime { get; set; }
        }
    }
}