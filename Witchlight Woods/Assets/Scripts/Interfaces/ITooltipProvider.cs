using UnityEngine;

namespace WitchlightWoods
{
    public interface ITooltipProvider
    {
        public Transform transform { get; }
        public TooltipUITemplate TooltipUITemplate { get; }
        public void UpdateContent(TooltipUITemplate tooltip);
    }
}