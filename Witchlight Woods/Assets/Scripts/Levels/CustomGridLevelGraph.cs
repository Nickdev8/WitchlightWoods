using Pathfinding;
using Pathfinding.Serialization;

namespace WitchlightWoods.Levels
{
    [JsonOptIn]
    [Pathfinding.Util.Preserve]
    public class CustomGridLevelGraph : GridGraph
    {
        [JsonMember]
        public string ParentGuid;
    }
}