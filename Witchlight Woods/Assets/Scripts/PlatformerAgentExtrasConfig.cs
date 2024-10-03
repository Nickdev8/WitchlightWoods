using System;
using UnityEngine;

namespace WitchlightWoods
{
    [Serializable]
    public class PlatformerAgentExtrasConfig
    {
        [Header("Movement Curves")]
        public AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public AnimationCurve decelerationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
        public AnimationCurve alteringDirectionCurve = AnimationCurve.EaseInOut(0, 0.8f, 1f, 1f);
        [Header("Jump Curves")]
        public AnimationCurve ascendMovementSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.2f);
        public AnimationCurve ascendGravityCurve = AnimationCurve.Constant(0, 1, 1);
        public AnimationCurve wallJumpControlCurve = AnimationCurve.Linear(0, 0, 1, 1);
    }
}