using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace WitchlightWoods.GOAP.Sensors
{
    public class IdleTargetSensor : LocalTargetSensorBase
    {
        public override void Created() {}
        public override void Update() {}

        public override ITarget Sense(IActionReceiver agent, IComponentReference references, ITarget existingTarget)
        {
            var random = Random.insideUnitCircle * 5;
            var position = agent.Transform.position + new Vector3(random.x, 0, random.y);
        
            if (existingTarget is PositionTarget positionTarget)
            {
                return positionTarget.SetPosition(position);
            }
            return new PositionTarget(position);
        }
    }
}