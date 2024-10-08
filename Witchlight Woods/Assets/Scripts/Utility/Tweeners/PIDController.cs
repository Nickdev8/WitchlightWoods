using System;
using UnityEngine;

namespace WitchlightWoods
{
    [Serializable]
    public class PIDController
    {
        public float gainScale = 1;
        [Range(0, 10)]
        public float proportionalGain = 4;
        [Range(0, 2)]
        public float integralGain = 0.2f;
        [Range(0, 2)]
        public float derivativeGain = 0.1f;
        public float max = 1;
        public float min = -1;

        private float _p;
        private float _i;
        private float _d;
        [SerializeField] public float error;
        private float _previousError;

        public void Reset()
        {
            for (var i = 0; i < 3; i++)
            {
                _p = 0;
                _i = 0;
                _d = 0;
                _previousError = 0;
            }
        }

        public float Get(float current, float target, float deltaTime)
        {
            if (deltaTime <= 0) return 0;
            error = target - current;

            _p = error;
            _i += _p * deltaTime;
            _d = (_p - _previousError) / deltaTime;
            _previousError = error;

            var output = (_p * proportionalGain + _i * integralGain + _d * derivativeGain) * gainScale;
            
            return Mathf.Clamp(output, min, max);
        }
    }
}