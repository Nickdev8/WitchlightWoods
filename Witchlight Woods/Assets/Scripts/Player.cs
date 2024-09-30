using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WitchlightWoods
{
    [RequireComponent(typeof(PlatformerAgent))]
    public class Player : MonoBehaviour
    {
        [SerializeField] private InputActionReference move;
        [SerializeField] private InputActionReference jump;
        [SerializeField] private InputActionReference walk;
        [SerializeField] private InputActionReference crouch;
        private PlatformerAgent _agent;
        
        private void Awake()
        {
            _agent = GetComponent<PlatformerAgent>();
        }

        private void Update()
        {
            _agent.SetMoveInput(move.action.ReadValue<Vector2>().x);
            _agent.SetJump(jump.action.IsPressed());
            _agent.SetWalk(walk.action.IsPressed());
            _agent.SetCrouch(crouch.action.IsPressed());
        }
    }
}