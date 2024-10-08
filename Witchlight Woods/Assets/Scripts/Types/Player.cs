using System;
using JetBrains.Annotations;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using WitchlightWoods.Levels;

namespace WitchlightWoods
{
    [RequireComponent(typeof(PlatformerAgent))]
    public class Player : MonoBehaviour
    {
        public new Camera camera;
        
        [SerializeField] private InputActionReference interact;
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference jump;
        [SerializeField] private InputActionReference walk;
        [SerializeField] private InputActionReference crouch;
        [NotNull] private InteractionAgent _interactionAgent;
        [NotNull] private PlatformerAgent _agent;
        
        private void Awake()
        {
            _interactionAgent = GetComponent<InteractionAgent>()!;
            _agent = GetComponent<PlatformerAgent>()!;
        }

        private void Update()
        {
            _agent.SetMoveInput(move.action.ReadValue<Vector2>().x);
            _agent.SetJump(jump.action.IsPressed());
            _agent.SetWalk(walk.action.IsPressed());
            _agent.SetCrouch(crouch.action.IsPressed());
            
            _interactionAgent.SetInteract(interact.action.IsPressed());
        }
    }
}