using System;
using FMODUnity;
using JetBrains.Annotations;
using UnityEngine;

namespace WitchlightWoods
{
    [RequireComponent(typeof(PlatformerAgent))]
    public class PlatformerSfx : MonoBehaviour
    {
        [SerializeField] private EventReference footsteps;
        [SerializeField] private EventReference land;
        [SerializeField] private EventReference jump;
        [SerializeField] private EventReference wallClimb;
        [SerializeField] private RemapFloat footstepsInterval = new (0.3f, 1f, 0.5f, 0.2f);

        [NotNull] private PlatformerAgent _platformerAgent;

        private void Awake()
        {
            _platformerAgent = GetComponent<PlatformerAgent>()!;
        }

        private void OnEnable()
        {
            _platformerAgent.OnJump += PlayJumpSfx;
        }

        private void OnDisable()
        {
            _platformerAgent.OnJump -= PlayJumpSfx;
        }

        private void PlayJumpSfx(int jumpCount)
        {
            RuntimeManager.PlayOneShotAttached(jump, gameObject);
        }

        private void Update()
        {
            var currentFootstepsInterval = footstepsInterval.Get(_platformerAgent.Speed);
        }
    }

    [Serializable]
    public struct RemapFloat
    {
        public float from;
        public float to;
        public float fromValue;
        public float toValue;

        public RemapFloat(float from = 0f, float to = 1f, float fromValue = 0f, float toValue = 2f)
        {
            this.from = from;
            this.to = to;
            this.fromValue = fromValue;
            this.toValue = toValue;
        }

        public float Get(float value)
        {
            value = Mathf.Clamp(value, from, to);
            return fromValue + (toValue - fromValue) * ((value - from) / (to - from));
        }
    }
}