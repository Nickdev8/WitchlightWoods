using System;
using JetBrains.Annotations;
using UnityEngine;

namespace WitchlightWoods
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Pickupable : MonoBehaviour, IInteractable
    {
        public bool held;
        [NotNull] private Collider2D _collider2D;
        [NotNull] private Rigidbody2D _rigidbody2D;
        private bool _interactionButtonReleased;
        private InteractionAgent _heldBy;
        private RelativeJoint2D _joint;

        private void Awake()
        {
            _collider2D = GetComponent<Collider2D>()!;
            _rigidbody2D = GetComponent<Rigidbody2D>()!;
        }

        public void OnInteractionStart(InteractionAgent agent)
        {
            if(held)
                Drop(agent);
            else
                PickUp(agent);
        }

        private void Update()
        {
            if (!held) return;
            // if (!_heldBy.wantsToInteract)
            //     _interactionButtonReleased = true;
            // if (_interactionButtonReleased && _heldBy.wantsToInteract)
            //     Drop(_heldBy);
        }

        private void PickUp(InteractionAgent agent)
        {
            if (!agent.canPickupObjects || agent.heldObject != null) return;

            if (held) return;
            held = true;
            _interactionButtonReleased = false;
            agent.heldObject = this;

            _collider2D.isTrigger = true;
            _joint = agent.gameObject.AddComponent<RelativeJoint2D>();
            _joint.connectedBody = _rigidbody2D;
            _joint.autoConfigureOffset = false;
            _joint.angularOffset = 0;
            _joint.linearOffset = agent.heldObjectAnchor;
            
            _heldBy = agent;
        }

        public void OnInteractionStop(InteractionAgent agent, bool outOfReach)
        {
            _interactionButtonReleased = true;
            if(outOfReach)
                Drop(agent);
        }

        private void Drop(InteractionAgent agent)
        {
            if(!held || agent != _heldBy) return;
            _collider2D.isTrigger = false;
            held = false;
            agent.heldObject = null;
            _heldBy = null;
            Destroy(_joint);
        }
    }
}