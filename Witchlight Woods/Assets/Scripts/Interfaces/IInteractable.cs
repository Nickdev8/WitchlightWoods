using JetBrains.Annotations;
using UnityEngine;

namespace WitchlightWoods
{
    public interface IInteractable
    {
        [NotNull] public Transform transform { get; }
        public void OnInteractionStart([NotNull] InteractionAgent agent);
        public void OnInteractionUpdate([NotNull] InteractionAgent agent) {}
        public void OnInteractionStop([NotNull] InteractionAgent agent, bool outOfReach) {}
    }
}