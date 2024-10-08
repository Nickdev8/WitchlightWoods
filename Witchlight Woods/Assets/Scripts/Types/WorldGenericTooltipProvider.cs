using UnityEngine;

namespace WitchlightWoods
{
    public class WorldGenericTooltipProvider : MonoBehaviour, ITooltipProvider
    {
        [SerializeField] private TooltipUITemplate tooltipTemplate;
        public TooltipUITemplate TooltipUITemplate => tooltipTemplate;
        public void UpdateContent(TooltipUITemplate tooltip)
        {
            
        }
    }
}