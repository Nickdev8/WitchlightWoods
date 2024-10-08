using System;
using UnityEngine;

namespace WitchlightWoods
{
    [RequireComponent(typeof(PlatformerAgent))]
    public class PlatformerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private PlatformerAgent _agent;

        private void Awake()
        {
            _agent = GetComponent<PlatformerAgent>();
        }

        private void Update()
        {
            animator.SetFloat("control", 1f - _agent.Config.stunFactor);
            animator.SetFloat("facingDir", _agent.LastMoveInput);
            animator.SetFloat("velocityX", _agent.Rigidbody2D.linearVelocityX);
            animator.SetBool("grounded", _agent.Grounded);
            animator.SetBool("ascending", _agent.JumpAscendingPhase);
        }
    }
}