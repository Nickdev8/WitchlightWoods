using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace WitchlightWoods
{
    [RequireComponent(typeof(PlatformerAgent))]
    public class PlatformerVfx : MonoBehaviour
    {
        public List<AnimatedVisualEffect> jumpVfx = new ();
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
        }

        private void SpawnJumpVfx(bool isWallJump, int count)
        {
            if (!isWallJump)
            {
                if (count < 0 || count > jumpVfx.Count) return;
                ObjectPooling.OneShotAnimation(jumpVfx[count], transform.position).Forget();
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