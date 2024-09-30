using System;
using UnityEngine;

namespace WitchlightWoods
{
    [Serializable]
    public class PIDControllerVector2
    {
        public PIDController x;
        public PIDController y;

        public void CopyParametersToY()
        {
            y.proportionalGain = x.proportionalGain;
            y.integralGain = x.integralGain;
            y.derivativeGain = x.derivativeGain;
            y.min = x.min;
            y.max = x.max;
        }
        
        public Vector2 Get(Vector2 current, Vector2 target, float deltaTime) => new(x.Get(current.x, target.x, deltaTime), y.Get(current.y, target.y, deltaTime));
    }
}