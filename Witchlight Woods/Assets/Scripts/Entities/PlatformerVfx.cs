using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace WitchlightWoods
{
    [RequireComponent(typeof(PlatformerAgent))]
    public class PlatformerVfx : MonoBehaviour
    {
        public LayerMask groundLayerMask = 1 | 1 << 8;
        public List<AnimatedVisualEffect> jumpVfx = new ();
        public float bigLandThreshold = 3f;
        public AnimatedVisualEffect smallLandVfx;
        public AnimatedVisualEffect bigLandVfx;
        
        [NotNull] private PlatformerAgent _platformerAgent;

        private void Awake()
        {
            _platformerAgent = GetComponent<PlatformerAgent>()!;
        }

        private void OnEnable()
        {
            _platformerAgent.OnJump += SpawnJumpVfx;
            _platformerAgent.OnLand += SpawnLandVfx;
        }

        private void OnDisable()
        {
            _platformerAgent.OnJump -= SpawnJumpVfx;
            _platformerAgent.OnLand -= SpawnLandVfx;
        }

        private void SpawnJumpVfx(bool isWallJump, int count)
        {
            if (!isWallJump)
            {
                if (count < 0 || count >= jumpVfx.Count) return;
                ObjectPooling.OneShotAnimation(jumpVfx[count], transform.position).Forget();
            }
        }

        private void SpawnLandVfx(float airBoneTime)
        {
            var hit = Physics2D.Raycast(transform.position, Vector2.down, 3f, groundLayerMask);
            if (!hit) return;
            var landingPos = hit.point;
            Debug.DrawLine(hit.point, hit.point + Vector2.up, Color.red);
            
            if (airBoneTime > bigLandThreshold)
            {
                ObjectPooling.OneShotAnimation(bigLandVfx, landingPos).Forget();
            }
            else
            {
                ObjectPooling.OneShotAnimation(smallLandVfx, landingPos).Forget();
            }
        }
    }

    [Serializable]
    public class AnimatedVisualEffect
    {
        public Animator prefab;
        public string stateName;
    }
}