using System;
using JetBrains.Annotations;
using UnityEngine;

namespace WitchlightWoods
{
    public class InteractionAgent : MonoBehaviour
    {
        public Vector2 interactionRoot;
        public float interactionRadius = 0.5f;
        public ContactFilter2D interactableFilter;
        
        public bool canPickupObjects = true;
        public Pickupable heldObject;
        public Vector2 heldObjectAnchor;
        
        public bool wantsToInteract;
        private bool _isInteracting;
        private bool _outOfReach;
        private IInteractable _interactable;
        
        [NotNull] private readonly Collider2D[] _resultsBuffer = new Collider2D[10];
        public Rigidbody2D Rigidbody2D { get; private set; }

        private void Awake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public void SetInteract(bool interact)
        {
            wantsToInteract = interact;
        }

        private void FixedUpdate()
        {
            if (_isInteracting && (_outOfReach || !wantsToInteract))
            {
                _isInteracting = false;
                _interactable!.OnInteractionStop(this, _outOfReach);
                _interactable = null;
            } else if (wantsToInteract && !_isInteracting)
            {
                var results = Physics2D.OverlapCircle((Vector2)transform.position + interactionRoot, interactionRadius, interactableFilter, _resultsBuffer);
                for (var i = 0; i < results; i++)
                {
                    if (!_resultsBuffer[i]!.gameObject.TryGetComponent(out IInteractable newInteractable)) continue;

                    _interactable = newInteractable;
                    _outOfReach = false;
                    _isInteracting = true;
                    _interactable!.OnInteractionStart(this);
                }
            }else if (wantsToInteract && _isInteracting && !_outOfReach)
            {
                _interactable!.OnInteractionUpdate(this);
                
                var distanceSqr = ((Vector2)transform.position + interactionRoot - (Vector2)_interactable.transform!.position).sqrMagnitude;

                if (distanceSqr > interactionRadius * interactionRadius)
                    _outOfReach = true;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere((Vector2)transform.position + interactionRoot, interactionRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube((Vector2)transform.position + heldObjectAnchor, Vector3.one * 0.5f);
        }
    }
}