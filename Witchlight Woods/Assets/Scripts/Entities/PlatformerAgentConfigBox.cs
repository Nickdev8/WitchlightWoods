using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WitchlightWoods
{
    [CreateAssetMenu]
    public class PlatformerAgentConfigBox : ScriptableObject
    {
        [SerializeField] private PlatformerAgentConfig baseConfig;
        [SerializeReference] public List<IModifier<PlatformerAgentConfig>> modifiers = new();
        public PlatformerAgentConfig Config => modifiers!.Aggregate(baseConfig, (cfg, mod) => mod?.Modify(cfg) ?? cfg);

        public void UpdateModifiers(float deltaTime)
        {
            foreach (var modifier in modifiers!)
            {
                modifier?.Update(deltaTime);
            }

            modifiers.RemoveAll(m => m == null || m.ShouldRemove());
        }
    }
}