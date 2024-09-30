using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using WitchlightWoods.GOAP.Capabilities;

namespace WitchlightWoods.GOAP.AgentTypes
{
    public class TestAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("TestDemoAgent");
        
            factory.AddCapability<IdleCapabilityFactory>();
        
            return factory.Build();
        }
    }
}