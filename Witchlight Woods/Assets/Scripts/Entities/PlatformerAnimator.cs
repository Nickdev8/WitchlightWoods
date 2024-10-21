using System;
using UnityEngine;

namespace WitchlightWoods
{
    [RequireComponent(typeof(PlatformerAgent))]
    public class PlatformerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private bool useTransformFlip;
        [SerializeField] private Transform transformFlipRoot;
        private PlatformerAgent _agent;
        private bool _facingRight = true;

        private void Awake()
        {
            _agent = GetComponent<PlatformerAgent>();
        }

        private void OnEnable()
        {
            _agent.OnJump += PlayJumpAnimation;
            _agent.OnLand += PlayLandAnimation;
        }

        private void OnDisable()
        {
            _agent.OnJump -= PlayJumpAnimation;
            _agent.OnLand -= PlayLandAnimation;
        }

        private void PlayJumpAnimation(bool wallJumped, int jumpCounter)
        {
            animator.SetTrigger("didJump");
        }

        private void PlayLandAnimation(float airBoneTime)
        {
            animator.SetTrigger("didLand");
        }

        private void Update()
        {
            animator.SetFloat("control", 1f - _agent.Config.stunFactor);
            var lastMoveInput = _agent.LastMoveInput;
            if (lastMoveInput != 0)
            {
                _facingRight = lastMoveInput > 0;
                animator.SetFloat("facingDir", lastMoveInput);
                if (useTransformFlip)
                    transformFlipRoot.localScale = new Vector3(_facingRight ? 1 : -1, 1, 1);
            }
            var rb = _agent.Rigidbody2D;
            animator.SetFloat("speed", Mathf.Abs(rb.linearVelocityX));
            animator.SetFloat("velocityX", rb.linearVelocityX);
            animator.SetFloat("velocityY", rb.linearVelocityY);
            animator.SetBool("grounded", _agent.Grounded);
            animator.SetBool("onWall", _agent.OnClimbableWall);
        }
    }
}